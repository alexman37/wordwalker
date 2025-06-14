using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// The popup that appears when you win a round.
/// </summary>
public class PostgameUI : MonoBehaviour
{
    //animating - gotta know the right positions
    private Vector2 postgameAnimationStart;
    private Vector2 postgameAnimationDest;
    private RectTransform rectTransform;
    public RectTransform scoreSheet;

    // Score sheet
    public TextMeshProUGUI timeDisp;
    public TextMeshProUGUI mistakesDisp;
    public TextMeshProUGUI mistakesPenalty;
    public TextMeshProUGUI scoreDisp;

    public bool usingComp = true;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        ScalingUIComponent scalingComp = GetComponent<ScalingUIComponent>();
        GetComponent<ScalingUIComponent>().completedScaling += () =>
        {
            postgameAnimationStart = GetComponent<RectTransform>().anchoredPosition;
            postgameAnimationDest = new Vector2(0, 0);
        };
        if (scalingComp.DONE)
        {
            postgameAnimationStart = GetComponent<RectTransform>().anchoredPosition;
            postgameAnimationDest = new Vector2(0, 0); //relative to bottom of screen
        }
    }

    public void enableComp() { usingComp = true; }
    public void disableComp() { usingComp = false; }

    private void OnEnable()
    {
        GameManagerSc.levelWon += BeginPostgameAnimation;
        GameManagerSc.levelReset += postgameReset;
        GameManagerSc.updatePostgameScoreSheet += setScoreSheetDisplay;
        GameManagerSc.onLastLevel += disableComp;
        GameManagerSc.newGame += enableComp;
    }

    private void OnDisable()
    {
        GameManagerSc.levelWon -= BeginPostgameAnimation;
        GameManagerSc.levelReset -= postgameReset;
        GameManagerSc.updatePostgameScoreSheet -= setScoreSheetDisplay;
        GameManagerSc.onLastLevel -= disableComp;
        GameManagerSc.newGame -= enableComp;
    }

    void postgameReset()
    {
        if(rectTransform == null) rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.anchoredPosition = postgameAnimationStart;
        scoreSheet.anchoredPosition = new Vector2(0, 250);
    }

    private void setScoreSheetDisplay(int timeSecondsTaken, int mistakes, int penalty, int scoreChange)
    {
        int seconds = (timeSecondsTaken % 60);
        timeDisp.text = (timeSecondsTaken / 60) + ":" + (seconds < 10 ? "0" + seconds : seconds.ToString());
        mistakesDisp.text = mistakes.ToString();
        mistakesPenalty.text = "-" + penalty;
        if (penalty > 0) mistakesPenalty.color = Color.red; else mistakesPenalty.color = Color.white;
        scoreDisp.text = (scoreChange >= 0 ? "+" : "-") + scoreChange.ToString();
    }

    private void BeginPostgameAnimation()
    {
        if(usingComp)
        {
            StartCoroutine(postgameAnimation(1.5f));
        }
    }

    IEnumerator postgameAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        float frameTime = 30;
        float timeSec = 1f;
        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        Vector2 adjustedStart = rectTransform.anchoredPosition;

        for (float i = 0; i <= frameTime; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(adjustedStart,
                    postgameAnimationDest,
                    i / frameTime);

            yield return new WaitForSeconds(1 / frameTime * timeSec);
        }

        Vector2 scoreSheetStart = new Vector2(0, 250);
        Vector2 scoreSheetEnd = new Vector2(0, 0);
        for (float i = 0; i <= frameTime; i++)
        {
            scoreSheet.anchoredPosition = UIUtils.XerpStandard(scoreSheetStart,
                    scoreSheetEnd,
                    i / frameTime);

            yield return new WaitForSeconds(1 / frameTime * timeSec);
        }
    }
}
