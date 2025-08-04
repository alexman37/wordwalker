using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    //animating - gotta know the right positions
    public TextMeshProUGUI title;
    public TextMeshProUGUI definitionReadout;

    private Vector2 gameOverAnimationStart;
    private Vector2 gameOverAnimationDest;
    private RectTransform rectTransform;

    // Alt spellings
    private bool usingAltSpellings = false;
    public RectTransform altSpellings;
    public TextMeshProUGUI alternateSpellingsReadout;

    private bool canUseButtons = false;
    public GameObject orText;
    public GameObject retryButton;


    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        ScalingUIComponent scalingComp = GetComponent<ScalingUIComponent>();
        GetComponent<ScalingUIComponent>().completedScaling += () =>
        {
            gameOverAnimationStart = GetComponent<RectTransform>().anchoredPosition;
            gameOverAnimationDest = new Vector2(0, 0);
        };
        if (scalingComp.DONE)
        {
            gameOverAnimationStart = GetComponent<RectTransform>().anchoredPosition;
            gameOverAnimationDest = new Vector2(0, 0); //relative to bottom of screen
        }

        if(GameManagerSc.state.dailyWord)
        {
            retryButton.SetActive(false);
            orText.SetActive(false);
        }
    }

    // Setup alternative spellings, definition, etc.
    private void prepareWord(WordGen.Word word)
    {
        if (word.alternateSpellings != null && word.alternateSpellings.Length > 0)
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

    private void OnEnable()
    {
        GameManagerSc.wordPrepared += prepareWord;
        GameManagerSc.gameOver += BeginGameOverAnimation;
        GameManagerSc.levelReset += gameOverReset;
    }

    private void OnDisable()
    {
        GameManagerSc.wordPrepared -= prepareWord;
        GameManagerSc.gameOver -= BeginGameOverAnimation;
        GameManagerSc.levelReset -= gameOverReset;
    }

    public void retryHit()
    {
        if(canUseButtons)
        {
            canUseButtons = false;
            GameManagerSc.retry();
        }
    }

    public void quitHit()
    {
        if (canUseButtons)
        {
            canUseButtons = false;
            GameManagerSc.returnToMainMenu();
        }
    }

    public void useAlternateSpellings(string[] altSpellings)
    {
        usingAltSpellings = true;
        alternateSpellingsReadout.text = " *Also spelled as";
        foreach (string spelling in altSpellings)
        {
            alternateSpellingsReadout.text = alternateSpellingsReadout.text + "\n" + "   - " + spelling;
        }
    }

    public void setDefinition(string def)
    {
        definitionReadout.text = def;
    }

    void gameOverReset()
    {
        if(rectTransform == null) rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.anchoredPosition = gameOverAnimationStart;

        usingAltSpellings = false; //it's rare, so we assume you don't use these.
        altSpellings.anchoredPosition = Vector2.zero;
    }

    private void BeginGameOverAnimation(GameManagerSc.LossReason lr)
    {
        //TODO maybe we have one of many messages?
        switch (lr)
        {
            case GameManagerSc.LossReason.TOTEMS: title.text = "Your last word was"; break;
            case GameManagerSc.LossReason.TIME: title.text = "You ran out of time spelling"; break;
            case GameManagerSc.LossReason.JUMP: title.text = "You went a bridge too far on"; break;
        }
        StartCoroutine(gameOverAnimation(1.5f));
    }

    IEnumerator gameOverAnimation(float delay)
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
                    gameOverAnimationDest,
                    i / frameTime);

            yield return new WaitForSeconds(1 / frameTime * timeSec);
        }

        if (usingAltSpellings)
        {
            Vector2 altSpellingStart = Vector2.zero;
            Vector2 altSpellingEnd = new Vector2(altSpellings.rect.width, 0);
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
