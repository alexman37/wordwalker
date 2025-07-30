using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConfirmResizePopup : WidgetPopup
{
    const int timeToRevert = 10;
    private bool confirmed = false;
    ScreenSizeSetting proposedNewSize;
    IEnumerator oneTickingRoutine;
    private bool notARepeat = true;

    public TextMeshProUGUI textTimeField;

    public SettingsMenu settingsMenu;

    // Start is called before the first frame update
    void Start()
    {
        this.Setup();
    }

    private void OnEnable()
    {
        SettingsMenu.toggledScreenSize += beginScreenResize;
    }

    private void OnDisable()
    {
        SettingsMenu.toggledScreenSize -= beginScreenResize;
    }

    public void beginScreenResize(ScreenSizeSetting newSize)
    {
        // revertAndClose() dispatches the same action we are subscribed to here, meaning we would get caught up
        // in an infinite loop of resizing / not resizing unless we explicitly look out for that.
        if(notARepeat)
        {
            this.openWidgetPopupSafe();

            confirmed = false;
            proposedNewSize = newSize;

            // reset coroutine
            if (oneTickingRoutine != null) StopCoroutine(oneTickingRoutine);
            oneTickingRoutine = tickDownToRevert();
            StartCoroutine(oneTickingRoutine);
        } else
        {
            notARepeat = true;
        }
        
    }

    // Need to get desired screen size from an action, probably
    public void confirmAndClose()
    {
        Debug.Log("Confirmed new screen size.");
        confirmed = true;
        GlobalStatMap.ModifyScreenSize(proposedNewSize);
        base.closeWidgetPopupSafe();
    }

    public void revertAndClose()
    {
        Debug.Log("Reverted to old screen size.");
        notARepeat = false;
        settingsMenu.selectScreenSize(GlobalStatMap.loadGlobalStatMap().settingsValues.screenSizeSetting);
        base.closeWidgetPopupSafe();
    }

    IEnumerator tickDownToRevert()
    {
        for(int s = timeToRevert; s > 0 && !confirmed; s--)
        {
            textTimeField.text = $"Will automatically revert in {s} seconds.";
            yield return new WaitForSeconds(1);
        }

        // If the timer runs out and you still haven't confirmed new screen size, revert to the old one
        if(!confirmed)
        {
            revertAndClose();
        }
    }
}
