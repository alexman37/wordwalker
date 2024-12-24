using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingUIComponent : MonoBehaviour
{
    public Position focalPoint;
    private RectTransform rect;

    private Rect screenSpace;
    private const float typicalHeight = 707; //TODO: adjust?

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

    // Given as floats from 0-1: the percent relative to screenSpace
    public void proportionalSetLoc(Vector2 desiredLoc, Position relativeTo)
    {
        if(!rect) Start();
        float newScale = screenSpace.height / typicalHeight;
        Vector2 newLoc = new Vector2(0, 0);

        switch (relativeTo)
        {
            case Position.TOP:
            case Position.TOP_LEFT:
            case Position.TOP_RIGHT:
                newLoc.y = newLoc.y - screenSpace.height / 2.0f * desiredLoc.y;
                break;
            case Position.BOTTOM:
            case Position.BOTTOM_LEFT:
            case Position.BOTTOM_RIGHT:
                newLoc.y = newLoc.y + screenSpace.height / 2.0f * desiredLoc.y;
                break;
            case Position.CENTER:
            case Position.LEFT:
            case Position.RIGHT:
                break;
        }

        switch(relativeTo)
        {
            case Position.LEFT:
            case Position.TOP_LEFT:
            case Position.BOTTOM_LEFT:
                newLoc.x = newLoc.x + screenSpace.width / 2.0f * desiredLoc.x;
                break;
            case Position.RIGHT:
            case Position.BOTTOM_RIGHT:
            case Position.TOP_RIGHT:
                newLoc.x = newLoc.x - screenSpace.width / 2.0f * desiredLoc.x;
                break;
            case Position.CENTER:
            case Position.TOP:
            case Position.BOTTOM:
                break;
        }

        rect.localScale = new Vector3(rect.localScale.x * newScale, rect.localScale.y * newScale, rect.localScale.z * newScale);

        Vector2 finalLoc = newLoc;

        rect.anchoredPosition = finalLoc;
    }



    


    // Start is called before the first frame update
    void Start()
    {
        rect = this.GetComponent<RectTransform>();
        screenSpace = Screen.safeArea;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
