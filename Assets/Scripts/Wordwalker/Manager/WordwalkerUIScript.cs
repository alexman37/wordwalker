using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all UI in the game- or at least, modifying them and kicking off animations
/// </summary>
public class WordwalkerUIScript : MonoBehaviour
{
    public static bool greenlight = false;

    public GameObject critStats;         // Score, Room # and totems
    public GameObject clueBox;           // The book used for image clues
    public Animator clueBoxAnimator;          // Animation component
    public GameObject inventory;         // Inventory menu

    public Sprite[] critStatsOptions;
    public GameObject specialGuide;
    public GameObject specialGuidePopup;

    private Vector2 specAnimationOffsite;
    private Vector2 specAnimationStart;

    // Critical stat fields
    private TextMeshProUGUI displayScore;
    private TextMeshProUGUI displayRoom;
    private TextMeshProUGUI displayTotem;
    public Image totemPicture;
    public Sprite totemsNormal;
    public Sprite totemsIronman;
    public Sprite totemsDead;
    private Sprite thisGamesDefaultTotem; // either normal or ironman depending on challenges you'd selected
    private int numLevels;

    // The RankBox is also a part of stats
    private RankBox rankBox;

    // The timer needs constant updates when timer challenge is enabled
    public GameObject timer;
    public Image timerVis;
    public Sprite[] timerSprites;
    public TextMeshProUGUI timeDisplay;

    //TODO: not in final product
    public GameObject debugRegen;


    // Start is called before the first frame update
    void Start()
    {
        displayScore = critStats.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        displayRoom = critStats.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        displayTotem = critStats.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        rankBox = critStats.transform.GetChild(0).GetChild(1).GetComponent<RankBox>();

        specAnimationOffsite = new Vector2(0, -Screen.safeArea.height);
        specAnimationStart = new Vector2(0, 0);

        // Have to set how many totems given on game start.
        displayTotem.text = GameManagerSc.getNumTotems().ToString();

        if(GameManagerSc.state.selectedChallenges.Contains(MenuScript.Challenge.TIMER))
        {
            timer.SetActive(true);
        }

        if(GameManagerSc.state.selectedChallenges.Contains(MenuScript.Challenge.SPECIAL_TILES))
        {
            critStats.GetComponent<Image>().sprite = critStatsOptions[1];
            specialGuide.SetActive(true);
        } else
        {
            critStats.GetComponent<Image>().sprite = critStatsOptions[0];
        }

        if(GameManagerSc.state.selectedChallenges.Contains(MenuScript.Challenge.IRON_MAN))
        {
            thisGamesDefaultTotem = totemsIronman;
            totemPicture.sprite = totemsIronman;
        } else
        {
            thisGamesDefaultTotem = totemsNormal;
        }

        // We can set the number of levels here since gameManager is guaranteed to have its state set up by now
        SetLevelAmount(GameManagerSc.state.getNumLevels());

        Debug.Log("Wordwalker UI READY");
        greenlight = true;
    }

    private void OnEnable()
    {
        TimeManager.secondChanged += setTimerDisplay;
        TimeManager.activationChange += shiftTimer;
    }

    private void OnDisable()
    {
        TimeManager.secondChanged -= setTimerDisplay;
        TimeManager.activationChange -= shiftTimer;
    }

    // Set how many levels there will be in the game
    private void SetLevelAmount(int amnt)
    {
        displayRoom.text = "0 / " + amnt.ToString();
        numLevels = amnt;

        // Also set up the ranking system based on this
        rankBox.setupRankingSystem(amnt);
    }

    // When we enter a new room update the level counter
    public void SetNewRoom(int nextLvl)
    {
        displayRoom.text = nextLvl.ToString() + " / " + numLevels;
        if(nextLvl == 10)
        {
            displayRoom.transform.localPosition = displayRoom.transform.localPosition + new Vector3(10, 0, 0);
        }
    }

    // Update score
    public void ChangeScore(int oldAmnt, int delta, bool adding)
    {
        StartCoroutine(steadyNumberIncrease(1f, 0.5f, oldAmnt, delta));
    }

    // Update rank box
    public int GetNewRank(int numMistakes, int levelsToGo)
    {
        return rankBox.determineNewRank(numMistakes, levelsToGo);
    }

    public void AwardGoldStar()
    {
        rankBox.awardGoldStar();
    }

    public void withNewState(WWGameState state)
    {
        displayScore.text = state.getScore().ToString();
        displayTotem.text = state.getNumTotems().ToString();
        displayRoom.text = state.getCurrentLevel().ToString() + " / " + numLevels;
        rankBox.defaultRank();
    }

    // Number go up
    IEnumerator steadyNumberIncrease(float takeTime, float delay, int oldAmnt, int delta)
    {
        yield return new WaitForSeconds(delay);

        for (float i = 0; i <= takeTime; i += Time.deltaTime)
        {
            displayScore.text = ((int)(oldAmnt + (delta * Mathf.Clamp(i / takeTime, 0f, 1f)))).ToString();

            yield return null;
        }
        displayScore.text = (oldAmnt + delta).ToString();
    }

    public void ChangeTotems(int newAmnt, int delta, bool adding)
    {
        if(newAmnt >= 0)
        {
            displayTotem.text = newAmnt.ToString();
            totemPicture.sprite = thisGamesDefaultTotem;
        }
        else
        {
            displayTotem.text = "X";
            totemPicture.sprite = totemsDead;
        }
    }

    private void shiftTimer(bool ontoScreen)
    {
        StartCoroutine(shiftTimerCo(ontoScreen));
    }

    IEnumerator shiftTimerCo(bool ontoScreen)
    {
        float timeSec = 1f;

        Vector2 onScreen = new Vector2(0, 0);
        Vector2 offScreen = new Vector2(-timer.GetComponent<RectTransform>().rect.width, 0);

        Vector2 to = ontoScreen ? onScreen : offScreen;
        Vector2 fro = ontoScreen ? offScreen : onScreen;

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            timer.GetComponent<RectTransform>().anchoredPosition = UIUtils.XerpStandard(fro, to, Mathf.Clamp(i / timeSec, 0f, 1f));
            yield return null;
        }

        yield return null;
    }

    void setTimerDisplay(int secs)
    {
        secs = (int)((float)secs % TimeManager.timeInterval);
        timerVis.sprite = timerSprites[secs];
        timeDisplay.text = secs.ToString();
    }
}
