using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The popup that appears when you win a round.
/// </summary>
public class PostgameUI : MonoBehaviour
{
    //animating - gotta know the right positions
    private Vector2 postgameAnimationStart;
    private Vector2 postgameAnimationDest;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        ScalingUIComponent scalingComp = GetComponent<ScalingUIComponent>();
        GetComponent<ScalingUIComponent>().completedScaling += () =>
        {
            postgameAnimationStart = GetComponent<RectTransform>().anchoredPosition;
            postgameAnimationDest = new Vector2(0, 0);
        };
        if (scalingComp.DONE)
        {
            postgameAnimationStart = GetComponent<RectTransform>().anchoredPosition;
            postgameAnimationDest = new Vector2(0, 0); //relative to bottom of screen
        }

        GameManagerSc.levelWon += BeginPostgameAnimation;
        GameManagerSc.levelReset += postgameReset;
    }

    void postgameReset()
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.anchoredPosition = postgameAnimationStart;
    }

    private void BeginPostgameAnimation()
    {
        StartCoroutine(postgameAnimation(1.5f));
    }

    IEnumerator postgameAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        float frameTime = 30;
        float timeSec = 1f;
        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        Vector2 adjustedStart = rectTransform.anchoredPosition;

        for (float i = 0; i <= frameTime; i++)
        {
            rectTransform.anchoredPosition = UIUtils.XerpStandard(postgameAnimationStart,
                    postgameAnimationDest,
                    i / frameTime);

            yield return new WaitForSeconds(1 / frameTime * timeSec);
        }
    }
}
