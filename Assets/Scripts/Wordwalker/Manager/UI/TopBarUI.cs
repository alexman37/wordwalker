using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// Also known as CurrSpelling, this component tracks which letters the player stepped on to this point-
/// Eventually revealing the correct word at the end.
/// </summary>
public class TopBarUI : MonoBehaviour
{
    private Image progressBar;
    private Image answerBar;

    //For animation
    public Image container;
    private Vector2 topBarAnimationOffsite; // Relative to BOTTOM
    private Vector2 topBarAnimationStart; // Relative to BOTTOM
    private Vector2 topBarAnimationDestWin; // Relative to TOP
    private Vector2 topBarAnimationDestLose; // Relative to TOP

    // For spawning / tracking letters in the visualization
    public GameObject baseTileVis;
    private List<GameObject> currProgressVis;
    private List<GameObject> answerVis;

    // Potential colors for the tile
    private Color correct = new Color(65f / 255f, 133f / 255f, 65f / 255f);
    private Color golden = new Color(150f / 255f, 153f / 255f, 63f / 255f);
    private Color red = new Color(153f / 255f, 72f / 255f, 63f / 255f);
    private Color gray = new Color(100f / 255f, 100f / 255f, 100f / 255f);

    // How far apart are the tiles in post game? (Depends on their scaled size.)
    private float tileVisSize = 0.06f;

    // Start is called before the first frame update
    void Start()
    {
        progressBar = this.transform.GetChild(0).GetComponent<Image>();
        answerBar = this.transform.GetChild(1).GetComponent<Image>();

        // Set scroll "start" and "dest" positions when scaling finishes. Assume it wont take long
        // If done before we get a chance to subscribe to it just do so immediately
        ScalingUIComponent scalingComp = container.GetComponent<ScalingUIComponent>();
        Rect screenSpace = Screen.safeArea;

        container.GetComponent<ScalingUIComponent>().completedScaling += () =>
        {
            topBarAnimationStart = container.GetComponent<RectTransform>().anchoredPosition;
            topBarAnimationOffsite = topBarAnimationStart + new Vector2(0, container.rectTransform.rect.height);
            topBarAnimationDestWin = new Vector2(0, -50);
            topBarAnimationDestLose = new Vector2(0, -screenSpace.height * 0.08f - (screenSpace.yMax - (screenSpace.yMin + screenSpace.height)));
        };
        if (scalingComp.DONE)
        {
            topBarAnimationStart = container.GetComponent<RectTransform>().anchoredPosition;
            topBarAnimationOffsite = topBarAnimationStart + new Vector2(0, container.rectTransform.rect.height);
            topBarAnimationDestWin = new Vector2(0, -50);
            topBarAnimationDestLose = new Vector2(0, -screenSpace.height * 0.08f - (screenSpace.yMax - (screenSpace.yMin + screenSpace.height)));
        }

        baseTileVis = this.transform.GetChild(0).GetChild(0).gameObject;
        currProgressVis = new List<GameObject>();
        answerVis = new List<GameObject>();
    }

    private void OnEnable()
    {
        // Different positions depending on if you win or lose
        GameManagerSc.levelWon += postgameTransition;
        GameManagerSc.gameOver += gameOverTransition;
    }

    private void OnDisable()
    {
        GameManagerSc.levelWon -= postgameTransition;
        GameManagerSc.gameOver -= gameOverTransition;
    }


    /// <summary>
    /// Add a new letter to the progress bar (and adjust all existing ones)
    /// </summary>
    /// <param name="ch">New letter</param>
    public void AddLetterToProgress(char ch, char other)
    {
        // If this is the very first tile then we also do the initialization animation
        if(currProgressVis.Count == 0)
        {
            StartCoroutine(initializeAnimation());
        }

        // Spawn new tile, set the letter
        GameObject next = Instantiate(baseTileVis);
        next.SetActive(true);
        TextMeshProUGUI comp = next.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        comp.text = ch.ToString();
        currProgressVis.Add(next);

        // split tiles will pass in their actual other letter...
        if (other != ' ')
        {
            next.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.9f);
            comp.text = ch.ToString() + "\n" + other.ToString();
            comp.fontSize = comp.fontSize / 2f;
        }

        // Adjust position
        next.transform.SetParent(baseTileVis.transform.parent);
        next.transform.localScale = baseTileVis.transform.localScale;
        Readjust();
    }

    /// <summary>
    /// Readjust the topBar so all tiles are centered
    /// </summary>
    // TODO: Adjust/resize the black bar as well
    void Readjust()
    {
        progressBar.rectTransform.sizeDelta = new Vector2(progressBar.rectTransform.rect.width + tileVisSize * 2, 1);
        for(int i = 0; i < currProgressVis.Count; i++)
        {
            GameObject tile = currProgressVis[i];
            float x = (-tileVisSize * (currProgressVis.Count - 1)) + (tileVisSize * 2 * i);
            tile.GetComponent<RectTransform>().localPosition = new Vector3(x, 0f, -0.25f);
            tile.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, 0f, -0.25f);
        }
    }

    /// <summary>
    /// Clear all tiles previously used in progress bar (and delete their objects)
    /// </summary>
    public void ResetBar()
    {
        progressBar.rectTransform.sizeDelta = new Vector2(0, 1);
        foreach (GameObject tile in currProgressVis)
        {
            GameObject.Destroy(tile);
        }
        currProgressVis.Clear();

        foreach (GameObject tile in answerVis)
        {
            GameObject.Destroy(tile);
        }
        answerVis.Clear();

        this.transform.rotation = Quaternion.Euler(0, 0, 0);
        container.GetComponent<RectTransform>().anchoredPosition = topBarAnimationOffsite;
    }

    /// <summary>
    /// Set the answer bar when you either clear the level or lose the game
    /// Needs to be called externally by WalkManager- that's got the data on which tiles to draw.
    /// </summary>
    /// <param name="corrects"></param>
    /// <param name="won"></param>
    public void SetAnswer(List<Tile> corrects, bool won)
    {
        int addCoins = 0;
        int addTotems = 0;

        // Draw the answer made up of (1) correct tiles and (2) correct tiles you skipped
        foreach(Tile til in corrects)
        {
            // Skip blanks
            if (til.specType == Tile.SpecialTile.BLANK) continue;

            GameObject next = Instantiate(baseTileVis);
            next.SetActive(true);

            TextMeshProUGUI comp = next.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            comp.text = til.letter.ToString();
            Color col = determineAnswerColor(til, won);
            next.GetComponent<Image>().color = col;

            //prepare to add totems
            if (won)
            {
                addCoins += 1;
                if (col == golden) addTotems += 1;
            }

            next.transform.SetParent(this.transform.GetChild(1));
            next.transform.rotation = Quaternion.Euler(90, 0, 0);
            answerVis.Add(next);
        }

        // All tiles are added on the angle so that, preparing for the rotateReveal animation
        for (int i = 0; i < answerVis.Count; i++)
        {
            GameObject tile = answerVis[i];
            float x = (-tileVisSize * (answerVis.Count - 1)) + (tileVisSize * 2 * i);
            tile.GetComponent<RectTransform>().localPosition = new Vector3(x, 0, -0.25f);
            tile.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, 0f, -0.25f);
            tile.transform.localScale = baseTileVis.transform.localScale;
        }

        GameManagerSc.changeTotems(addTotems, true);
    }

    /// <summary>
    /// Spins the topBar around to reveal the correct answer.
    /// </summary>
    IEnumerator rotateReveal()
    {
        float steps = 30;
        float timeSec = 0.5f;

        for (float i = 0; i <= steps; i++)
        {
            this.transform.rotation = Quaternion.Euler(-90 * (i / steps), 0, 0);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return new WaitForSeconds(1);
    }

    private void postgameTransition()
    {
        StartCoroutine(postgameTransitionCo(1));
    }

    private void gameOverTransition(GameManagerSc.LossReason _)
    {
        StartCoroutine(gameOverTransitionCo(1));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator initializeAnimation()
    {
        float steps = 30;
        float timeSec = 0.5f;

        RectTransform rectTransform = container.GetComponent<RectTransform>();

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(topBarAnimationOffsite,
                    topBarAnimationStart,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }
    }

    /// <summary>
    /// Moves topBar down to the postgame position
    /// </summary>
    // TODO might differentiate from gameOver in other ways.
    IEnumerator postgameTransitionCo(float delay)
    {
        yield return new WaitForSeconds(delay);

        float steps = 30;
        float timeSec = 0.5f;

        RectTransform rectTransform = container.GetComponent<RectTransform>();

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(topBarAnimationStart,
                    topBarAnimationDestWin,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return new WaitForSeconds(1);
    }

    /// <summary>
    /// Moves topBar down to the postgame position
    /// </summary>
    // TODO might differentiate from gameOver in other ways.
    IEnumerator gameOverTransitionCo(float delay)
    {
        yield return new WaitForSeconds(delay);

        float steps = 30;
        float timeSec = 0.5f;

        RectTransform rectTransform = container.GetComponent<RectTransform>();

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(topBarAnimationStart,
                    topBarAnimationDestLose,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return new WaitForSeconds(1);
    }

    /// <summary>
    /// Begin rotate reveal animation
    /// This also has to be called externally (WalkManager) so it goes after the answer has been set.
    /// </summary>
    public void kickOffRotation()
    {
        StartCoroutine(rotateReveal());
    }

    /// <summary>
    /// When drawing answer tiles it'll either be:
    /// 
    /// Green (stepped on), Gold (skipped!), Gray (missed because you died)
    /// </summary>
    /// <param name="til"></param>
    /// <param name="won"></param>
    /// <returns></returns>
    /// //TODO: Red for the tile you failed to step on.
    private Color determineAnswerColor(Tile til, bool won)
    {
        if (til.stepped)
        {
            return this.correct;
        }
        else
        {
            if (won)
            {
                //Hey, you found a totem, congratulations
                return golden;
            }
            else
            {
                return gray;
            }
        }
    }
}
