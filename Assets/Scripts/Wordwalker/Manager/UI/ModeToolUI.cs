using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ModeToolUI : MonoBehaviour
{
    public PlayerMode currentMode;
    Image image;
    public Sprite[] imageRotation;

    public static event Action inMarkerMode;
    public static event Action inViewMode;
    public static event Action inStepperMode;

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
    }

    // Update is called once per frame
    void Update()
    {
        
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
        alsoChangePicture();
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
}
