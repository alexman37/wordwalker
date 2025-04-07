using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScalingUIComponent : MonoBehaviour
{
    public Position focalPoint; // Where is the "pivot" of this component
    public Vector2 percentPosition; // Percentage (0-1) of where in screen space (x,y) this should be located
    public Vector2 percentScale; // Percentage (0-1) of how much of the screen space (x,y) this component takes up
    public bool maintainAspectRatio;

    private RectTransform rect;
    private Rect screenSpace;

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
                newLoc.y = - screenSpace.height * (1 - percentPosition.y);
                break;
            case Position.BOTTOM:
            case Position.BOTTOM_LEFT:
            case Position.BOTTOM_RIGHT:
                newLoc.y = screenSpace.height * percentPosition.y;
                break;
            case Position.CENTER:
            case Position.LEFT:
            case Position.RIGHT:
                newLoc.y = screenSpace.height / 2 * (percentPosition.y - 0.5f);
                break;
        }
        switch (focalPoint)
        {
            case Position.LEFT:
            case Position.TOP_LEFT:
            case Position.BOTTOM_LEFT:
                newLoc.x = screenSpace.width * percentPosition.x;
                break;
            case Position.RIGHT:
            case Position.BOTTOM_RIGHT:
            case Position.TOP_RIGHT:
                newLoc.x = - screenSpace.width * (1 - percentPosition.x);
                break;
            case Position.CENTER:
            case Position.TOP:
            case Position.BOTTOM:
                newLoc.x = screenSpace.width / 2 * (percentPosition.x - 0.5f);
                break;
        }

        //Set scale only if we want to
        if(percentScale.x != -1)
        {
            Vector2 oldDims = new Vector2(rect.rect.width, rect.rect.height);
            if(maintainAspectRatio)
            {
                float aspectedHeight = rect.rect.height / rect.rect.width * screenSpace.width * percentScale.x;
                rect.sizeDelta = new Vector2(screenSpace.width * percentScale.x, aspectedHeight);
            } else
            {
                rect.sizeDelta = new Vector2(screenSpace.width * percentScale.x, screenSpace.height * percentScale.y);
            }
            
            Vector2 newDims = new Vector2(rect.rect.width, rect.rect.height);

            //resizeChildren(oldWidth, oldHeight);
            for (int i = 0; i < this.transform.childCount; i++)
            {
                recursiveResizeChildren(this.transform.GetChild(i).GetComponent<RectTransform>(), oldDims, newDims);
            }
        }

        rect.anchoredPosition = newLoc;
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
        rect = this.GetComponent<RectTransform>();
        screenSpace = Screen.safeArea;

        proportionalSetLoc();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
