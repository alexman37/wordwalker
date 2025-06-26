using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    // I really shouldve gave an F about singletons earlier, huh
    public static SfxManager instance;

    public AudioSource soundFXObject;

    // Some audio clips are tricky to get into some classes, so we store them / play them from here.
    public List<string> clipNames;
    public AudioClip[] clipsInOrder;

    // When we want to start/stop audio loops we use this
    private Dictionary<string, AudioSource> activeLoops;

    // Sequences are lists of sounds
    private Dictionary<string, List<AudioSource>> activeSequences;

    private static float globalSFXVolume = 1f; // 0 - 1

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        activeLoops = new Dictionary<string, AudioSource>();
        activeSequences = new Dictionary<string, List<AudioSource>>();
    }

    private void OnEnable()
    {
        SettingsMenu.toggledSfxVol += adjustGlobalSFXVolume;
    }

    private void OnDisable()
    {
        SettingsMenu.toggledSfxVol -= adjustGlobalSFXVolume;
    }

    public void adjustGlobalSFXVolume(float newPct)
    {
        globalSFXVolume = newPct;
        // TODO Play a sound effect
    }

    // Play typical audio clip
    public void playSFX(AudioClip audioClip, Transform spawnHere, float volumeLevel)
    {
        AudioSource audioSource;
        if (spawnHere != null)
            audioSource = Instantiate(soundFXObject, spawnHere.position, Quaternion.identity);
        else
            audioSource = Instantiate(soundFXObject);

        audioSource.clip = audioClip;
        audioSource.volume = volumeLevel * globalSFXVolume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;

        GameObject.Destroy(audioSource.gameObject, clipLength);
    }

    // Play an audio clip specified in this manager's local list
    public void playSFXbyName(string clipName, Transform spawnHere, float volumeLevel)
    {
        AudioSource audioSource;
        if (spawnHere != null)
            audioSource = Instantiate(soundFXObject, spawnHere.position, Quaternion.identity);
        else
            audioSource = Instantiate(soundFXObject);

        audioSource.clip = clipsInOrder[clipNames.IndexOf(clipName)];
        audioSource.volume = volumeLevel * globalSFXVolume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;

        GameObject.Destroy(audioSource.gameObject, clipLength);
    }

    // Audio loop - will play continuously until manually stopped
    public void beginSFXLoop(string nameOfLoop, AudioClip audioClip, Transform spawnHere, float volumeLevel)
    {
        AudioSource audioSource;
        if (spawnHere != null)
            audioSource = Instantiate(soundFXObject, spawnHere.position, Quaternion.identity);
        else
            audioSource = Instantiate(soundFXObject);

        if (!activeLoops.ContainsKey(nameOfLoop))
        {
            activeLoops.Add(nameOfLoop, audioSource);

            audioSource.loop = true;
            audioSource.clip = audioClip;
            audioSource.volume = volumeLevel * globalSFXVolume;
            audioSource.Play();
        }
        else
        {
            GameObject.Destroy(audioSource);
            Debug.LogWarning($"Couldn't create looping SFX {nameOfLoop}: That loop is already running?");
        }
    }

    public void endSFXLoop(string nameOfLoop)
    {
        if(activeLoops.ContainsKey(nameOfLoop))
        {
            GameObject.Destroy(activeLoops[nameOfLoop].gameObject);
            activeLoops.Remove(nameOfLoop);
        }
    }

    // Set up a sequence of sounds to be played
    public void setupSFXSequence(string nameOfSequence, AudioClip[] audioClips, float volumeLevel)
    {
        List<AudioSource> audioSources = new List<AudioSource>();

        foreach(AudioClip clip in audioClips)
        {
            AudioSource audioSource = Instantiate(soundFXObject);
            audioSource.clip = clip;
            audioSource.volume = volumeLevel;
            audioSources.Add(audioSource);
        }

        activeSequences.Add(nameOfSequence, audioSources);
    }

    public void useSFXSequence(string nameOfSequence, int index, bool deleteOnPlay)
    {
        if(activeSequences.ContainsKey(nameOfSequence))
        {
            if (activeSequences[nameOfSequence].Count <= index)
            {
                Debug.LogWarning($"Couldn't play audio sequence {nameOfSequence}, index {index}: Out of range");
            }
            else if (activeSequences[nameOfSequence][index] == null)
            {
                Debug.Log($"The audio sequence {nameOfSequence} index {index} was already used and deleted!");
            }
            else {
                activeSequences[nameOfSequence][index].volume *= globalSFXVolume;
                activeSequences[nameOfSequence][index].Play();
                if(deleteOnPlay)
                {
                    float clipLength = activeSequences[nameOfSequence][index].clip.length;
                    GameObject.Destroy(activeSequences[nameOfSequence][index].gameObject, clipLength);
                    activeSequences[nameOfSequence][index] = null;
                }
            }
        } else
        {
            Debug.LogWarning($"Couldn't find audio sequence '{nameOfSequence}'.");
        }
    }

    public void destroySFXSequence(string nameOfSequence)
    {
        if (activeSequences.ContainsKey(nameOfSequence))
        {
            foreach(AudioSource audioSource in activeSequences[nameOfSequence])
            {
                if(audioSource != null)
                {
                    GameObject.Destroy(audioSource.gameObject);
                }
            }
            activeSequences.Remove(nameOfSequence);
        }
        else
        {
            Debug.LogWarning($"Couldn't find audio sequence '{nameOfSequence}'.");
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
