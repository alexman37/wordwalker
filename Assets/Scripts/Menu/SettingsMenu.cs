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
    public Image[] buttonGroupImgs;
    public Button[] buttonGroup;
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

    // Start is called before the first frame update
    void Start()
    {
        toggledInGameMusic += (_) => { };
        toggledMusicVol += (_) => { };
        toggledSfxVol += (_) => { };
        toggledScreenOr += (_) => { };

        this.Setup();
        loadSettings();
        initializeValuesVisually();

        musicVolSlider.onValueChanged.AddListener(adjustMusicVolSlider);
        sfxVolSlider.onValueChanged.AddListener(adjustSfxVolSlider);
        buttonGroup[0].onClick.AddListener(() => selectScreenOrientation(ScreenOrientationSetting.LEFT));
        buttonGroup[1].onClick.AddListener(() => selectScreenOrientation(ScreenOrientationSetting.TOP));
        buttonGroup[2].onClick.AddListener(() => selectScreenOrientation(ScreenOrientationSetting.BOTTOM));
    }

    void initializeValuesVisually()
    {
        adjustMusicVolSlider(settingsValues.musicVolume);
        adjustSfxVolSlider(settingsValues.sfxVolume);
        setInGameMusic(settingsValues.inGameMusic);
        selectScreenOrientation(settingsValues.screenOrientationSetting);
    }

    public void toggleInGameMusic()
    {
        settingsValues.inGameMusic = !settingsValues.inGameMusic;
        setInGameMusic(settingsValues.inGameMusic);
    }

    public void setInGameMusic(bool val)
    {
        settingsValues.inGameMusic = val;
        if (settingsValues.inGameMusic)
        {
            checkbox.sprite = checkboxChecked;
        }
        else
        {
            checkbox.sprite = checkboxUnchecked;
        }
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
        settingsValues.screenOrientationSetting = screenOr;
        foreach(Image button in buttonGroupImgs)
        {
            button.color = new Color(0.3f, 0.3f, 0.3f, 1);
        }
        switch(screenOr)
        {
            case ScreenOrientationSetting.LEFT:
                buttonGroupImgs[0].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
            case ScreenOrientationSetting.TOP:
                buttonGroupImgs[1].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
            case ScreenOrientationSetting.BOTTOM:
                buttonGroupImgs[2].color = new Color(0.55f, 0.5f, 0.2f, 1);
                break;
        }

        toggledScreenOr.Invoke(screenOr);
    }

    public void attemptReset()
    {
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
            settings = GlobalStatMap.loadGlobalStatMap().settingsValues;
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

    public SettingsValues(float m, float s, bool i, ScreenOrientationSetting so)
    {
        musicVolume = m;
        sfxVolume = s;
        inGameMusic = i;
        screenOrientationSetting = so;
    }

    public SettingsValues()
    {
        musicVolume = 0.5f;
        sfxVolume = 0.5f;
        inGameMusic = false;
        screenOrientationSetting = ScreenOrientationSetting.LEFT;
    }
}

public enum ScreenOrientationSetting
{
    LEFT,
    TOP,
    BOTTOM
}