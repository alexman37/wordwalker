using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] musicTracks;
    public AudioSource audioSource;
    int index = 0;

    float timeToNextClip;
    float timeTarget;

    bool inLoop = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!inLoop)
        {
            timeToNextClip = 0f;
            timeTarget = 0f;
            kickoffMusicLoop();
        }
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
                audioSource.Play();
            }
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
