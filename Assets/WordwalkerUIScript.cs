using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordwalkerUIScript : MonoBehaviour
{
    public GameObject critStats;
    public GameObject topBar;
    public GameObject scrollClue;
    public GameObject inventory;
    public GameObject postgame;

    //animating
    private float postgameAnimationTime = 0.5f;
    private float postgameAnimationCurr = 0f;
    private bool animating = false;
    private Vector3 postgameAnimationStart;
    private Vector3 postgameAnimationDest;
    private Vector3 topBarAnimationStart;
    private Vector3 topBarAnimationDest;

    private TextMeshProUGUI displayRoom;
    private TextMeshProUGUI displayOutOf;
    private TextMeshProUGUI displayCoins;
    private TextMeshProUGUI displayTotem;

    //TODO: not in final product
    public GameObject debugRegen;

    // Start is called before the first frame update
    void Start()
    {
        critStats.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.05f, 0.05f), ScalingUIComponent.Position.TOP_LEFT);
        topBar.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0f, 0.1f), ScalingUIComponent.Position.TOP);
        scrollClue.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0f, 0.05f), ScalingUIComponent.Position.BOTTOM);
        debugRegen.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.05f, 0.05f), ScalingUIComponent.Position.TOP_RIGHT);
        inventory.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.1f, 0.1f), ScalingUIComponent.Position.BOTTOM_LEFT);
        postgame.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0, -1f), ScalingUIComponent.Position.TOP);

        postgameAnimationStart = postgame.transform.localPosition;
        postgameAnimationDest = postgame.transform.localPosition - new Vector3(0, Screen.safeArea.yMax / 1.5f, 0);
        topBarAnimationStart = topBar.transform.localPosition;
        topBarAnimationDest = new Vector3(0, postgameAnimationDest.y - postgame.GetComponent<RectTransform>().rect.height / 2, 0);

        displayRoom = critStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        displayOutOf = critStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        displayCoins = critStats.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        displayTotem = critStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        TopBarUI.readyForPostgameAnimation += BeginPostgameAnimation;
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

    public void ChangeCoins(int amount, bool adding)
    {

    }

    public void ChangeTotems(int amount, bool adding)
    {

    }

    private void BeginPostgameAnimation()
    {
        postgameAnimationCurr = 0f;
        animating = true;
    }

    public void ResetPostgamePosition()
    {
        postgame.transform.localPosition = postgameAnimationStart;
        topBar.transform.localPosition = topBarAnimationStart;
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
}
