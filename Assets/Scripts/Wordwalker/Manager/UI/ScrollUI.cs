using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScrollUI : MonoBehaviour
{
    public GameObject scrollClue;        // The scroll used for text clues
    public Animator scrollAnimator;      // Animation component

    //animating - gotta know the right positions
    private Vector2 scrollStart;
    private Vector2 scrollDest;

    // Start is called before the first frame update
    void Start()
    {
        // Set scroll "start" and "dest" positions when scaling finishes. Assume it wont take long
        // If done before we get a chance to subscribe to it just do so immediately
        ScalingUIComponent scalingComp = GetComponent<ScalingUIComponent>();
        scalingComp.completedScaling += () =>
        {
            scrollStart = scrollClue.GetComponent<RectTransform>().anchoredPosition;
            scrollDest = new Vector2(0, 0); //relative to bottom of screen
        };
        if (scalingComp.DONE)
        {
            scrollStart = scrollClue.GetComponent<RectTransform>().anchoredPosition;
            scrollDest = new Vector2(0, 0); //relative to bottom of screen
        }

        // When the character plays the animation to open their scroll, start same animation here
        // Win or lose, move the scroll out of view afterwards
        WalkManager.openedScroll += moveScrollOnStart;
        GameManagerSc.levelWon += moveScrollOnFinish;
        GameManagerSc.gameOver += moveScrollOnFinish;
    }

    /// <summary>
    /// Move the scroll into view when starting a level.
    /// </summary>
    public void moveScrollOnStart()
    {
        StartCoroutine(moveScrollIntoView());
    }

    /// <summary>
    /// Gradually make scroll disappear and also get it out of the way when done a level.
    /// </summary>
    public void moveScrollOnFinish()
    {
        StartCoroutine(fadeClueScroll());
        StartCoroutine(moveScrollOutOfView());
    }

    /// <summary>
    /// Move scroll up into the bottom of the screen where it shall remain
    /// </summary>
    IEnumerator moveScrollIntoView()
    {
        // "Unfade" the scroll immediately, making it visible again
        Image img = scrollClue.GetComponent<Image>();
        Color col = img.color;
        img.color = new Color(col.r, col.g, col.b, 1);

        RectTransform scrollRect = scrollClue.GetComponent<RectTransform>();

        // Movement animation: Move up
        float steps = 30;
        float timeSec = 1f;
        for (float i = 0; i <= steps; i++)
        {
            scrollRect.anchoredPosition = UIUtils.XerpStandard(scrollStart, scrollDest, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        // Sprite animation: Open scroll, see what's inside
        scrollAnimator.SetTrigger("BeginUnfurl");
        TextMeshProUGUI texts = scrollRect.GetComponentInChildren<TextMeshProUGUI>();
        col = texts.color;
        for (float i = 0; i <= steps; i++)
        {
            texts.color = new Color(col.r, col.g, col.b, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return null;
    }

    /// <summary>
    /// Move scroll below bottom of screen
    /// </summary>
    IEnumerator moveScrollOutOfView()
    {
        RectTransform scrollRect = scrollClue.GetComponent<RectTransform>();

        // Movement animation: Move scroll below bottom of screen
        float steps = 30;
        float timeSec = 1f;
        for (float i = 0; i <= steps; i++)
        {
            scrollRect.anchoredPosition = UIUtils.XerpStandard(scrollDest, scrollStart, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        // Make scroll's text invisible (as scroll itself is fading) and close it so the opening animation plays again
        TextMeshProUGUI texts = scrollRect.GetComponentInChildren<TextMeshProUGUI>();
        Color col = texts.color;
        texts.color = new Color(col.r, col.g, col.b, 0);
        scrollAnimator.ResetTrigger("BeginUnfurl");
        scrollAnimator.SetTrigger("Reset");

        yield return null;
    }

    /// <summary>
    /// Fade the scroll away
    /// </summary>
    //TODO eventually we might have a burning animation for this.
    IEnumerator fadeClueScroll()
    {
        yield return null;
        float frameTime = 30;

        Image img = scrollClue.GetComponent<Image>();
        Color col = img.color;

        for (float i = 0; i <= frameTime; i++)
        {
            img.color = new Color(col.r, col.g, col.b, (frameTime - i) / frameTime);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
