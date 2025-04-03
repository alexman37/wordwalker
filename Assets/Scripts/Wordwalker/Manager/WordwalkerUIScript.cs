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
    public GameObject inventory;
    public GameObject postgame;
    public GameObject gameOver;

    //animating
    private float postgameAnimationTime = 0.5f;
    private float postgameAnimationCurr = 0f;
    private bool animating = false;
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
        /*critStats.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.05f, 0.05f), ScalingUIComponent.Position.TOP_LEFT);
        topBar.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0f, 0.2f), ScalingUIComponent.Position.BOTTOM);
        scrollClue.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0f, 0.05f), ScalingUIComponent.Position.BOTTOM);
        debugRegen.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.05f, 0.05f), ScalingUIComponent.Position.TOP_RIGHT);
        inventory.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.1f, 0.1f), ScalingUIComponent.Position.BOTTOM_LEFT);
        postgame.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0, -1f), ScalingUIComponent.Position.BOTTOM);*/

        postgameAnimationStart = postgame.transform.localPosition;
        postgameAnimationDest = postgame.transform.localPosition + new Vector3(0, Screen.safeArea.yMax / 1.5f, 0);
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
        postgameAnimationCurr = 0f;
        animating = true;
        StartCoroutine(fadeClueScroll());
    }

    public void ResetPostgamePosition()
    {
        StopCoroutine(fadeClueScroll());
        postgame.transform.localPosition = postgameAnimationStart;
        topBar.transform.localPosition = topBarAnimationStart;

        Color col = scrollClue.GetComponent<Image>().color;
        scrollClue.GetComponent<Image>().color = new Color(col.r, col.g, col.b, 1);
        scrollClue.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
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

        Color col = scrollClue.GetComponent<Image>().color;
        scrollClue.GetComponent<Image>().color = new Color(col.r, col.g, col.b, 1);
        scrollClue.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if(animating)
        {
            postgameAnimationCurr += Time.deltaTime;

            postgame.transform.localPosition = Vector3.Lerp(postgameAnimationStart,
                    postgameAnimationDest,
                    postgameAnimationCurr / postgameAnimationTime);

            topBar.transform.localPosition = Vector3.Lerp(topBarAnimationStart,
                    topBarAnimationDest,
                    postgameAnimationCurr / postgameAnimationTime);

            if (postgameAnimationCurr >= postgameAnimationTime)
            {
                animating = false;
            }
        }
    }

    //TODO eventually we might have a burning animation for this
    IEnumerator fadeClueScroll()
    {
        float frameTime = 30;
        Image img = scrollClue.GetComponent<Image>();
        TextMeshProUGUI text = scrollClue.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        Color col = img.color;

        text.color = new Color(0, 0, 0, 0);

        for (float i = 0; i <= frameTime; i++)
        {
            img.color = new Color(col.r, col.g, col.b, (frameTime - i) / frameTime);
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
