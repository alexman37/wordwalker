using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WidgetPopup : MonoBehaviour
{
    public static string activeWidget = null;
    public string widgetId;
    public bool overrideOtherWidgets = false; // if true, this widget can open even when others are already opened

    private RectTransform rectTransform;
    IEnumerator movingCoroutineIn;
    IEnumerator movingCoroutineOut;

    

    public void Setup()
    {
        rectTransform = GetComponent<RectTransform>();
        movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, 0));
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, -Screen.safeArea.height));
    }

    // kind of inefficient to run this for every widget so...just call it when you need to
    public static void resetWidgets()
    {
        activeWidget = null;
    }

    public void openWidgetPopup()
    {
        // only allow one open widget at a time
        if(activeWidget == null)
        {
            activeWidget = widgetId;
            StopCoroutine(movingCoroutineOut);
            movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, 0));
            StartCoroutine(movingCoroutineIn);
            SfxManager.instance.playSFXbyName("slide", null, 1);
        }

        // similar story for widget overrides but it doesn't affect the already opened "active widget"
        else if(overrideOtherWidgets)
        {
            StopCoroutine(movingCoroutineOut);
            movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, 0));
            StartCoroutine(movingCoroutineIn);
        }

        // Player cannot move the map around when a widget is open...thems the rules
        if(PlayerManager.instance != null)
            PlayerManager.instance.setFreeCamera(false);
    }

    public void closeWidgetPopup()
    {
        StopCoroutine(movingCoroutineIn);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, -Screen.safeArea.height));
        StartCoroutine(movingCoroutineOut);
        SfxManager.instance.playSFXbyName("slide-away", null, 1);
        activeWidget = null;

        // Player can open widgets again
        if (PlayerManager.instance != null)
            PlayerManager.instance.setFreeCamera(true);
    }

    // Special use cases
    public void openWidgetPopupSafe()
    {
        StopCoroutine(movingCoroutineOut);
        StopCoroutine(movingCoroutineIn);
        Debug.Log("I think i called the openwidget safe.");
        StartCoroutine(waitForResize(0.75f));
        
        //rectTransform.anchoredPosition = new Vector2(0, 0);
        //movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, 0));
        //StartCoroutine(movingCoroutineIn);
    }

    // TODO we could definitely do something better than this to wait for the resize to finish...
    IEnumerator waitForResize(float sec)
    {
        yield return new WaitForSeconds(sec);
        rectTransform.anchoredPosition = new Vector2(0, 0);
    }

    public void closeWidgetPopupSafe()
    {
        StopCoroutine(movingCoroutineIn);
        rectTransform.anchoredPosition = new Vector2(0, -Screen.safeArea.height * 3f);
    }
}
