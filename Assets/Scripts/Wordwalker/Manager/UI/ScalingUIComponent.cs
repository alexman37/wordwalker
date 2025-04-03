using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScalingUIComponent : MonoBehaviour
{
    public Position focalPoint; // Where is the "pivot" of this component
    public Vector2 percentPosition; // Percentage (0-1) of where in screen space (x,y) this should be located
    public Vector2 percentScale; // Percentage (0-1) of how much of the screen space (x,y) this component takes up

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
                break;
            case Position.TOP_LEFT:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                break;
            case Position.TOP_RIGHT:
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                break;
            case Position.BOTTOM:
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                break;
            case Position.BOTTOM_LEFT:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                break;
            case Position.BOTTOM_RIGHT:
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 0);
                break;
            case Position.CENTER:
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                break;
            case Position.LEFT:
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                break;
            case Position.RIGHT:
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                break;
        }


        // Now set position proper
        switch (focalPoint)
        {
            case Position.TOP:
            case Position.TOP_LEFT:
            case Position.TOP_RIGHT:
                newLoc.y = screenSpace.height * percentPosition.y;
                break;
            case Position.BOTTOM:
            case Position.BOTTOM_LEFT:
            case Position.BOTTOM_RIGHT:
                newLoc.y = - screenSpace.height * (1 - percentPosition.y);
                break;
            case Position.CENTER:
            case Position.LEFT:
            case Position.RIGHT:
                newLoc.y = screenSpace.height * (percentPosition.y - 0.5f);
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
                newLoc.x = screenSpace.width * (percentPosition.x - 0.5f);
                break;
        }

        //Set scale
        rect.sizeDelta = new Vector2(screenSpace.width * percentScale.x, screenSpace.height * percentScale.y);
        //rect.localScale = new Vector3(rect.localScale.x * newScale, rect.localScale.y * newScale, rect.localScale.z * newScale);

        rect.anchoredPosition = newLoc;
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
