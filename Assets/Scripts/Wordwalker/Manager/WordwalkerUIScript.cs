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
    private Vector3 postgameAnimationStart;
    private Vector3 postgameAnimationDest;
    private Vector3 topBarAnimationStart;
    private Vector3 topBarAnimationDest;

    // Critical stat fields
    private TextMeshProUGUI displayRoom;
    private TextMeshProUGUI displayOutOf;
    private TextMeshProUGUI displayCoins;
    private TextMeshProUGUI displayTotem;

    //TODO: not in final product
    public GameObject debugRegen;

    // Start is called before the first frame update
    void Start()
    {
        postgameAnimationStart = postgame.transform.localPosition;
        postgameAnimationDest = new Vector3(0, Screen.safeArea.yMax * 0.25f, 0);
        topBarAnimationStart = topBar.transform.localPosition;
        topBarAnimationDest = new Vector3(0, postgameAnimationDest.y + postgame.GetComponent<RectTransform>().rect.height / 2, 0);

        displayRoom = critStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        displayOutOf = critStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        displayCoins = critStats.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        displayTotem = critStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        GameManagerSc.levelWon += BeginPostgameAnimation;
        GameManagerSc.gameOver += BeginGameOverAnimation;

        greenlight = true;
    }

    public void SetLevelAmount(int amnt)
    {
        displayOutOf.text += amnt.ToString();
    }

    public void SetNewRoom(int nextLvl)
    {
        displayRoom.text = nextLvl.ToString();
        if(nextLvl == 10)
        {
            displayOutOf.transform.localPosition = displayOutOf.transform.localPosition + new Vector3(10, 0, 0);
        }
    }

    public void ChangeCoins(int newAmnt, int delta, bool adding)
    {
        displayCoins.text = newAmnt.ToString();
    }

    public void ChangeTotems(int newAmnt, int delta, bool adding)
    {
        displayTotem.text = newAmnt.ToString();
    }

    private void BeginPostgameAnimation()
    {
        postgame.transform.localPosition = postgameAnimationDest;
        StartCoroutine(fadeClueScroll());
    }

    public void ResetPostgamePosition()
    {
        StopCoroutine(fadeClueScroll());
        postgame.transform.localPosition = postgameAnimationStart;
        topBar.transform.localPosition = topBarAnimationStart;

        Color col = clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);
        scrollAnimator.SetTrigger("BeginUnfurl");
        clueBoxAnimator.SetTrigger("gotoNextPage");
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
}
