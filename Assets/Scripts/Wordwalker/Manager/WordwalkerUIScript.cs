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
    private int numLevels;

    // The RankBox is also a part of stats
    private RankBox rankBox;

    // The timer needs constant updates when timer challenge is enabled
    public GameObject timer;
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

        if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.TIMER))
        {
            timer.SetActive(true);
        }

        if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.SPECIAL_TILES))
        {
            critStats.GetComponent<Image>().sprite = critStatsOptions[1];
            specialGuide.SetActive(true);
        } else
        {
            critStats.GetComponent<Image>().sprite = critStatsOptions[0];
        }

        Debug.Log("Wordwalker UI READY");
        greenlight = true;
    }

    private void OnEnable()
    {
        TimeManager.secondChanged += setTimerDisplay;
    }

    private void OnDisable()
    {
        TimeManager.secondChanged -= setTimerDisplay;
    }

    // Set how many levels there will be in the game
    public void SetLevelAmount(int amnt)
    {
        displayRoom.text = "0 / " + amnt.ToString();
        numLevels = amnt;
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

    // Update score (TODO: nicer animation?)
    public void ChangeScore(int oldAmnt, int delta, bool adding)
    {
        StartCoroutine(steadyNumberIncrease(1f, 0.5f, oldAmnt, delta));
        rankBox.determineNewRank(oldAmnt + delta);
    }

    // Number go up
    IEnumerator steadyNumberIncrease(float takeTime, float delay, int oldAmnt, int delta)
    {
        float steps = 10;

        for (float i = 0; i <= steps; i++)
        {
            displayScore.text = ((int)(oldAmnt + (delta * (i / steps)))).ToString();

            yield return new WaitForSeconds(1 / steps * takeTime);
        }
        displayScore.text = (oldAmnt + delta).ToString();
    }

    public void ChangeTotems(int newAmnt, int delta, bool adding)
    {
        displayTotem.text = newAmnt.ToString();
    }

    void setTimerDisplay(int secs)
    {
        string timeFormat = ":";
        if(secs < 10)
        {
            timeFormat = timeFormat + "0" + secs;
        } else
        {
            timeFormat = timeFormat + secs;
        }
        timeDisplay.text = timeFormat;
    }




    /// Special Tiles Guide
    public void openPopup()
    {
        StartCoroutine(openPopupCo());
    }

    IEnumerator openPopupCo()
    {
        float steps = 30;
        float timeSec = 0.5f;

        RectTransform rectTransform = specialGuidePopup.GetComponent<RectTransform>();

        Vector2 pos = rectTransform.anchoredPosition;

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(pos,
                    specAnimationStart,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }
    }

    public void closePopup()
    {
        StartCoroutine(closePopupCo());
    }

    IEnumerator closePopupCo()
    {
        float steps = 30;
        float timeSec = 0.5f;

        RectTransform rectTransform = specialGuidePopup.GetComponent<RectTransform>();

        Vector2 pos = rectTransform.anchoredPosition;

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(pos,
                    specAnimationOffsite,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }
    }
}
