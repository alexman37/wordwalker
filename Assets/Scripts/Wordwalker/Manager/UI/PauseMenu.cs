using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// The pause menu functions similarly to the settings menu
public class PauseMenu : WidgetPopup
{
    // appearance
    public Sprite checkboxChecked;
    public Sprite checkboxUnchecked;
    public Image checkbox;
    public Slider musicVolSlider;
    public TextMeshProUGUI musicVolText;
    public Slider sfxVolSlider;
    public TextMeshProUGUI sfxVolText;
    public GameObject returnToMM;
    public GameObject returnConfirm;

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
    }

    void initializeValuesVisually()
    {
        adjustMusicVolSlider(settingsValues.musicVolume);
        adjustSfxVolSlider(settingsValues.sfxVolume);
        setInGameMusic(settingsValues.inGameMusic);
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

    public void attemptReturn()
    {
        returnToMM.SetActive(false);
        returnConfirm.SetActive(true);
    }

    public void backoffReturn()
    {
        returnToMM.SetActive(true);
        returnConfirm.SetActive(false);
    }

    public void exitToMainMenu()
    {
        GameManagerSc.returnToMainMenu();
    }

    public void loadSettings()
    {
        SettingsValues settings;
        try
        {
            StatMap stats = GlobalStatMap.loadGlobalStatMap();
            settings = stats.settingsValues;
        }
        catch (Exception e)
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
