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
    public static event Action blueItemCancelled;

    [SerializeField] private AudioClip greenItemClip;
    [SerializeField] private AudioClip redItemClip;
    [SerializeField] private AudioClip blueItemClip;

    public PlayerManager playerManager;
    public AnimationManager animationManager;

    private bool inItemAnimation = false;


    public void useAnyItem(ItemType item)
    {
        if(GameManagerSc.getNumTotems() <= 0)
        {
            Debug.LogWarning("Could not use the item - all out of totems!");
            // TODO close / prevent further uses
            return;
        }

        else
        {
            GameManagerSc.changeTotems(1, false);

            switch (item)
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

            if(GameManagerSc.getNumTotems() <= 0)
            {
                // TODO close / prevent further uses

            }
        }
    }

    private bool genericItemUsed()
    {
        if(GameManagerSc.getNumTotems() <= 0)
        {
            Debug.LogWarning("Could not use the item - all out of totems!");
            return false;
        } else
        {
            GameManagerSc.state.funStuff.itemsUsed += 1;
            GameManagerSc.changeTotems(1, false);
            return true;
        }
    }

    public void useGreenItem()
    {
        if(!inItemAnimation)
        {
            if(genericItemUsed())
            {
                inItemAnimation = true;
                StartCoroutine(greenItemCo());
            }
        }
    }

    IEnumerator greenItemCo()
    {
        playerManager.setFreeCamera(false);

        playerManager.walterWhitePan(1.5f);
        yield return new WaitForSeconds(2.5f);

        greenItemUsed.Invoke();
        SfxManager.instance.playSFX(greenItemClip, null, 1f);
        yield return new WaitForSeconds(1f);

        Vector3 nextPos = animationManager.playerCharacter.transform.position;
        nextPos.y = 18;
        playerManager.XerpCameraTo(nextPos, 1f, true);
        yield return new WaitForSeconds(1f);

        inItemAnimation = false;
        playerManager.setFreeCamera(true);
    }

    public void useRedItem()
    {
        if (!inItemAnimation)
        {
            if (genericItemUsed())
            {
                inItemAnimation = true;
                StartCoroutine(redItemCo());
            }
        }
    }

    IEnumerator redItemCo()
    {
        playerManager.setFreeCamera(false);

        playerManager.walterWhitePan(1.5f);
        yield return new WaitForSeconds(2.5f);

        redItemUsed.Invoke();
        SfxManager.instance.playSFX(redItemClip, null, 1f);
        yield return new WaitForSeconds(1f);

        Vector3 nextPos = animationManager.playerCharacter.transform.position;
        nextPos.y = 18;
        playerManager.XerpCameraTo(nextPos, 1f, true);
        yield return new WaitForSeconds(1f);

        inItemAnimation = false;
        playerManager.setFreeCamera(true);
    }

    public void useBlueItem()
    {
        if (!inItemAnimation)
        {
            if(WalkManager.jumping)
            {
                blueItemCancelled.Invoke();
            }
            else
            {
                // jumping is special because you can cancel it. you don't use the totem until after you've committed
                if (GameManagerSc.getNumTotems() > 0)
                {
                    blueItemUsed.Invoke();
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        greenItemUsed += () => { };
        redItemUsed += () => { };
        blueItemUsed += () => { };
        blueItemCancelled += () => { };
    }

    
    public enum ItemType
    {
        REVEAL_CORRECT,
        REVEAL_INCORRECTS,
        JUMP
    }
}
