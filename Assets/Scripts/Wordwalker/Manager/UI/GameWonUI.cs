using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameWonUI : MonoBehaviour
{
    public RectTransform rectTransform;
    private Vector2 oldPosition;
    //public TextMeshProUGUI stats1; // TODO
    IEnumerator movingToScreen;
    public bool usingComp = false;

    private bool canUseButtons = false;

    public TextMeshProUGUI timeTaken;
    public TextMeshProUGUI mistakes;
    public TextMeshProUGUI funStatName;
    public TextMeshProUGUI funStatValue;
    public TextMeshProUGUI commentary;
    public Image finalRankSprite;

    // These change depending on your rank. Make sure there is one per each rank.
    public string[] commentaryLines;

    public RankBox rankBox; // you need this to get the proper sprite.

    private void OnEnable()
    {
        GameManagerSc.levelWon += openGameWon;
        GameManagerSc.onLastLevel += enableComp;
        GameManagerSc.newGame += disableComp;
    }

    private void OnDisable()
    {
        GameManagerSc.levelWon -= openGameWon;
        GameManagerSc.onLastLevel -= enableComp;
        GameManagerSc.newGame -= disableComp;
    }

    void Start()
    {
        oldPosition = new Vector2(0, Screen.safeArea.height);
        rectTransform.anchoredPosition = oldPosition;
    }

    public void retryHit()
    {
        if (canUseButtons)
        {
            canUseButtons = false;
            closeGameWon();
            GameManagerSc.retry();
        }
    }

    public void exitHit()
    {
        if (canUseButtons)
        {
            canUseButtons = false;
            GameManagerSc.returnToMainMenu();
        }
    }

    public void enableComp() { usingComp = true; }
    public void disableComp() { usingComp = false; }

    // When you win the game you share the victory stats
    public void openGameWon()
    {
        if(usingComp)
        {
            // Set postgame stats
            timeTaken.text = secondsToMinSec(GameManagerSc.state.totalTime);
            mistakes.text = GameManagerSc.state.numMistakes.ToString();
            // fun stat name and value are set elsewhere
            finalRankSprite.sprite = rankBox.getRankAsSprite(GameManagerSc.getRank());
            commentary.text = commentaryLines[GameManagerSc.getRank()];

            movingToScreen = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, Screen.safeArea.height / 4f));
            StartCoroutine(movingToScreen);
            canUseButtons = true;
        }
    }

    public void closeGameWon()
    {
        StopCoroutine(movingToScreen);
        rectTransform.anchoredPosition = oldPosition;
    }


    // Utility - postgame formatting
    private string secondsToMinSec(int seconds)
    {
        float secs = (seconds % 60);
        return (seconds / 60) + ":" + (secs < 10 ? "0" : "") + secs;
    }

    // Given the name of the stat and its value (determined somewhere else) set them
    // TODO
    private string formatFunStatName(string statName)
    {
        return "";
    }
}
