using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// The popup that appears when you win a round.
/// </summary>
public class PostgameUI : MonoBehaviour
{
    public TextMeshProUGUI definitionReadout;

    //animating - gotta know the right positions
    private Vector2 postgameAnimationStart;
    private Vector2 postgameAnimationDest;
    private RectTransform rectTransform;

    // Score sheet
    public RectTransform scoreSheet;
    public TextMeshProUGUI timeDisp;
    public TextMeshProUGUI mistakesDisp;
    public TextMeshProUGUI mistakesPenalty;
    public TextMeshProUGUI scoreDisp;

    // Alt spellings
    private bool usingAltSpellings = false;
    public RectTransform altSpellings;
    public TextMeshProUGUI alternateSpellingsReadout;

    public bool usingComp = true;
    private bool canUseButtons = false;

    public AnimationManager animationManager;

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

    // Click the "next" button.
    public void goToNextLevel()
    {
        // Only allowed to click guy this once
        if(canUseButtons)
        {
            canUseButtons = false;
            animationManager.startWalkingToNextLevel();
        }
    }

    // Setup alternative spellings, postgame defn, etc.
    private void prepareWord(WordGen.Word word)
    {
        if(word.alternateSpellings != null && word.alternateSpellings.Length > 0)
        {
            useAlternateSpellings(word.alternateSpellings);
        }
        if (word.definition != null && word.definition != "")
        {
            setDefinition(word.definition);
        }
        else
        {
            // TODO maybe we want the specific clue you got...
            setDefinition(word.clues[0].clue);
        }
    }

    public void useAlternateSpellings(string[] altSpellings)
    {
        usingAltSpellings = true;
        alternateSpellingsReadout.text = " *Also spelled as";
        foreach(string spelling in altSpellings)
        {
            alternateSpellingsReadout.text = alternateSpellingsReadout.text + "\n" + "   - " + spelling;
        }
    }

    public void setDefinition(string def)
    {
        definitionReadout.text = def;
    }

    public void enableComp() { usingComp = true; }
    public void disableComp() { usingComp = false; }

    private void OnEnable()
    {
        GameManagerSc.wordPrepared += prepareWord;
        GameManagerSc.levelWon += BeginPostgameAnimation;
        GameManagerSc.levelReset += postgameReset;
        GameManagerSc.updatePostgameScoreSheet += setScoreSheetDisplay;
        GameManagerSc.onLastLevel += disableComp;
        GameManagerSc.newGame += enableComp;
    }

    private void OnDisable()
    {
        GameManagerSc.wordPrepared -= prepareWord;
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

        usingAltSpellings = false; //it's rare, so we assume you don't use these.
        altSpellings.anchoredPosition = Vector2.zero;
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

        if(usingAltSpellings)
        {
            Vector2 altSpellingStart = Vector2.zero;
            Vector2 altSpellingEnd = new Vector2(0, -altSpellings.rect.height);
            for (float i = 0; i <= frameTime; i++)
            {
                altSpellings.anchoredPosition = UIUtils.XerpStandard(altSpellingStart,
                        altSpellingEnd,
                        i / frameTime);

                yield return new WaitForSeconds(1 / frameTime * timeSec);
            }
        }

        canUseButtons = true;
    }
}
