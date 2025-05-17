using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ChallengeClick : MonoBehaviour
{
    private Image imgContent;
    private Image imgBackground;

    public MenuScript.Challenge id;

    public static event Action<MenuScript.Challenge, bool, Sprite> enable;
    private bool challengeEnabled = false;

    public Sprite enabledSpr;
    public Sprite disabledSpr;

    public void changeEnable()
    {
        challengeEnabled = !challengeEnabled;
        if (challengeEnabled) imgBackground.sprite = enabledSpr;
        else imgBackground.sprite = disabledSpr;
        enable.Invoke(id, challengeEnabled, imgContent.sprite);
    }

    public void enableFromAll(bool on)
    {
        challengeEnabled = on;
        if (challengeEnabled) imgBackground.sprite = enabledSpr;
        else imgBackground.sprite = disabledSpr;
    }

    // Start is called before the first frame update
    void Start()
    {
        enable += (i,b,_) => { };
        imgContent = transform.GetComponent<Image>();
        imgBackground = GetComponent<Image>();
    }

    private void OnEnable()
    {
        ChallengeEnabler.enableAllChallenges += enableFromAll;
    }

    private void OnDisable()
    {
        ChallengeEnabler.enableAllChallenges -= enableFromAll;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
