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

    //components
    public TextMeshProUGUI clueText;
    RectTransform scrollRect;
    Image img;

    [SerializeField] private AudioClip unfurlClip;

    // Start is called before the first frame update
    void Start()
    {
        img = scrollClue.GetComponent<Image>();
        scrollRect = scrollClue.GetComponent<RectTransform>();

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
    }

    private void OnEnable()
    {
        // When the character plays the animation to open their scroll, start same animation here
        // Win or lose, move the scroll out of view afterwards
        AnimationManager.openedScroll += moveScrollOnStart;
        GameManagerSc.levelWon += moveScrollOnFinish;
        GameManagerSc.gameOver += moveScrollOnFinish;
    }

    private void OnDisable()
    {
        // When the character plays the animation to open their scroll, start same animation here
        // Win or lose, move the scroll out of view afterwards
        AnimationManager.openedScroll -= moveScrollOnStart;
        GameManagerSc.levelWon -= moveScrollOnFinish;
        GameManagerSc.gameOver -= moveScrollOnFinish;
    }

    public void setClue(string clue)
    {
        clueText.text = clue;
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
    public void moveScrollOnFinish(GameManagerSc.LossReason _)
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
        Color col = img.color;
        img.color = new Color(col.r, col.g, col.b, 1);

        SfxManager.instance.playSFX(unfurlClip, null, 1f);

        // Movement animation: Move up
        float timeSec = 1f;
        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            scrollRect.anchoredPosition = UIUtils.XerpStandard(scrollStart, scrollDest, Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }

        // Sprite animation: Open scroll, see what's inside
        scrollAnimator.SetTrigger("BeginUnfurl");
        col = clueText.color;
        for (float i = 0; i <= timeSec; i++)
        {
            clueText.color = new Color(col.r, col.g, col.b, Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }

        yield return null;
    }

    /// <summary>
    /// Move scroll below bottom of screen
    /// </summary>
    IEnumerator moveScrollOutOfView()
    {
        // Movement animation: Move scroll below bottom of screen
        float timeSec = 1f;
        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            scrollRect.anchoredPosition = UIUtils.XerpStandard(scrollDest, scrollStart, Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }

        // Make scroll's text invisible (as scroll itself is fading) and close it so the opening animation plays again
        Color col = clueText.color;
        clueText.color = new Color(col.r, col.g, col.b, 0);
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
        float timeSec = 1f;

        Color col = img.color;

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            img.color = new Color(col.r, col.g, col.b, 1f - Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }
    }
}
