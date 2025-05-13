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
    public string display = " ";            // What the tile shows - sometimes different from letter if it's a "Special" tile
    public (float, float) absolutePosition; // "World" position
    public Coordinate coords;               // Coordinate system works in (row, space)

    bool finalized;          // (used solely in generation)
    public bool stepped;     // Has the tile been stepped on? If correct, you may walk on it again at any time.
    public bool marked;      // When marked as dangerous this tile cannot be stepped on until unmarked
    public bool correct;     // Is this tile part of the correct word path?
    public bool isBackRow;   // Is this tile in the back row? (If correct, you win.)

    public SpecialTile specType = SpecialTile.NONE; // If special then some of its behavior is changed
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
        GameManagerSc.levelWon += startFlipTile;
        if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.FOG))
        {
            WalkManager.atCurrentRow += determineFogginess;
        }
    }

    private void OnDisable()
    {
        fallAllTiles -= fall;
        GameManagerSc.levelWon -= startFlipTile;
        WalkManager.atCurrentRow -= determineFogginess;
    }


    // TODO: When you click on a tile one of a number of things can happen
    public void OnMouseDown()
    {
        Debug.Log("Clicked on " + this.ToString());
        tileClicked.Invoke(this);
    }

    // When a tile falls and hits the bottom of the floor we'll just remove it
    /*private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "FloorBottom")
        {
            Destroy(this.gameObject);
        }
    }*/

    public static void triggerFallAllTiles() { fallAllTiles.Invoke(); }


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
        Debug.Log("Correct press");
        if(specType != SpecialTile.NONE)
        {
            textComponent.text = display.ToString();
        }
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
    /// If you win a round, all incorrect tiles will flip over.
    /// </summary>
    /// <returns></returns>
    private void startFlipTile() { StartCoroutine(flipTile()); }

    private IEnumerator flipTile()
    {
        if(coords != null && (!correct || specType == Tile.SpecialTile.BLANK))
        {
            float steps = 50;
            float timeSec = 1.5f;

            Quaternion start = Quaternion.Euler(-90, 0, 0);
            Quaternion end = Quaternion.Euler(90, 0, 0);

            yield return new WaitForSeconds(coords.r * 0.25f);

            for (float i = 0; i <= steps; i++)
            {
                this.gameObject.transform.rotation = Quaternion.Lerp(start, end, i / steps);
                yield return new WaitForSeconds(1 / steps * timeSec);
            }
        }

        yield return null;
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
                // Correct tiles, we don't want incorrect ones to "stay" on top of them - so activate the invis wall
                transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// (If fog challenge enabled) Keep / make all tiles past (passed in row + foggyDistance) foggy.
    /// WalkManager is supposed to do the calculations to make this value consistently increasing.
    /// </summary>
    private void determineFogginess(int row)
    {
        //TODO change material as well
        if(this.coords != null)
        {
            if (this.coords.r - row > GameManagerSc.foggyVision)
            {
                textComponent.text = "";
                changeMaterial(WalkManager.tileMats.fog);
            }
            else
            {
                textComponent.text = display.ToString();
                changeMaterial(WalkManager.tileMats.getCurrentBase(marked, stepped, correct, specType));
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
        display = setTo.ToString();
        textComponent.text = setTo.ToString();

        finalized = true;
        correct = isPartOfPath;
    }

    /// <summary>
    /// You've chosen to make this a special tile - adjust its appearance / behavior here
    /// </summary>
    public void setAsSpecialTile(SpecialTile specType)
    {
        if(textComponent == null) textComponent = this.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        this.specType = specType;

        switch (specType)
        {
            // Random: Appears as "?", value hidden until stepped on
            case SpecialTile.RANDOM:
                display = "?";
                break;
            // Blank: Appears as nothing and can always safely be stepped on
            case SpecialTile.BLANK:
                letter = ' ';
                display = " ";
                correct = true;
                break;
            // Fake: Has a certain chance (configure...) of appearing as something else
            case SpecialTile.FAKE:
                textComponent.fontSize = 0.9f;
                if(UnityEngine.Random.value < fakeTileLyingChance)
                {
                    // The tile lies
                    display = "\"" + LetterGen.getProportionallyRandomLetter() + "\"";
                } else
                {
                    // The tile tells the truth
                    display = "\"" + letter + "\"";
                }
                
                break;
            case SpecialTile.SPLIT:
                textComponent.fontSize = 0.8f;
                // Will either have the correct tile on top or the bottom
                if (UnityEngine.Random.value < 0.5f)
                {
                    display = LetterGen.getProportionallyRandomLetter() + "\n" + letter;
                } else
                {
                    display = letter + "\n" + LetterGen.getProportionallyRandomLetter();
                }
                
                break;
        }

        textComponent.text = display;
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
