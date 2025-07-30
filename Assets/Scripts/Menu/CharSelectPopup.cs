using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharSelectPopup : WidgetPopup
{
    public static CharacterSprite activeCharSprite;
    public GameObject titleSpriteObj;
    public GameObject selectionFrame;
    public RectTransform[] picturePositions;
    Animator anim;

    //unlock condition popup
    public GameObject unlockCondition;
    private int lastOrder = -1;

    // new character unlocked alert
    public GameObject newCharacterUnlockedAlert;

    // assist in loading each character button
    public static StatMap lastLoadedStats;
    public CharSelectButton[] charButtons;

    // Start is called before the first frame update
    void Start()
    {
        this.Setup();

        anim = titleSpriteObj.GetComponent<Animator>();

        // Get last used character, or use smitty as the default
        lastLoadedStats = GlobalStatMap.loadGlobalStatMap();
        Dictionary<string, int> intMap = lastLoadedStats.intMap;
        if (intMap.ContainsKey("activeCharSprite"))
        {
            activeCharSprite = (CharacterSprite)intMap["activeCharSprite"];
        }
        else activeCharSprite = CharacterSprite.SMITTY;
        hideUnlockCondition();

        // Display the alert for a new character being unlocked, if eligible
        if(lastLoadedStats.flags.Contains("newCharUnlocked"))
        {
            newCharacterUnlockedAlert.SetActive(true);
            GlobalStatMap.RemoveFlag("newCharUnlocked");
        }

        // determine if each character is locked or unlocked in their own scripts.
        foreach(CharSelectButton button in charButtons)
        {
            button.SetupButton();
        }

        changeTitleAnimation();
        changeSelectionFramePos();
    }

    private void OnMouseDown()
    {
        hideUnlockCondition();
    }

    public void useNewCharSprite(int newVal)
    {
        activeCharSprite = (CharacterSprite) newVal;
        GlobalStatMap.AddOrModifyInt("activeCharSprite", (int) activeCharSprite);
        GlobalStatMap.saveGlobalStatMap();

        hideUnlockCondition();
        changeTitleAnimation();
        changeSelectionFramePos();
    }

    void changeTitleAnimation()
    {
        switch(activeCharSprite)
        {
            case CharacterSprite.SMITTY: anim.Play("char-smitty-anim"); break;
            case CharacterSprite.JESSE: anim.Play("char-jesse-anim"); break;
            case CharacterSprite.JANGO: anim.Play("char-jango-anim"); break;
            case CharacterSprite.SHADOW_SMITTY: anim.Play("char-shadow-smitty-anim"); break;
            case CharacterSprite.MONK: anim.Play("char-monk-anim"); break;
            case CharacterSprite.NIGHTINGALE: anim.Play("char-nightingale-anim"); break;
            case CharacterSprite.HAZEL: anim.Play("char-hazel-anim"); break;
            case CharacterSprite.WINTER: anim.Play("char-winter-anim"); break;
            case CharacterSprite.GOLDEN_SMITTY: anim.Play("char-golden-smitty-anim"); break;
            default: anim.Play("char-smitty-anim"); break;
        }
    }

    public void showUnlockCondition(int order, string condition)
    {
        if(order == lastOrder)
        {
            lastOrder = -1; // you can see it again after you close this one
            hideUnlockCondition();
        } else
        {
            lastOrder = order;
            RectTransform rt = unlockCondition.GetComponent<RectTransform>();
            rt.localPosition = picturePositions[order].localPosition + new Vector3(picturePositions[order].rect.width, 0, 0);
            unlockCondition.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = condition;
        }
    }

    public void hideUnlockCondition()
    {
        RectTransform rt = unlockCondition.GetComponent<RectTransform>();
        rt.localPosition = new Vector2(Screen.safeArea.width * 3, Screen.safeArea.height * 3);
    }

    //Change the position of the selection frame
    //Kind of janky...right now we basically hard code it knowing where it'd be...
    void changeSelectionFramePos()
    {
        int i = (int)activeCharSprite;
        selectionFrame.GetComponent<RectTransform>().localPosition = picturePositions[i].localPosition;
    }

    public void openPopup()
    {
        base.openWidgetPopup();
        newCharacterUnlockedAlert.SetActive(false);
    }

    public void closePopup()
    {
        base.closeWidgetPopup();
        hideUnlockCondition();
    }

    public enum CharacterSprite
    {
        SMITTY = 0,
        JESSE = 1,
        JANGO = 2,
        SHADOW_SMITTY = 3,
        MONK = 4,
        NIGHTINGALE = 5,
        HAZEL = 6,
        WINTER = 7,
        GOLDEN_SMITTY = 8
    }
}
