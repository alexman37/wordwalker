using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : WidgetPopup
{
    // appearance
    public Sprite checkboxChecked;
    public Sprite checkboxUnchecked;
    public Image checkbox;
    public Image[] screenOrientationGroupImgs;
    public Button[] screenOrientationGroup;
    public Image[] screenSizeGroupImgs;
    public Button[] screenSizeGroup;
    public Slider musicVolSlider;
    public TextMeshProUGUI musicVolText;
    public Slider sfxVolSlider;
    public TextMeshProUGUI sfxVolText;
    public GameObject clearData;
    public GameObject resetConfirmation;

    // stat
    public SettingsValues settingsValues = new SettingsValues();

    // global events
    public static event Action<bool> toggledInGameMusic;
    public static event Action<float> toggledMusicVol;
    public static event Action<float> toggledSfxVol;
    public static event Action<ScreenOrientationSetting> toggledScreenOr;
    public static event Action<ScreenSizeSetting> toggledScreenSize;

    private bool readyForSFX = false;

    // Start is called before the first frame update
    void Start()
    {
        toggledInGameMusic += (_) => { };
        toggledMusicVol += (_) => { };
        toggledSfxVol += (_) => { };
        toggledScreenOr += (_) => { };
        toggledScreenSize += (_) => { };

        this.Setup();
        loadSettings();
        initializeValuesVisually();

        musicVolSlider.onValueChanged.AddListener(adjustMusicVolSlider);
        sfxVolSlider.onValueChanged.AddListener(adjustSfxVolSlider);
        screenOrientationGroup[0].onClick.AddListener(() => selectScreenOrientation(ScreenOrientationSetting.LEFT));
        screenOrientationGroup[1].onClick.AddListener(() => selectScreenOrientation(ScreenOrientationSetting.TOP));
        screenOrientationGroup[2].onClick.AddListener(() => selectScreenOrientation(ScreenOrientationSetting.BOTTOM));

        screenSizeGroup[0].onClick.AddListener(() => selectScreenSize(ScreenSizeSetting.MAX));
        screenSizeGroup[1].onClick.AddListener(() => selectScreenSize(ScreenSizeSetting.SMALL_WINDOW));
        screenSizeGroup[2].onClick.AddListener(() => selectScreenSize(ScreenSizeSetting.BIG_WINDOW));
    }

    private void OnEnable()
    {
        // TODO: It'd be nice if we could keep the settings window open but the way scaling components are set up, it'd be difficult.
        //ScreenResizeComponent.resizeScalingComponents += onResizeCloseWidget;
    }

    private void OnDisable()
    {
        //ScreenResizeComponent.resizeScalingComponents -= onResizeCloseWidget;
    }

    private void onResizeCloseWidget(float _, float newHeight)
    {
        //base.closeWidgetPopup();
    }

    void initializeValuesVisually()
    {
        adjustMusicVolSlider(settingsValues.musicVolume);
        adjustSfxVolSlider(settingsValues.sfxVolume);
        setInGameMusic(settingsValues.inGameMusic);
        selectScreenOrientation(settingsValues.screenOrientationSetting);
        selectScreenSize(settingsValues.screenSizeSetting);
        //selectScreenSize(ScreenSizeSetting.SMALL_WINDOW);

        readyForSFX = true;
    }

    public void toggleInGameMusic()
    {
        settingsValues.inGameMusic = !settingsValues.inGameMusic;
        GlobalStatMap.ModifySettings(settingsValues);
        setInGameMusic(settingsValues.inGameMusic);
    }

    public void setInGameMusic(bool val)
    {
        if (readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
        settingsValues.inGameMusic = val;
        if (settingsValues.inGameMusic)
        {
            checkbox.sprite = checkboxChecked;
        }
        else
        {
            checkbox.sprite = checkboxUnchecked;
        }
        GlobalStatMap.ModifySettings(settingsValues);
        toggledInGameMusic.Invoke(settingsValues.inGameMusic);
    }

    public void adjustMusicVolSlider(float newVal)
    {
        settingsValues.musicVolume = newVal;
        musicVolText.text = (newVal * 100f).ToString().Split('.')[0];
        musicVolSlider.value = newVal;

        toggledMusicVol.Invoke(newVal);
    }

    public void adjustSfxVolSlider(float newVal)
    {
        settingsValues.sfxVolume = newVal;
        sfxVolText.text = (newVal * 100f).ToString().Split('.')[0];
        sfxVolSlider.value = newVal;

        toggledSfxVol.Invoke(newVal);
    }

    public void selectScreenOrientation(ScreenOrientationSetting screenOr)
    {
        if(readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
        settingsValues.screenOrientationSetting = screenOr;
        foreach(Image button in screenOrientationGroupImgs)
        {
            button.color = new Color(0.3f, 0.3f, 0.3f, 1);
        }
        switch(screenOr)
        {
            case ScreenOrientationSetting.LEFT:
                screenOrientationGroupImgs[0].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
            case ScreenOrientationSetting.TOP:
                screenOrientationGroupImgs[1].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
            case ScreenOrientationSetting.BOTTOM:
                screenOrientationGroupImgs[2].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
        }

        GlobalStatMap.ModifySettings(settingsValues);
        toggledScreenOr.Invoke(screenOr);
    }

    // Screen size requires more work than the others
    public void selectScreenSize(ScreenSizeSetting screenSize)
    {
        if (readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
        settingsValues.screenSizeSetting = screenSize;
        foreach (Image button in screenSizeGroupImgs)
        {
            button.color = new Color(0.3f, 0.3f, 0.3f, 1);
        }
        switch (screenSize)
        {
            case ScreenSizeSetting.MAX:
                screenSizeGroupImgs[0].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
            case ScreenSizeSetting.SMALL_WINDOW:
                screenSizeGroupImgs[1].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
            case ScreenSizeSetting.BIG_WINDOW:
                screenSizeGroupImgs[2].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
        }

        GlobalStatMap.ModifySettings(settingsValues);
        if(readyForSFX) toggledScreenSize.Invoke(screenSize);
    }

    public void attemptReset()
    {
        if (readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
        clearData.SetActive(false);
        resetConfirmation.SetActive(true);
    }

    public void backoffReset()
    {
        clearData.SetActive(true);
        resetConfirmation.SetActive(false);
    }

    public void resetAllData()
    {
        if (readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
        DatabaseTracker.resetAllData();
        GlobalStatMap.resetAllData();
        clearData.SetActive(true);
        resetConfirmation.SetActive(false);
    }

    public void loadSettings()
    {
        SettingsValues settings;
        try
        {
            StatMap stats = GlobalStatMap.loadGlobalStatMap();
            settings = stats.settingsValues;
        } catch(Exception e)
        {
            Debug.LogWarning("Failed to load settings, using defaults");
            settings = new SettingsValues();
        }
        
        settingsValues = settings;
    }

    public void saveSettings()
    {
        GlobalStatMap.ModifySettings(settingsValues);
    }

}

[System.Serializable]
public class SettingsValues
{
    public float musicVolume;  // 0 - 1
    public float sfxVolume;  // 0 - 1
    public bool inGameMusic; // 0 - 1
    public ScreenOrientationSetting screenOrientationSetting;
    public ScreenSizeSetting screenSizeSetting;

    public SettingsValues(float m, float s, bool i, ScreenOrientationSetting so, ScreenSizeSetting ss)
    {
        musicVolume = m;
        sfxVolume = s;
        inGameMusic = i;
        screenOrientationSetting = so;
        screenSizeSetting = ss;
    }

    public SettingsValues()
    {
        musicVolume = 0.5f;
        sfxVolume = 0.5f;
        inGameMusic = false;
        screenOrientationSetting = ScreenOrientationSetting.LEFT;
        screenSizeSetting = ScreenSizeSetting.SMALL_WINDOW;
    }
}

public enum ScreenOrientationSetting
{
    LEFT,
    TOP,
    BOTTOM
}

public enum ScreenSizeSetting
{
    MAX,
    SMALL_WINDOW,
    BIG_WINDOW
}