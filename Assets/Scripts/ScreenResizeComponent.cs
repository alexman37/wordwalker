using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScreenResizeComponent : MonoBehaviour
{
    public static event Action<float, float> resizeScalingComponents;

    // Start is called before the first frame update
    void Start()
    {
        resizeScalingComponents += (_,__) => { };
    }

    private void OnEnable()
    {
        SettingsMenu.toggledScreenSize += screenSizeChanged;
    }

    private void OnDisable()
    {
        SettingsMenu.toggledScreenSize -= screenSizeChanged;
    }

    void screenSizeChanged(ScreenSizeSetting toSize)
    {
        float width = 1920;
        float height = 1080;
        Debug.Log("old safearea " + Screen.safeArea);
        switch(toSize)
        {
            case ScreenSizeSetting.MAX:
                Screen.fullScreen = true;
                int lastRes = Screen.resolutions.Length - 1;
                width = Screen.resolutions[lastRes].width;
                height = Screen.resolutions[lastRes].height;
                break;
            case ScreenSizeSetting.SMALL_WINDOW:
                Screen.SetResolution(1366, 768, false);
                width = 1366;
                height = 1080;
                break;
            case ScreenSizeSetting.BIG_WINDOW:
                Screen.SetResolution(1920, 1080, false);
                width = 1920;
                height = 1080;
                break;
        }
        Debug.Log("new safearea " + Screen.safeArea);

        Debug.Log("All res: ");
        foreach(Resolution r in Screen.resolutions)
        {
            Debug.Log(r);
        }

        resizeScalingComponents.Invoke(width, height);
    }
}
