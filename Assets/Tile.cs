using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Tile : MonoBehaviour
{
    public char letter = ' ';
    public (float, float) absolutePosition;
    public Coordinate coords;
    public GameObject physicalObject;

    TextMeshProUGUI textComponent;
    bool finalized;
    public bool stepped;
    public bool correct;

    //Order of adjacencies is CLOCKWISE FROM EAST
    //E, SE, SW, W, NW, NE
    public List<Adjacency> adjacencies = new List<Adjacency>();

    //When a tile is clicked you propogate it to the WalkManager
    public static event Action<Tile> tileClicked;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setLetter(char setTo, bool isPartOfPath)
    {
        textComponent = this.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        letter = setTo;
        textComponent.text = setTo.ToString();

        finalized = true;
        correct = isPartOfPath;
    }

    public void highlightMaterial(Material changeTo)
    {
        if(!stepped)
            physicalObject.GetComponent<MeshRenderer>().material = changeTo;
    }

    public void stepMaterial(Material changeTo)
    {
        stepped = true;
        physicalObject.GetComponent<MeshRenderer>().material = changeTo;
    }

    public bool isFinalized()
    {
        return finalized;
    }

    public void OnMouseDown()
    {
        tileClicked.Invoke(this);
        Debug.Log(this);
    }

    public override string ToString()
    {
        return "Tile at " + coords + ": [" + letter + "]";
    }
}
