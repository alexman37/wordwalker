using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

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

    private static bool acceptingClicks = true;

    bool finalized;          // (used solely in generation)
    public bool banned;      // You cannot interact with this tile no matter what (only special use cases)
    public bool stepped;     // Has the tile been stepped on? If correct, you may walk on it again at any time.
    public bool revealed;    // You know if the tile is correct or not.
    public bool marked;      // When marked as dangerous this tile cannot be stepped on until unmarked
    public bool correct;     // Is this tile part of the correct word path?
    public bool isBackRow;   // Is this tile in the back row? (If correct, you win.)

    public SpecialTile specType = SpecialTile.NONE; // If special then some of its behavior is changed
    public static float fakeTileLyingChance = 0.50f;

    // Note to self- we don't store local copies of the materials here bc it would be needlessly expensive.

    public List<Adjacency> adjacencies = new List<Adjacency>(); // Order of adjacencies is CLOCKWISE FROM EAST (E, SE, SW, W, NW, NE)

    // PHYSICAL
    public GameObject physicalObject;       // Physical tile object - move it, change it, etc
    Image timeFader;                        // When this tile is close to falling we fade an image over it (rather than highlight it)
    TextMeshProUGUI textComponent;          // Where the tile's letter is drawn

    // ACTIONS
    public static event Action<bool, bool> fallAllTiles;      // When losing the game, all incorrect tiles fall down
    public static event Action<Tile> tileClicked; // When a tile is clicked you propogate it to the WalkManager


    private void Start()
    {
        textComponent = this.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

        if (GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.TIMER)) {
            timeFader = this.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            timeFader.gameObject.SetActive(true);
        }
    }

    // Sub / unsub to actions
    private void OnEnable()
    {
        fallAllTiles += fall;
        GameManagerSc.levelWon += startFlipTile;
        TimeManager.timerExpired += fallIfInRow;
        TimeManager.timerExpired += fadeWarning;
        if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.FOG))
        {
            WalkManager.atCurrentRow += determineFogginess;
        }
    }

    private void OnDisable()
    {
        fallAllTiles -= fall;
        GameManagerSc.levelWon -= startFlipTile;
        TimeManager.timerExpired -= fallIfInRow;
        TimeManager.timerExpired -= fadeWarning;
        WalkManager.atCurrentRow -= determineFogginess;
    }


    // When you click on a tile one of a number of things can happen.
    // The only scenario things DON'T actually happen is if the tile is outright banned from use -
    // Which can happen if it falls out of the game, for example.
    public void OnMouseDown()
    {
        if(acceptingClicks && !banned)
        {
            Debug.Log("Clicked on " + this.ToString());
            tileClicked.Invoke(this);
        }
    }

    // When a tile falls and hits the bottom of the floor we'll just remove it
    /*private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "FloorBottom")
        {
            Destroy(this.gameObject);
        }
    }*/

    public static void toggleCanClickTiles(bool canClick) { acceptingClicks = canClick; }
    public static void triggerFallAllTiles() { fallAllTiles.Invoke(false, true); }


    //Will either push down for a correct guess, or fall off for an incorrect one
    public void pressAnimation()
    {
        if (correct) { 
            correctPress();
            SfxManager.instance.playSFXbyName("correct-step", this.transform, 1f);
        }
        else { 
            incorrectPress();
            SfxManager.instance.playSFXbyName("incorrect-step", this.transform, 1f);
        }
    }

    // Will only reveal the tile as opposed to above
    public void revealAnimation()
    {
        // Maybe we do cam movement stuff here
        if (correct)
        {
            correctPress();
            // TODO reveal sound effect
        }
        else
        {
            textComponent.text = letter.ToString();
            fall(false, false);
        }
    }

    /// <summary>
    /// Run this when you've stepped on a correct tile
    /// </summary>
    private void correctPress()
    {
        StartCoroutine(pushDownTile());
        Debug.Log("Correct press");
        if(specType != SpecialTile.SPLIT && specType != SpecialTile.BLANK)
        {
            textComponent.text = letter.ToString();
        }
    }

    /// <summary>
    /// Run this when you step on an incorrect tile
    /// </summary>
    private void incorrectPress()
    {
        textComponent.text = letter.ToString();
        StartCoroutine(fallTile());
    }

    /// <summary>
    /// When you jump on the wrong tile you immediately crash through it
    /// </summary>
    public void incorrectAndLoseImmediately()
    {
        textComponent.text = letter.ToString();
        fallAllTiles.Invoke(false, true);
    }

    /// <summary>
    /// Tile falling animation
    /// We also do the "fall all" animation here, if you lose the game
    /// </summary>
    private IEnumerator fallTile()
    {
        fall(false, false);

        if (GameManagerSc.getNumTotems() <= 0 || GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.IRON_MAN))
        {
            // First wait one second, so you realize you done goofed
            yield return new WaitForSeconds(1.5f);

            fallAllTiles.Invoke(false, true);
        }
    }

    /// <summary>
    /// If you win a round, all incorrect tiles will flip over.
    /// </summary>
    /// <returns></returns>
    private void startFlipTile() { StartCoroutine(flipTile()); }

    private IEnumerator flipTile()
    {
        // This weird check basically checks for tiles that are: Incorrect, or blanks that were NOT part of the path.
        if(coords != null && (!correct || (specType == Tile.SpecialTile.BLANK && letter != '_')))
        {
            float steps = 50;
            float timeSec = 1.2f;

            Quaternion start = Quaternion.Euler(-90, 0, 0);
            Quaternion end = Quaternion.Euler(90, 0, 0);

            yield return new WaitForSeconds(coords.r * 0.2f);

            for (float i = 0; i <= steps; i++)
            {
                this.gameObject.transform.rotation = Quaternion.Lerp(start, end, i / steps);
                yield return new WaitForSeconds(1 / steps * timeSec);
            }
        }

        yield return null;
    }

    private void fadeWarning(int row)
    {
        StartCoroutine(fadeWarningWhenTimeRunningOut(row));
    }

    private IEnumerator fadeWarningWhenTimeRunningOut(int row)
    {
        // Only do this animation if in the next row
        if(coords != null && row + 1 == coords.r)
        {
            float steps = 50;
            float timeSec = TimeManager.timeInterval;
            Color col = this.timeFader.color;

            for (float i = 0; i <= steps; i++)
            {
                this.timeFader.color = new Color(col.r, col.g, col.b, i / steps);
                yield return new WaitForSeconds(1 / steps * timeSec);
            }
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
    private void fall(bool evenIfCorrect, bool violently)
    {
        if(this.physicalObject != null)
        {
            if (!this.correct || evenIfCorrect)
            {
                banned = true;

                Rigidbody rbody = this.physicalObject.GetComponent<Rigidbody>();
                rbody.constraints = RigidbodyConstraints.None;
                rbody.useGravity = true;
                if(violently)
                {
                    rbody.AddForce(new Vector3(UnityEngine.Random.Range(-400, 100), 0, UnityEngine.Random.Range(-400, 100)));
                    rbody.AddTorque(new Vector3(UnityEngine.Random.Range(-400, 400), 0, UnityEngine.Random.Range(-400, 400)));
                } else
                {
                    rbody.AddForce(new Vector3(UnityEngine.Random.Range(-50, 0), 0, UnityEngine.Random.Range(-50, 0)));
                    rbody.AddTorque(new Vector3(UnityEngine.Random.Range(-100, 100), 0, UnityEngine.Random.Range(-100, 100)));
                }
                
            } else
            {
                // Correct tiles, we don't want incorrect ones to "stay" on top of them - so activate the invis wall
                //transform.GetChild(1).gameObject.SetActive(true);

                // OR we just set them to be untouchable
                transform.GetComponent<MeshCollider>().isTrigger = true;
            }
        }
    }

    private void fallIfInRow(int row)
    {
        if(coords != null && coords.r == row)
        {
            fall(true, false);
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
        if(textComponent == null) textComponent = this.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

        // Sometimes blank tiles will interject the path of the word
        if(setTo != '_')
        {
            letter = setTo;
            display = setTo.ToString();
            textComponent.text = setTo.ToString();
        } else
        {
            // These aren't "normal" blank tiles - kind of part of the word, kind of not
            // Confusing i know - just don't change it please
            letter = '_';
            display = " ";
            textComponent.text = " ";
            specType = SpecialTile.BLANK;
            changeMaterial(WalkManager.tileMats.getCurrentBase(false, false, true, specType));
        }
        

        finalized = true;
        correct = isPartOfPath;
    }

    /// <summary>
    /// You've chosen to make this a special tile - adjust its appearance / behavior here
    /// </summary>
    public void setAsSpecialTile(SpecialTile specType)
    {
        if(textComponent == null) textComponent = this.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

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
                    display = LetterGen.getProportionallyRandomLetter() + "?";
                } else
                {
                    // The tile tells the truth
                    display = letter + "?";
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
        revealed = true;
        changeMaterial(changeTo);
    }

    public void revealMaterial(Material changeTo)
    {
        revealed = true;
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
