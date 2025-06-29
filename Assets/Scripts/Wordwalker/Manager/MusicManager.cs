using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] musicTracks;
    static AudioSource audioSource;
    int index = 0;

    float timeToNextClip;
    float timeTarget;

    bool inLoop = false;
    static bool fadingOut = false;
    static bool fadingIn = false;
    static float fadeTimer = 0f;

    private static float globalMusicVolume = 1f;
    private static float storedVolume = 1f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();

        if (!inLoop)
        {
            timeToNextClip = 0f;
            timeTarget = 0f;
            kickoffMusicLoop();
        }
    }

    private void OnEnable()
    {
        SettingsMenu.toggledMusicVol += adjustGlobalMusicVolume;
        PauseMenu.toggledMusicVol += adjustGlobalMusicVolume;
    }

    private void OnDisable()
    {
        SettingsMenu.toggledMusicVol -= adjustGlobalMusicVolume;
        PauseMenu.toggledMusicVol -= adjustGlobalMusicVolume;
    }

    // In-game music handler
    public static void inGameMusicFade(bool fadeOut)
    {
        if(!GlobalStatMap.statMap.settingsValues.inGameMusic)
        {
            // Fade out, stop playing altogether
            if (fadeOut)
            {
                storedVolume = globalMusicVolume;
                fadingOut = true;
            }
            // Fade in, continue playing at previous levels
            else
            {
                fadingIn = true;
                audioSource.Play();
            }
        }
    }

    public void adjustGlobalMusicVolume(float newPct)
    {
        globalMusicVolume = newPct;
        // Since there's only one we just set it here
        audioSource.volume = globalMusicVolume;
    }

    private void Update()
    {
        if(inLoop)
        {
            timeToNextClip += Time.deltaTime;
            if (timeToNextClip >= timeTarget)
            {
                audioSource.Stop();
                timeToNextClip = 0;

                index = (index + 1) % musicTracks.Length;
                audioSource.clip = musicTracks[index];
                float timeToPlay = musicTracks[index].length;
                timeTarget = timeToPlay + 3f; // 3 second buffer
                audioSource.Stop();
            }
        }

        // So needlessly complicated because we cant run coroutines from static methods...sigh...
        if(fadingOut)
        {
            fadeTimer = fadeTimer + Time.deltaTime;
            inLoop = false;
            if(fadeTimer >= 0.3f)
            {
                fadeTimer = fadeTimer % 0.3f;
                globalMusicVolume -= globalMusicVolume * 0.1f;
            }
            
            if (globalMusicVolume <= 0.02f)
            {
                audioSource.Stop();
                fadingOut = false;
                globalMusicVolume = 0;
            }
            audioSource.volume = globalMusicVolume;
        }

        else if(fadingIn)
        {
            inLoop = true;

            if (fadeTimer >= 0.3f)
            {
                fadeTimer = fadeTimer % 0.3f;
                globalMusicVolume += storedVolume * 0.1f;
            }

            if(globalMusicVolume >= storedVolume)
            {
                fadingIn = false;
                globalMusicVolume = storedVolume;
            }
            audioSource.volume = globalMusicVolume;
        }
    }

    // play all music tracks in order, repeatedly
    public void kickoffMusicLoop()
    {
        audioSource.clip = musicTracks[index];
        float timeToPlay = musicTracks[index].length;
        index = (index + 1) % musicTracks.Length;

        inLoop = true;

        timeTarget = timeToPlay + 3f; // 3 second buffer
        timeToNextClip = 0f;
        audioSource.Play();
    }

    public void stopMusicLoop()
    {
        audioSource.Stop();
        inLoop = false;
    }
}
