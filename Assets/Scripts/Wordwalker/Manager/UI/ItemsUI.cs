using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsUI : MonoBehaviour
{
    public GameObject itemsMenu;
    bool isOpened = false;
    bool isActive = true;

    private Image thisImg;
    public Sprite normal;
    public Sprite activated;
    public Sprite deactivated;

    private RectTransform containerRect;
    public ScalingUIComponent scalingComp;

    IEnumerator movingCoroutineIn;
    IEnumerator movingCoroutineOut;
    Vector2 itemsStart;
    Vector2 itemsDest;

    private void Start()
    {
        thisImg = GetComponent<Image>();

        scalingComp.completedScaling += () =>
        {
            containerRect = transform.GetChild(0).GetComponent<RectTransform>();
            itemsStart = containerRect.anchoredPosition;
            itemsDest = new Vector2(itemsStart.x - containerRect.rect.width * 1.25f, itemsStart.y);
        };
        if (scalingComp.DONE)
        {
            containerRect = transform.GetChild(0).GetComponent<RectTransform>();
            itemsStart = containerRect.anchoredPosition;
            itemsDest = new Vector2(itemsStart.x - containerRect.rect.width * 1.25f, itemsStart.y);
        }

        containerRect.anchoredPosition = itemsDest;

        movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, containerRect, itemsStart);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, containerRect, itemsDest);
    }

    private void OnEnable()
    {
        GameManagerSc.changeInTotems += respondToTotemsChange;
        GameManagerSc.gameOver += closeMenu;
        GameManagerSc.levelWon += closeMenu;
    }

    private void OnDisable()
    {
        GameManagerSc.changeInTotems -= respondToTotemsChange;
        GameManagerSc.gameOver -= closeMenu;
        GameManagerSc.levelWon -= closeMenu;
    }

    public void toggleItemsMenu()
    {
        if(isActive)
        {
            isOpened = !isOpened;

            if (isOpened)
            {
                openMenu();
                thisImg.sprite = activated;
            }

            else
            {
                closeMenu(); // the loss reason means nothing here - just need it to cooperate with an action
                thisImg.sprite = normal;
            }
        }
    }

    private void respondToTotemsChange(int totems)
    {
        if(totems <= 0)
        {
            closeMenu();
            thisImg.sprite = deactivated;
            isActive = false;
        }
        else
        {
            thisImg.sprite = normal;
            isActive = true;
        }
    }

    void openMenu()
    {
        StopCoroutine(movingCoroutineOut);
        movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 1f, containerRect, itemsStart);
        StartCoroutine(movingCoroutineIn);
    }

    void closeMenu()
    {
        StopCoroutine(movingCoroutineIn);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 1f, containerRect, itemsDest);
        StartCoroutine(movingCoroutineOut);
    }

    void closeMenu(GameManagerSc.LossReason _)
    {
        StopCoroutine(movingCoroutineIn);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 1f, containerRect, itemsDest);
        StartCoroutine(movingCoroutineOut);
    }
}
