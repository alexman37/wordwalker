using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemsScript : MonoBehaviour
{
    // Reveal a correct tile
    public static event Action greenItemUsed;

    // Reveal some number of incorrect tiles
    public static event Action redItemUsed;

    // Jump as many as 3 tiles away
    public static event Action blueItemUsed;

    [SerializeField] private AudioClip greenItemClip;
    [SerializeField] private AudioClip redItemClip;
    [SerializeField] private AudioClip blueItemClip;


    public void useAnyItem(ItemType item)
    {
        switch(item)
        {
            case ItemType.REVEAL_CORRECT:
                useGreenItem();
                break;
            case ItemType.REVEAL_INCORRECTS:
                useRedItem();
                break;
            case ItemType.JUMP:
                useBlueItem();
                break;
        }
    }

    public void useGreenItem()
    {
        greenItemUsed.Invoke();

        // TODO move it to after the animation plays
        SfxManager.instance.playSFX(greenItemClip, null, 1f);
    }

    public void useRedItem()
    {
        redItemUsed.Invoke();
    }

    public void useBlueItem()
    {
        blueItemUsed.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    public enum ItemType
    {
        REVEAL_CORRECT,
        REVEAL_INCORRECTS,
        JUMP
    }
}
