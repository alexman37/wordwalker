using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioClip[] musicTracks;
    static AudioSource audioSource;
    int index;

    float timeToNextClip;
    float timeTarget;

    static bool fadingOut;
    static bool fadingIn;
    static float fadeTimer;

    private static float globalMusicVolume;
    private static float fadeMultiplier;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();

            index = 0;
            fadingOut = false;
            fadingIn = false;
            fadeTimer = 0;
            globalMusicVolume = 1f;
            fadeMultiplier = 1f;

            timeToNextClip = 0f;
            timeTarget = 0f;
        }
        else
        {
            Destroy(this);
        }

        /*if (!inLoop)
        {
            Debug.Log("Not in loop");
            
        }*/
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
        // Fade out, stop playing altogether
        if (fadeOut)
        {
            fadingOut = true;
            fadingIn = false;
        }
        // Fade in, continue playing at previous levels
        else
        {
            fadingIn = true;
            fadingOut = false;
        }
    }

    public void adjustGlobalMusicVolume(float newPct)
    {
        globalMusicVolume = newPct;
        // Since there's only one we just set it here
        audioSource.volume = globalMusicVolume * fadeMultiplier;
    }

    private void Update()
    {
        timeToNextClip += Time.deltaTime;
        if (timeToNextClip >= timeTarget)
        {
            Debug.Log("AAA playing new song");
            audioSource.Stop();
            timeToNextClip = 0;

            index = (index + 1) % musicTracks.Length;
            audioSource.clip = musicTracks[index];
            float timeToPlay = musicTracks[index].length;
            timeTarget = timeToPlay + 3f; // 3 second buffer
            audioSource.Play();
        }

        // So needlessly complicated because we cant run coroutines from static methods...sigh...
        if (fadingOut)
        {
            fadeTimer = fadeTimer + Time.deltaTime;
            if(fadeTimer >= 0.3f)
            {
                fadeTimer = fadeTimer % 0.3f;
                fadeMultiplier -= 0.1f;
                Debug.Log("AAA Faded out next step: " + fadeMultiplier);
            }
            
            if (fadeMultiplier <= 0.02f)
            {
                Debug.Log("AAA Done fading out");
                fadingOut = false;
                fadeMultiplier = 0;
            }
            audioSource.volume = globalMusicVolume * fadeMultiplier;
        }

        else if(fadingIn)
        {
            fadeTimer = fadeTimer + Time.deltaTime;

            if (fadeTimer >= 0.3f)
            {
                fadeTimer = fadeTimer % 0.3f;
                fadeMultiplier += 0.1f;
                Debug.Log("AAA Faded in next step: " + fadeMultiplier);
            }

            if(fadeMultiplier >= 1)
            {
                Debug.Log("AAA Done fading in");
                fadingIn = false;
                fadeMultiplier = 1;
            }
            audioSource.volume = globalMusicVolume * fadeMultiplier;
        }
    }

    // play all music tracks in order, repeatedly
    public void kickoffMusicLoop()
    {
        audioSource.clip = musicTracks[index];
        float timeToPlay = musicTracks[index].length;
        index = (index + 1) % musicTracks.Length;

        timeTarget = timeToPlay + 3f; // 3 second buffer
        timeToNextClip = 0f;

        audioSource.Play();
    }

    public void stopMusicLoop()
    {
        audioSource.Stop();
    }
}
