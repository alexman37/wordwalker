using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ChallengeClick : MonoBehaviour
{
    private Image imgContent;
    private Image imgBackground;

    public string id;
    public string challengeName;
    public string desc;
    public static event Action<string, bool, Sprite, string, string> enable;
    private bool challengeEnabled = false;

    public Sprite enabledSpr;
    public Sprite disabledSpr;

    public void changeEnable()
    {
        challengeEnabled = !challengeEnabled;
        if (challengeEnabled) imgBackground.sprite = enabledSpr;
        else imgBackground.sprite = disabledSpr;
        enable.Invoke(id, challengeEnabled, imgContent.sprite, challengeName, desc);
    }

    // Start is called before the first frame update
    void Start()
    {
        enable += (i,b,_,__,___) => { };
        imgContent = transform.GetChild(0).GetComponent<Image>();
        imgBackground = GetComponent<Image>();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
