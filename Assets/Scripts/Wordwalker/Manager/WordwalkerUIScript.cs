using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WordwalkerUIScript : MonoBehaviour
{
    public static bool greenlight = false;

    public GameObject critStats;
    public GameObject topBar;
    public GameObject scrollClue;
    public Animator scrollAnimator;
    public GameObject clueBox;
    public Animator clueBoxAnimator;
    public GameObject inventory;
    public GameObject postgame;
    public GameObject gameOver;

    //animating - gotta know the right positions
    private Vector3 scrollStart;
    private Vector3 scrollDest;
    private Vector3 postgameAnimationStart;
    private Vector3 postgameAnimationDest;
    private Vector3 topBarAnimationStart;
    private Vector3 topBarAnimationDest;

    // Critical stat fields
    private TextMeshProUGUI displayScore;
    private TextMeshProUGUI displayRoom;
    private TextMeshProUGUI displayTotem;
    private int numLevels;

    //TODO: not in final product
    public GameObject debugRegen;

    // Start is called before the first frame update
    void Start()
    {
        scrollStart = scrollClue.GetComponent<RectTransform>().anchoredPosition;
        scrollDest = new Vector3(0, 0, 0); //relative to bottom of screen
        postgameAnimationStart = postgame.transform.localPosition;
        postgameAnimationDest = new Vector3(0, Screen.safeArea.yMax * -0.5f, 0);
        topBarAnimationStart = topBar.transform.localPosition;
        topBarAnimationDest = new Vector3(0, postgameAnimationDest.y + postgame.GetComponent<RectTransform>().rect.height / 2, 0);

        displayScore = critStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        displayRoom = critStats.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        displayTotem = critStats.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();

        TopBarUI.readyForPostgameAnimation += BeginPostgameAnimation;
        TopBarUI.readyForPostgameAnimation += BeginGameOverAnimation;
        WalkManager.openedScroll += moveScrollOnStart;
        GameManagerSc.levelWon += () => { StartCoroutine(moveScrollOutOfView()); };

        greenlight = true;
    }

    public void SetLevelAmount(int amnt)
    {
        displayRoom.text = "0 / " + amnt.ToString();
        numLevels = amnt;
    }

    public void SetNewRoom(int nextLvl)
    {
        displayRoom.text = nextLvl.ToString() + " / " + numLevels;
        if(nextLvl == 10)
        {
            displayRoom.transform.localPosition = displayRoom.transform.localPosition + new Vector3(10, 0, 0);
        }
    }

    public void ChangeScore(int newAmnt, int delta, bool adding)
    {
        displayScore.text = newAmnt.ToString();
    }

    public void ChangeTotems(int newAmnt, int delta, bool adding)
    {
        displayTotem.text = newAmnt.ToString();
    }

    private void BeginPostgameAnimation()
    {
        StartCoroutine(postgameAnimation());
        StartCoroutine(fadeClueScroll());
    }

    public void ResetPostgamePosition()
    {
        StopCoroutine(fadeClueScroll());
        postgame.transform.localPosition = postgameAnimationStart;
        topBar.transform.localPosition = topBarAnimationStart;

        Color colS = scrollClue.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(colS.r, colS.g, colS.b, 0);
        Color col = clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);
    }

    private void BeginGameOverAnimation()
    {
        StartCoroutine(gameOverAnimation());
        StartCoroutine(fadeClueScroll());
    }

    public void ResetGameOverPosition()
    {
        StopCoroutine(fadeClueScroll());
        gameOver.transform.localPosition = postgameAnimationStart;
        topBar.transform.localPosition = topBarAnimationStart;

        Color col = clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);
    }

    void moveScrollOnStart()
    {
        StartCoroutine(moveScrollIntoView());
    }

    //TODO eventually we might have a burning animation for this
    IEnumerator fadeClueScroll()
    {
        float frameTime = 30;
        TextMeshProUGUI text = clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        Color col = text.color;

        for (float i = 0; i <= frameTime; i++)
        {
            text.color = new Color(col.r, col.g, col.b, (frameTime - i) / frameTime);
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator postgameAnimation()
    {
        Debug.Log(postgameAnimationStart);
        Debug.Log(postgameAnimationDest);
        float frameTime = 30;
        float timeSec = 1f;

        for (float i = 0; i <= frameTime; i++)
        {
            postgame.transform.localPosition = UIUtils.XerpStandard(postgameAnimationStart,
                    postgameAnimationDest,
                    i / frameTime);

            postgame.transform.localPosition = UIUtils.XerpStandard(topBarAnimationStart,
                    topBarAnimationDest,
                    i / frameTime);

            yield return new WaitForSeconds(1 / frameTime * timeSec);
        }
    }

    IEnumerator gameOverAnimation()
    {
        float frameTime = 30;

        for (float i = 0; i <= frameTime; i++)
        {
            gameOver.transform.localPosition = Vector3.Lerp(postgameAnimationStart,
                    postgameAnimationDest,
                    i / frameTime);

            topBar.transform.localPosition = Vector3.Lerp(topBarAnimationStart,
                    topBarAnimationDest,
                    i / frameTime);
            
            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator moveScrollIntoView()
    {
        RectTransform scrollRect = scrollClue.GetComponent<RectTransform>();

        float steps = 30;
        float timeSec = 1f;

        for (float i = 0; i <= steps; i++)
        {
            scrollRect.anchoredPosition = UIUtils.XerpStandard(scrollStart, scrollDest, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        scrollAnimator.SetTrigger("BeginUnfurl");
        TextMeshProUGUI texts = scrollRect.GetComponentInChildren<TextMeshProUGUI>();
        Color col = texts.color;
        for (float i = 0; i <= steps; i++)
        {
            texts.color = new Color(col.r, col.g, col.b, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }
        clueBoxAnimator.SetTrigger("gotoNextPage");

        yield return null;
    }

    IEnumerator moveScrollOutOfView()
    {
        RectTransform scrollRect = scrollClue.GetComponent<RectTransform>();

        float steps = 30;
        float timeSec = 1f;

        for (float i = 0; i <= steps; i++)
        {
            scrollRect.anchoredPosition = UIUtils.XerpStandard(scrollDest, scrollStart, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return null;
    }
}
