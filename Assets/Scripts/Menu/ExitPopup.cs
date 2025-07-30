using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPopup : WidgetPopup
{
    // Start is called before the first frame update
    void Start()
    {
        this.Setup();
    }

    public void exitGame()
    {
        SfxManager.instance.playSFXbyName("click-short", null, 1);
        Application.Quit();
    }

    public new void closeWidgetPopup()
    {
        SfxManager.instance.playSFXbyName("click-short", null, 1);
        base.closeWidgetPopup();
    }
}
