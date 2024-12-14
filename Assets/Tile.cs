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
    public bool isBackRow;

    //for pushing down animation
    private float pushDownDistance = 0.5f;
    private float pushDownTime = 2f;
    private float pushDownTimeElapsed = 0;
    private bool pushingDown = false;
    private Vector3 pushDownTarget;

    public static event Action fallAllTiles;

    //Order of adjacencies is CLOCKWISE FROM EAST
    //E, SE, SW, W, NW, NE
    public List<Adjacency> adjacencies = new List<Adjacency>();

    //When a tile is clicked you propogate it to the WalkManager
    public static event Action<Tile> tileClicked;

    // Start is called before the first frame update
    void Start()
    {
        fallAllTiles += fall;
    }

    // Update is called once per frame
    void Update()
    {
        if (pushingDown)
        {
            pushDownTimeElapsed += Time.deltaTime;
            Vector3 currPos = this.gameObject.transform.position;
            this.gameObject.transform.position = new Vector3(currPos.x, Vector3.Lerp(transform.position, pushDownTarget, pushDownTimeElapsed).y, currPos.z);
            if(pushDownTimeElapsed > pushDownTime)
            {
                pushingDown = false;
            }
        }
    }

    //Will either push down for a correct guess, or fall off for an incorrect one
    public void pressAnimation()
    {
        if (correct) correctPress();
        else incorrectPress();
    }

    //Slowly "push down" into place
    private void correctPress()
    {
        pushingDown = true;
        pushDownTimeElapsed = 0;
        pushDownTarget = setPushDownTarget();
    }

    //Have the tile fall downwards in a random direction
    private void incorrectPress()
    {
        fallAllTiles.Invoke();
    }

    private void fall()
    {
        if(this.physicalObject != null)
        {
            if (!this.correct)
            {
                Rigidbody rbody = this.physicalObject.GetComponent<Rigidbody>();
                rbody.constraints = RigidbodyConstraints.None;
                rbody.useGravity = true;
                rbody.AddForce(new Vector3(UnityEngine.Random.Range(-400, 400), 0, UnityEngine.Random.Range(-400, 400)));
                rbody.AddTorque(new Vector3(UnityEngine.Random.Range(-400, 400), 0, UnityEngine.Random.Range(-400, 400)));
            } else
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }
        }
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

    private Vector3 setPushDownTarget()
    {
        return new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - pushDownDistance, gameObject.transform.position.z);
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
