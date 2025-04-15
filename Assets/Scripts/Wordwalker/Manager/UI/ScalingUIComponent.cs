using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Attach a ScalingUIComponent to any UI object to reposition and rescale it relative to the (unknown until runtime)
/// size of the screen.
/// </summary>
/// The following properties are used:
/// - focalPoint: The pivot of the component
/// - percentPosition: Percentage (0-1) of where in screen space (x,y) this component should be located
/// - percentScale: Percentage (0-1) of how much of the screen space (x,y) this component takes up
/// - maintainAspectRatio: If true, the scale of Y is automatically set to the scale of X.
/// - constantSize: If true, the scale will not be changed at all (the component is 'pixel perfect').
public class ScalingUIComponent : MonoBehaviour
{
    public bool DONE = false; // Needed in case the ScalingUIComponent completes creating before UI elements can subscribe to below
    public event Action completedScaling;

    public Position focalPoint; // Where is the "pivot" of this component
    public Vector2 percentPosition; // Percentage (0-1) of where in screen space (x,y) this should be located
    public Vector2 percentScale; // Percentage (0-1) of how much of the screen space (x,y) this component takes up
    public bool maintainAspectRatioX;
    public bool maintainAspectRatioY;
    public bool constantSize;

    private RectTransform rect;
    private Rect screenSpace;

    /// <summary>
    /// Pivot point for a UI object.
    /// </summary>
    public enum Position
    {
        CENTER,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
        BOTTOM,
        TOP,
        LEFT,
        RIGHT
    }

    /// <summary>
    /// Set position and scale of this UI component- usually called on startup
    /// </summary>
    public void proportionalSetLoc()
    {
        if(!rect) Start();
        Vector2 newLoc = new Vector2(0, 0);

        // First set anchored position
        switch (focalPoint)
        {
            case Position.TOP:
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                break;
            case Position.TOP_LEFT:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                break;
            case Position.TOP_RIGHT:
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                break;
            case Position.BOTTOM:
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                rect.pivot = new Vector2(0.5f, 0);
                break;
            case Position.BOTTOM_LEFT:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                break;
            case Position.BOTTOM_RIGHT:
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                break;
            case Position.CENTER:
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;
            case Position.LEFT:
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                break;
            case Position.RIGHT:
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.pivot = new Vector2(1, 0.5f);
                break;
        }


        // Now set position proper
        switch (focalPoint)
        {
            case Position.TOP:
            case Position.TOP_LEFT:
            case Position.TOP_RIGHT:
                newLoc.y = -screenSpace.height * (1 - percentPosition.y) - (screenSpace.yMax - (screenSpace.yMin + screenSpace.height));
                break;
            case Position.BOTTOM:
            case Position.BOTTOM_LEFT:
            case Position.BOTTOM_RIGHT:
                newLoc.y = screenSpace.height * percentPosition.y + screenSpace.yMin;
                break;
            case Position.CENTER:
            case Position.LEFT:
            case Position.RIGHT:
                newLoc.y = screenSpace.height / 2 * ((percentPosition.y - 0.5f) * 2);
                break;
        }
        switch (focalPoint)
        {
            case Position.LEFT:
            case Position.TOP_LEFT:
            case Position.BOTTOM_LEFT:
                newLoc.x = screenSpace.width * percentPosition.x + screenSpace.xMin;
                break;
            case Position.RIGHT:
            case Position.BOTTOM_RIGHT:
            case Position.TOP_RIGHT:
                newLoc.x = -screenSpace.width * (1 - percentPosition.x) - (screenSpace.xMax - (screenSpace.xMin + screenSpace.width));
                break;
            case Position.CENTER:
            case Position.TOP:
            case Position.BOTTOM:
                newLoc.x = screenSpace.width / 2 * ((percentPosition.x - 0.5f) * 2);
                break;
        }

        //Set scale only if we want to
        if (percentScale.x != -1 && !constantSize)
        {
            Vector2 oldDims = new Vector2(rect.rect.width, rect.rect.height);
            if(maintainAspectRatioX)
            {
                // Height may not match up with inputted scale if you chose to scale by aspect ratio
                float aspectedHeight = rect.rect.height / rect.rect.width * screenSpace.width * percentScale.x;
                rect.sizeDelta = new Vector2(screenSpace.width * percentScale.x, aspectedHeight);
            } else if(maintainAspectRatioY)
            {
                // Similar story for width if you base the aspect ratio on Y
                float aspectedWidth = rect.rect.width / rect.rect.height * screenSpace.height * percentScale.y;
                rect.sizeDelta = new Vector2(aspectedWidth, screenSpace.height * percentScale.y);
            }
            else {
                rect.sizeDelta = new Vector2(screenSpace.width * percentScale.x, screenSpace.height * percentScale.y);
            }
            
            Vector2 newDims = new Vector2(rect.rect.width, rect.rect.height);

            for (int i = 0; i < this.transform.childCount; i++)
            {
                recursiveResizeChildren(this.transform.GetChild(i).GetComponent<RectTransform>(), oldDims, newDims);
            }
        }

        rect.anchoredPosition = newLoc;
        DONE = true;
        completedScaling.Invoke();
    }

    // Call on each child
    private void recursiveResizeChildren(RectTransform resizeMeAndMyKids, Vector2 oldDims, Vector2 newDims)
    {
        Vector2 ratio = newDims / oldDims;

        Vector2 thisOldDims = new Vector2(resizeMeAndMyKids.rect.width, resizeMeAndMyKids.rect.height);
        resizeMeAndMyKids.anchoredPosition *= ratio;
        resizeMeAndMyKids.sizeDelta *= ratio;
        Vector2 thisNewDims = new Vector2(resizeMeAndMyKids.rect.width, resizeMeAndMyKids.rect.height);

        // A side effect: If this component is text you must scale the font size accordingly
        // This takes both x and y into account
        TextMeshProUGUI possibleText = resizeMeAndMyKids.GetComponent<TextMeshProUGUI>();
        if(possibleText != null)
        {
            // For now just use whichever is more "extreme"

            float xExtremity, yExtremity;
            if (ratio.x < 1) xExtremity = 1 / ratio.x; else xExtremity = ratio.x;
            if (ratio.y < 1) yExtremity = 1 / ratio.y; else yExtremity = ratio.y;
            if (xExtremity > yExtremity) possibleText.fontSize *= ratio.x; else possibleText.fontSize *= ratio.y;
        }

        // And each child of the child...etc
        for (int i = 0; i < resizeMeAndMyKids.transform.childCount; i++)
        {
            recursiveResizeChildren(resizeMeAndMyKids.GetChild(i).GetComponent<RectTransform>(), thisOldDims, thisNewDims);
        }
    }



    


    // Start is called before the first frame update
    void Start()
    {
        completedScaling += () => { };

        rect = this.GetComponent<RectTransform>();
        screenSpace = Screen.safeArea;

        proportionalSetLoc();
    }

}
