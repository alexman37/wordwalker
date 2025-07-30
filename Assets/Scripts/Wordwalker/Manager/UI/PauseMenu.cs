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
    public Image[] buttonGroupImgs;
    public Button[] buttonGroup;
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

    private bool readyForSFX = false;

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
        readyForSFX = true;
    }

    public void toggleInGameMusic()
    {
        if (readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
        settingsValues.inGameMusic = !settingsValues.inGameMusic;
        GlobalStatMap.ModifySettings(settingsValues);
        setInGameMusic(settingsValues.inGameMusic);
    }

    public void setInGameMusic(bool val)
    {
        settingsValues.inGameMusic = val;
        Debug.Log("AAA Set in-game music to " + val);
        if (settingsValues.inGameMusic)
        {
            Debug.Log("AAA Trying to fade in");
            checkbox.sprite = checkboxChecked;
            MusicManager.inGameMusicFade(false);
        }
        else
        {
            checkbox.sprite = checkboxUnchecked;
            MusicManager.inGameMusicFade(true);
        }
        toggledInGameMusic.Invoke(settingsValues.inGameMusic);
        GlobalStatMap.ModifySettings(settingsValues);
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
        if (readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
        settingsValues.screenOrientationSetting = screenOr;
        foreach (Image button in buttonGroupImgs)
        {
            button.color = new Color(0.3f, 0.3f, 0.3f, 1);
        }
        switch (screenOr)
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

        GlobalStatMap.ModifySettings(settingsValues);
        toggledScreenOr.Invoke(screenOr);
    }

    public void attemptReturn()
    {
        if (readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
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
        if (readyForSFX) SfxManager.instance.playSFXbyName("click-short", null, 1);
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
