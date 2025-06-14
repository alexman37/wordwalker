using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWonUI : MonoBehaviour
{
    public RectTransform rectTransform;
    private Vector2 oldPosition;
    //public TextMeshProUGUI stats1; // TODO
    IEnumerator movingToScreen;
    public bool usingComp = false;

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

    public void enableComp() { usingComp = true; }
    public void disableComp() { usingComp = false; }

    public void openGameWon()
    {
        if(usingComp)
        {
            movingToScreen = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, 0));
            StartCoroutine(movingToScreen);
        }
    }

    public void closeGameWon()
    {
        StopCoroutine(movingToScreen);
        rectTransform.anchoredPosition = oldPosition;
    }
}
