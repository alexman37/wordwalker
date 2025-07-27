using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsUI : MonoBehaviour
{
    public GameObject itemsMenu;
    bool isOpened = false;
    bool isActive = true;
    bool initialDefine = false;
    bool deadState = false;

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

    public AudioClip itemsSlideIn;
    public AudioClip itemsSlideOut;

    private void Start()
    {
        thisImg = GetComponent<Image>();
    }

    private void OnEnable()
    {
        GameManagerSc.changeInTotems += respondToTotemsChange;
        GameManagerSc.gameOver += disableMenu;
        GameManagerSc.levelWon += disableMenu;
        GameManagerSc.levelReady += reenableMenu;
        GameManagerSc.newGame += defineBounds;
    }

    private void OnDisable()
    {
        GameManagerSc.changeInTotems -= respondToTotemsChange;
        GameManagerSc.gameOver -= disableMenu;
        GameManagerSc.levelWon -= disableMenu;
        GameManagerSc.levelReady -= reenableMenu;
        GameManagerSc.newGame -= defineBounds;
    }

    public void toggleItemsMenu()
    {
        if(isActive && !deadState)
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
        if (containerRect == null) defineBounds();
        if(!deadState)
        {
            if (totems <= 0)
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
    }

    void defineBounds()
    {
        if(!initialDefine)
        {
            containerRect = transform.GetChild(0).GetComponent<RectTransform>();
            itemsStart = containerRect.anchoredPosition;
            itemsDest = new Vector2(itemsStart.x - containerRect.rect.width * 1.25f, itemsStart.y);
            containerRect.anchoredPosition = itemsDest;
            movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, containerRect, itemsStart);
            movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, containerRect, itemsDest);
            initialDefine = true;
        }
    }

    void openMenu()
    {
        if (containerRect == null) defineBounds();
        StopCoroutine(movingCoroutineOut);
        movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 1f, containerRect, itemsStart);
        StartCoroutine(movingCoroutineIn);
        SfxManager.instance.playSFX(itemsSlideIn, null, 1f);
    }

    void closeMenu()
    {
        if (containerRect == null) defineBounds();
        StopCoroutine(movingCoroutineIn);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 1f, containerRect, itemsDest);
        StartCoroutine(movingCoroutineOut);
        SfxManager.instance.playSFX(itemsSlideOut, null, 1f);
    }

    void disableMenu()
    {
        deadState = true;
        if (containerRect == null) defineBounds();
        containerRect.anchoredPosition = itemsDest;
        Debug.Log("DEACTIVATING!");
        thisImg.sprite = deactivated;
        isActive = false;
        isOpened = false;
    }

    void disableMenu(GameManagerSc.LossReason _)
    {
        disableMenu();
    }

    void reenableMenu()
    {
        deadState = false;
        if (containerRect == null) defineBounds();
        containerRect.anchoredPosition = itemsDest;
        thisImg.sprite = normal;
        isActive = true;
        isOpened = false;
    }
}
