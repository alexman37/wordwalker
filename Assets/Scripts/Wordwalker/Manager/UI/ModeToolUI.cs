using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ModeToolUI : MonoBehaviour
{
    public PlayerMode currentMode;
    Image image;
    public Sprite[] imageRotation;

    public static event Action inMarkerMode;
    public static event Action inViewMode;
    public static event Action inStepperMode;

    public Image infographicImg;
    public TextMeshProUGUI infographicText;
    IEnumerator fadingCoroutine;

    public AudioClip modeChangeClip;

    public enum PlayerMode
    {
        MARKER,
        STEPPER,
        VIEW
    }

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        inStepperMode.Invoke();

        infographicImg.transform.parent.gameObject.SetActive(false);
    }

    public void changeMode()
    {
        switch(currentMode)
        {
            case PlayerMode.STEPPER: 
                this.currentMode = PlayerMode.VIEW;
                inViewMode.Invoke();
                break;
            case PlayerMode.MARKER: 
                this.currentMode = PlayerMode.STEPPER;
                inStepperMode.Invoke();
                break;
            case PlayerMode.VIEW: 
                this.currentMode = PlayerMode.MARKER;
                inMarkerMode.Invoke();
                break;
        }
        SfxManager.instance.playSFX(modeChangeClip, null, 1);
        alsoChangePicture();
        alsoShowInfographic();
    }

    void alsoChangePicture()
    {
        switch (currentMode)
        {
            case PlayerMode.STEPPER: this.image.sprite = imageRotation[0]; break;
            case PlayerMode.MARKER: this.image.sprite = imageRotation[1]; break;
            case PlayerMode.VIEW: this.image.sprite = imageRotation[2]; break;
        }
    }

    void alsoShowInfographic()
    {
        infographicImg.transform.parent.gameObject.SetActive(true);
        if(fadingCoroutine != null) StopCoroutine(fadingCoroutine);

        infographicImg.color = Color.white;
        infographicText.color = Color.white;
        switch (currentMode)
        {
            case PlayerMode.STEPPER:
                infographicImg.sprite = imageRotation[0];
                infographicText.text = "Player Camera";
                break;
            case PlayerMode.MARKER:
                infographicImg.sprite = imageRotation[1];
                infographicText.text = "Marker Mode";
                break;
            case PlayerMode.VIEW:
                infographicImg.sprite = imageRotation[2];
                infographicText.text = "Free Camera";
                break;
        }

        fadingCoroutine = fadeAwayInfographic();
        StartCoroutine(fadingCoroutine);
    }

    IEnumerator fadeAwayInfographic()
    {
        yield return new WaitForSeconds(1);
        float timeSec = 1f;

        for(float i = 0; i < timeSec; i += Time.deltaTime)
        {
            infographicImg.color = new Color(1, 1, 1, 1 - i / timeSec);
            infographicText.color = new Color(1, 1, 1, 1 - i / timeSec);
            yield return null;
        }

        infographicImg.color = Color.clear;
        infographicText.color = Color.clear;
        infographicImg.transform.parent.gameObject.SetActive(false);
    }
}
