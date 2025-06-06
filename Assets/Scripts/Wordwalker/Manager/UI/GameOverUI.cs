using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    //animating - gotta know the right positions
    public TextMeshProUGUI title;
    private Vector2 gameOverAnimationStart;
    private Vector2 gameOverAnimationDest;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        ScalingUIComponent scalingComp = GetComponent<ScalingUIComponent>();
        GetComponent<ScalingUIComponent>().completedScaling += () =>
        {
            gameOverAnimationStart = GetComponent<RectTransform>().anchoredPosition;
            gameOverAnimationDest = new Vector2(0, 0);
        };
        if (scalingComp.DONE)
        {
            gameOverAnimationStart = GetComponent<RectTransform>().anchoredPosition;
            gameOverAnimationDest = new Vector2(0, 0); //relative to bottom of screen
        }
    }

    private void OnEnable()
    {
        GameManagerSc.gameOver += BeginGameOverAnimation;
        GameManagerSc.levelReset += gameOverReset;
    }

    private void OnDisable()
    {
        GameManagerSc.gameOver -= BeginGameOverAnimation;
        GameManagerSc.levelReset -= gameOverReset;
    }

    void gameOverReset()
    {
        if(rectTransform == null) rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.anchoredPosition = gameOverAnimationStart;
    }

    private void BeginGameOverAnimation(GameManagerSc.LossReason lr)
    {
        //TODO maybe we have one of many messages?
        switch (lr)
        {
            case GameManagerSc.LossReason.TOTEMS: title.text = "GAME OVER"; break;
            case GameManagerSc.LossReason.TIME: title.text = "TIME'S UP!"; break;
        }
        StartCoroutine(gameOverAnimation(1.5f));
    }

    IEnumerator gameOverAnimation(float delay)
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
            rectTransform.anchoredPosition = UIUtils.XerpStandard(adjustedStart,
                    gameOverAnimationDest,
                    i / frameTime);

            yield return new WaitForSeconds(1 / frameTime * timeSec);
        }
    }
}
