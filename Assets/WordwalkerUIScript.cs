using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordwalkerUIScript : MonoBehaviour
{
    public GameObject itemCount;
    public GameObject topBar;
    public GameObject scrollClue;

    //TODO: not in final product
    public GameObject debugRegen;

    // Start is called before the first frame update
    void Start()
    {
        itemCount.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.05f, 0.05f), ScalingUIComponent.Position.TOP_LEFT);
        topBar.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0f, 0.3f), ScalingUIComponent.Position.BOTTOM);
        scrollClue.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0f, 0.1f), ScalingUIComponent.Position.BOTTOM);
        debugRegen.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.05f, 0.05f), ScalingUIComponent.Position.TOP_RIGHT);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
