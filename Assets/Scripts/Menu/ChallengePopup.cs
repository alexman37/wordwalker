using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ChallengePopup : MonoBehaviour
{
    public Image img;

    public Sprite[] withStars;

    public GameObject[] infoLines;

    private Vector2 animationOffsite;
    private Vector2 animationStart;

    public volatile int numChallengesEnabled = 0;

    // Conveys to MenuScript when a challenge is selected
    public static event Action<MenuScript.Challenge, bool> challengeEnabled;

    public static event Action<int> closedPopup;

    private void updateChallengeInfo(MenuScript.Challenge id, bool enabled, Sprite newSpr)
    {
        //if (enabled) Interlocked.Increment(ref numChallengesEnabled);
        //else Interlocked.Decrement(ref numChallengesEnabled);
        if (enabled) numChallengesEnabled++;
        else numChallengesEnabled--;

        getAppropriateStarBack();

        updateInfoLine(id, enabled);

        // TODO something with stars down below?

        challengeEnabled.Invoke(id, enabled);
    }

    private void enableAllChallenges(bool on)
    {
        if(on)
        {
            numChallengesEnabled = 5;
            getAppropriateStarBack();
            enOrDisAll(on);
        }

        if (!on)
        {
            numChallengesEnabled = 0;
            getAppropriateStarBack();
            enOrDisAll(on);
        }
    }

    private void updateInfoLine(MenuScript.Challenge id, bool enabled)
    {
        switch (id)
        {
            case MenuScript.Challenge.IRON_MAN:
                enOrDisInfoLine(0, enabled); break;
            case MenuScript.Challenge.SPECIAL_TILES:
                enOrDisInfoLine(1, enabled); break;
            case MenuScript.Challenge.TIMER:
                enOrDisInfoLine(2, enabled); break;
            case MenuScript.Challenge.GEN_PLUS:
                enOrDisInfoLine(3, enabled); break;
            case MenuScript.Challenge.FOG:
                enOrDisInfoLine(4, enabled); break;
        }
    }

    private void enOrDisInfoLine(int info, bool en)
    {
        if(en)
        {
            infoLines[info].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
            infoLines[info].transform.GetChild(1).GetComponent<Image>().color = new Color(0, 0, 0, 0);
        } else
        {
            infoLines[info].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
            infoLines[info].transform.GetChild(1).GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        }
        
    }

    private void enOrDisAll(bool on)
    {
        challengeEnabled.Invoke(MenuScript.Challenge.IRON_MAN, on);
        challengeEnabled.Invoke(MenuScript.Challenge.FOG, on);
        challengeEnabled.Invoke(MenuScript.Challenge.GEN_PLUS, on);
        challengeEnabled.Invoke(MenuScript.Challenge.SPECIAL_TILES, on);
        challengeEnabled.Invoke(MenuScript.Challenge.TIMER, on);

        for(int i = 0; i < 5; i++)
        {
            enOrDisInfoLine(i, on);
        }
    }

    // num stars enabled == index in this array
    private void getAppropriateStarBack()
    {
        img.sprite = withStars[numChallengesEnabled];
    }

    public void openPopup()
    {
        StartCoroutine(openPopupCo());
    }

    IEnumerator openPopupCo()
    {
        float steps = 30;
        float timeSec = 0.5f;

        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 pos = rectTransform.anchoredPosition;

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(pos,
                    animationStart,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }
    }

    public void closePopup()
    {
        closedPopup.Invoke(numChallengesEnabled);
        StartCoroutine(closePopupCo());
    }

    IEnumerator closePopupCo()
    {
        float steps = 30;
        float timeSec = 0.5f;

        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 pos = rectTransform.anchoredPosition;

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(pos,
                    animationOffsite,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }
    }


    private void Start()
    {
        closedPopup += (_) => { };

        animationOffsite = new Vector2(0, -Screen.safeArea.height);
        animationStart = new Vector2(0, 0);
        GetComponent<RectTransform>().anchoredPosition = animationOffsite;
    }

    private void OnEnable()
    {
        ChallengeClick.enable += updateChallengeInfo;
        ChallengeEnabler.enableAllChallenges += enableAllChallenges;
    }

    private void OnDisable()
    {
        ChallengeClick.enable -= updateChallengeInfo;
        ChallengeEnabler.enableAllChallenges -= enableAllChallenges;
    }
}
