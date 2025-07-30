using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharSelectButton : MonoBehaviour
{
    public int order;
    public string shortName;
    public string realName;
    public bool usable;
    public Image img;
    public TextMeshProUGUI title;
    public string unlockCondition;
    public Sprite unlocked;
    public Sprite locked;

    private CharSelectPopup popup;

    private void Start()
    {
        popup = transform.parent.parent.GetComponent<CharSelectPopup>();
    }

    public void SetupButton()
    {
        // smitty is always unlocked
        if(order == 0 || CharSelectPopup.lastLoadedStats.GetBool("char-unlock-" + shortName))
        {
            usable = true;
            img.sprite = unlocked;
            title.text = realName;
        } else
        {
            usable = false;
            img.sprite = locked;
            title.text = "???";
        }
    }

    public void clickedButton()
    {
        SfxManager.instance.playSFXbyName("click-short", null, 1);
        if(usable)
        {
            popup.useNewCharSprite(order);
            popup.hideUnlockCondition();
        } else
        {
            popup.showUnlockCondition(order, unlockCondition);
        }
    }
}
