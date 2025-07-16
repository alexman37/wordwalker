using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharSelectPopup : WidgetPopup
{
    public static CharacterSprite activeCharSprite;
    public GameObject titleSpriteObj;
    public GameObject selectionFrame;
    public RectTransform[] picturePositions;
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        this.Setup();

        anim = titleSpriteObj.GetComponent<Animator>();

        // Get last used character, or use smitty as the default
        Dictionary<string, int> intMap = GlobalStatMap.loadGlobalStatMap().intMap;
        if (intMap.ContainsKey("activeCharSprite"))
        {
            activeCharSprite = (CharacterSprite)intMap["activeCharSprite"];
        }
        else activeCharSprite = CharacterSprite.SMITTY;

        changeTitleAnimation();
        changeSelectionFramePos();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void useNewCharSprite(int newVal)
    {
        activeCharSprite = (CharacterSprite) newVal;
        GlobalStatMap.AddOrModifyInt("activeCharSprite", (int) activeCharSprite);
        GlobalStatMap.saveGlobalStatMap();

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

    //Change the position of the selection frame
    //Kind of janky...right now we basically hard code it knowing where it'd be...
    void changeSelectionFramePos()
    {
        int i = (int)activeCharSprite;
        selectionFrame.GetComponent<RectTransform>().localPosition = picturePositions[i].localPosition;
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
