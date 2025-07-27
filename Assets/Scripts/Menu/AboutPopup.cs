using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutPopup : WidgetPopup
{
    public GameObject aboutContent;
    public GameObject credsContent;

    // Start is called before the first frame update
    void Start()
    {
        this.Setup();


    }

    public void goToAbout()
    {
        SfxManager.instance.playSFXbyName("page-turn-short", null, 1);
        aboutContent.SetActive(true);
        credsContent.SetActive(false);
    }

    public void goToCredits()
    {
        SfxManager.instance.playSFXbyName("page-turn-short", null, 1);
        aboutContent.SetActive(false);
        credsContent.SetActive(true);
    }
}
