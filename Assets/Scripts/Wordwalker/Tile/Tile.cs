using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Attach to a physical tile object in the world. tracks all its stats.
/// </summary>
public class Tile : MonoBehaviour
{
    // STATS
    public char letter = ' ';               // Letter this tile represents
    public (float, float) absolutePosition; // "World" position
    public Coordinate coords;               // Coordinate system works in (row, space)

    bool finalized;          // (used solely in generation)
    public bool stepped;     // Has the tile been stepped on? If correct, you may walk on it again at any time.
    public bool marked;      // When marked as dangerous this tile cannot be stepped on until unmarked
    public bool correct;     // Is this tile part of the correct word path?
    public bool isBackRow;   // Is this tile in the back row? (If correct, you win.)

    public SpecialTile specType; // If special then some of its behavior is changed
    public static float fakeTileLyingChance = 0.50f;

    // Note to self- we don't store local copies of the materials here bc it would be needlessly expensive.

    public List<Adjacency> adjacencies = new List<Adjacency>(); // Order of adjacencies is CLOCKWISE FROM EAST (E, SE, SW, W, NW, NE)

    // PHYSICAL
    public GameObject physicalObject;       // Physical tile object - move it, change it, etc
    TextMeshProUGUI textComponent;          // Where the tile's letter is drawn

    // ACTIONS
    public static event Action fallAllTiles;      // When winning a round or losing the game, all incorrect tiles fall down
    public static event Action<Tile> tileClicked; // When a tile is clicked you propogate it to the WalkManager


    private void Start()
    {
        textComponent = this.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Sub / unsub to actions
    private void OnEnable()
    {
        fallAllTiles += fall;
    }

    private void OnDisable()
    {
        fallAllTiles -= fall;
    }


    // TODO: When you click on a tile one of a number of things can happen
    public void OnMouseDown()
    {
        Debug.Log("Clicked on " + this.ToString());
        tileClicked.Invoke(this);
    }


    //Will either push down for a correct guess, or fall off for an incorrect one
    public void pressAnimation()
    {
        if (correct) correctPress();
        else incorrectPress();
    }

    /// <summary>
    /// Run this when you've stepped on a correct tile
    /// </summary>
    private void correctPress()
    {
        StartCoroutine(pushDownTile());
    }

    /// <summary>
    /// Run this when you step on an incorrect tile
    /// </summary>
    private void incorrectPress()
    {
        StartCoroutine(fallTile());
    }

    /// <summary>
    /// Tile falling animation
    /// We also do the "fall all" animation here, if you lose the game
    /// </summary>
    private IEnumerator fallTile()
    {
        fall();

        if(GameManagerSc.getNumTotems() <= 0)
        {
            // First wait one second, so you realize you done goofed
            yield return new WaitForSeconds(1.5f);

            fallAllTiles.Invoke();
        }
    }

    /// <summary>
    /// Push tile down if correct. Simple pressing animation.
    /// </summary>
    private IEnumerator pushDownTile()
    {
        float steps = 50;
        float timeSec = 2f;

        float pushDownDistance = 0.5f;

        Vector3 currPos = this.gameObject.transform.position;
        for (float i = 0; i <= steps; i++)
        {
            this.gameObject.transform.position = new Vector3(currPos.x, currPos.y - (1 / steps) * pushDownDistance, currPos.z);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return null;
    }

    /// <summary>
    /// Mark tile as dangerous. Do not allow players to step on it
    /// The opposite if the tile is already marked
    /// </summary>
    public void markAsDangerous(Material changeTo)
    {
        Debug.Log("MARKING");
        marked = true;
        changeMaterial(changeTo);
    }

    public void unmarkAsDangerous(Material changeTo)
    {
        Debug.Log("UNMARKING");
        marked = false;
        changeMaterial(changeTo);
    }

    /// <summary>
    /// Make this tile fall down into the chasm
    /// </summary>
    private void fall()
    {
        if(this.physicalObject != null)
        {
            if (!this.correct)
            {
                Rigidbody rbody = this.physicalObject.GetComponent<Rigidbody>();
                rbody.constraints = RigidbodyConstraints.None;
                rbody.useGravity = true;
                rbody.AddForce(new Vector3(UnityEngine.Random.Range(-400, 100), 0, UnityEngine.Random.Range(-400, 100)));
                rbody.AddTorque(new Vector3(UnityEngine.Random.Range(-400, 400), 0, UnityEngine.Random.Range(-400, 400)));
            } else
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Set the letter of this tile (ON GENERATION ONLY)
    /// Once finalized it cannot be changed again (duh)
    /// </summary>
    public void setLetter(char setTo, bool isPartOfPath)
    {
        if(textComponent == null) textComponent = this.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        letter = setTo;
        textComponent.text = setTo.ToString();

        finalized = true;
        correct = isPartOfPath;

        specType = SpecialTile.NONE;
    }

    public void setAsSpecialTile(SpecialTile specType)
    {
        if(textComponent == null) textComponent = this.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        this.specType = specType;

        switch (specType)
        {
            // Random: Appears as "?", value hidden until stepped on
            case SpecialTile.RANDOM:
                textComponent.text = "?";
                break;
            // Blank: Appears as nothing and can always safely be stepped on
            case SpecialTile.BLANK:
                textComponent.text = " ";
                break;
            // Fake: Has a certain chance (configure...) of appearing as something else
            case SpecialTile.FAKE:
                textComponent.fontSize = 0.9f;
                if(UnityEngine.Random.value < fakeTileLyingChance)
                {
                    // The tile lies
                    textComponent.text = "\"" + LetterGen.getProportionallyRandomLetter() + "\"";
                } else
                {
                    // The tile tells the truth
                    textComponent.text = "\"" + letter + "\"";
                }
                
                break;
            case SpecialTile.SPLIT:
                // TODO
                break;
        }
    }

    /// <summary>
    /// Highlight surrounding tile (if not stepped on or marked)
    /// </summary>
    public void highlightMaterial(Material changeTo)
    {
        if(!stepped && !marked)
        {
            changeMaterial(changeTo);
        }
    }

    /// <summary>
    /// Step on this tile and change the material
    /// </summary>
    public void stepMaterial(Material changeTo)
    {
        stepped = true;
        changeMaterial(changeTo);
    }

    /// <summary>
    /// Change the material of this tile
    /// </summary>
    public void changeMaterial(Material changeTo)
    {
        Material[] mats = physicalObject.GetComponent<MeshRenderer>().materials;
        mats[1] = changeTo;
        physicalObject.GetComponent<MeshRenderer>().materials = mats;
    }

    /// <summary>
    /// Change the material of this tile to whatever the default is
    /// </summary>
    /*public void currentBaseMaterial()
    {
        Material[] mats = physicalObject.GetComponent<MeshRenderer>().materials;
        Material changeTo;


        mats[1] = changeTo;
        physicalObject.GetComponent<MeshRenderer>().materials = mats;
    }*/



    public bool isFinalized()
    {
        return finalized;
    }

    public override string ToString()
    {
        return "Tile at " + coords + ": [" + letter + "]";
    }

    public enum SpecialTile
    {
        NONE,
        RANDOM,
        SPLIT,
        FAKE,
        BLANK
    }
}
