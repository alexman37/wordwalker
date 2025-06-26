using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WidgetPopup : MonoBehaviour
{
    public static string activeWidget = null;
    public string widgetId;

    private RectTransform rectTransform;
    IEnumerator movingCoroutineIn;
    IEnumerator movingCoroutineOut;

    public void Setup()
    {
        rectTransform = GetComponent<RectTransform>();
        movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, 0));
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, -Screen.safeArea.height));
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
        }
    }

    public void closeWidgetPopup()
    {
        StopCoroutine(movingCoroutineIn);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, -Screen.safeArea.height));
        StartCoroutine(movingCoroutineOut);
        activeWidget = null;
    }
}
