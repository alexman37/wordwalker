using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordwalkerUIScript : MonoBehaviour
{
    public GameObject critStats;
    public GameObject topBar;
    public GameObject scrollClue;
    public GameObject inventory;

    //TODO: not in final product
    public GameObject debugRegen;

    // Start is called before the first frame update
    void Start()
    {
        critStats.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.05f, 0.05f), ScalingUIComponent.Position.TOP_LEFT);
        topBar.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0f, 0.1f), ScalingUIComponent.Position.TOP);
        scrollClue.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0f, 0.05f), ScalingUIComponent.Position.BOTTOM);
        debugRegen.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.05f, 0.05f), ScalingUIComponent.Position.TOP_RIGHT);
        inventory.GetComponent<ScalingUIComponent>().proportionalSetLoc(new Vector2(0.1f, 0.1f), ScalingUIComponent.Position.BOTTOM_LEFT);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
