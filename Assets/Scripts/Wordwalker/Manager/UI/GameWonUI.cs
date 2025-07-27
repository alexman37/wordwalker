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
    public TextMeshProUGUI commentary;
    public Image finalRankSprite;

    // These change depending on your rank. Make sure there is one per each rank.
    public string[] commentaryLines;

    public RankBox rankBox; // you need this to get the proper sprite.

    public AudioClip tada;
    public AudioClip applause;

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

            // Get a random fun stat to display
            (string n, string v) funStatInputs = FunStatUI.getFunStat();

            // Add however many periods you can to reach the "just before size" quota
            float width = funStatName.GetComponent<RectTransform>().rect.width;
            int funStatNameLength = funStatInputs.n.Length;

            string working = funStatInputs.n + funStatInputs.v;
            for (float i = funStatName.preferredWidth; i < width; i = funStatName.preferredWidth)
            {
                working = working.Substring(0, funStatNameLength) + "." + working.Substring(funStatNameLength);
                funStatName.text = working;
                i = funStatName.preferredWidth;
            }

            funStatName.text = working.Substring(0, funStatNameLength) + working.Substring(funStatNameLength + 1);

            // GameManagerSc may not do the calculation in time so we'll just do it here
            int trueRank = GameManagerSc.getRank() == 13 && GameManagerSc.state.selectedChallenges.Count == 5 ? 14 : GameManagerSc.getRank();
            finalRankSprite.sprite = rankBox.getRankAsSprite(trueRank);
            commentary.text = commentaryLines[trueRank];

            SfxManager.instance.playSFX(tada, null, 1);
            if(trueRank == 14) SfxManager.instance.playSFX(applause, null, 1);

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
}
