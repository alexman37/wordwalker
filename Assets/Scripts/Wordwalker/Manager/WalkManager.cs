using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manager for tracking user's input for clicking on tiles.
/// </summary>
public class WalkManager : MonoBehaviour
{
    // BASIC
    public static bool greenlight = false;
    public GameManagerSc gameManager;
    public PlayerManager playerManager;
    public AnimationManager animationManager;
    public TimeManager timeManager;

    // MATERIALS - What materials a tile becomes in different situations
    // This can change depending on the setting
    public static TileMats tileMats;

    // Keep track of where we're going to move next (cannot run these coroutines simultaneously)
    private Tile currTile;
    private int maxReachedRow = -1;
    public static event Action<int> atCurrentRow; // The fog challenge need this.

    public Queue<Tile> queuedMoves;   // so that we can pre-click multiple tiles
    public bool isActivelyMoving = false;
    private bool preventMovement = false; //for instance, if you're about to die
    private bool hasWon = false;
    private List<Tile> possibleNext;  // anywhere we can potentially go next
    private List<Tile> startingTiles;

    // Walk manager needs to interact with these components
    public TopBarUI topBar;
    public ScrollUI scrollUI;
    public ClueBookUI clueBookUI;

    public GameObject fogSheet;  // A sheet of fog (if fog is turned on) which moves in accordance with the fog tiles
    bool fogIsMoving = false;

    private string currWord;
    private string currDef;
    private List<Tile> correctTiles;


    // Start is called before the first frame update
    void Start()
    {
        tileMats = FindObjectOfType<TileMats>();

        if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.FOG))
        {
            fogSheet.SetActive(true);
        }

        possibleNext = new List<Tile>();
        correctTiles = new List<Tile>();
        startingTiles = new List<Tile>();

        queuedMoves = new Queue<Tile>();

        Debug.Log("Walk Manager READY");
        greenlight = true;
    }

    private void OnEnable()
    {
        AnimationManager.setPreventPlayerMovement += preventPlayerMovement;
        AnimationManager.setActivelyMoving += changeActivelyMoving;

        GenMethod.finishedGeneration += setStartingTiles;
        GenMethod.regenerate += reset;
        GenMethod.setCorrects += setCorrect;

        ModeToolUI.inMarkerMode += markerEnabled;
        ModeToolUI.inStepperMode += stepperEnabled;
        ModeToolUI.inViewMode += viewEnabled;

        TimeManager.timerExpired += ifOnFallenTileLoseImmediately;
    }

    private void OnDisable()
    {
        AnimationManager.setPreventPlayerMovement -= preventPlayerMovement;

        Tile.tileClicked -= whenTileClickedMakeStep;
        Tile.tileClicked -= whenTileClickedMarkAsDangerous;
        GenMethod.finishedGeneration -= setStartingTiles;
        GenMethod.regenerate -= reset;
        GenMethod.setCorrects -= setCorrect;

        ModeToolUI.inMarkerMode -= markerEnabled;
        ModeToolUI.inStepperMode -= stepperEnabled;
        ModeToolUI.inViewMode -= viewEnabled;

        TimeManager.timerExpired -= ifOnFallenTileLoseImmediately;
    }

    private void Update()
    {
        if(queuedMoves.Count != 0)
        {
            if(!isActivelyMoving)
            {
                isActivelyMoving = true;
                Tile pos = queuedMoves.Dequeue();
                animationManager.moveAnim(pos);

                // Will have to update Y-coord for camera in PlayerManager (according to zoom)
                playerManager.LerpCameraTo(new Vector3(pos.absolutePosition.Item1, 0, pos.absolutePosition.Item2), 0.5f);
            }
        }
    }

    private void preventPlayerMovement(bool val) { preventMovement = val; }
    private void changeActivelyMoving(bool val) { isActivelyMoving = val; }

    // MODE CHANGES
    // Stepper / View: Clicks move as normal
    // Marker: Clicks make the tile red and "unsteppable"
    // TODO different callback functions for clicks.
    void markerEnabled()
    {
        Tile.tileClicked -= whenTileClickedMakeStep;
        Tile.tileClicked -= whenTileClickedMarkAsDangerous;
        Tile.tileClicked += whenTileClickedMarkAsDangerous;
        Debug.Log("marker on");
    }
    void stepperEnabled()
    {
        Tile.tileClicked -= whenTileClickedMakeStep;
        Tile.tileClicked -= whenTileClickedMarkAsDangerous;
        Tile.tileClicked += whenTileClickedMakeStep;
        Debug.Log("stepper on");
    }
    void viewEnabled()
    {
        Tile.tileClicked -= whenTileClickedMakeStep;
        Tile.tileClicked -= whenTileClickedMarkAsDangerous;
        Tile.tileClicked += whenTileClickedMakeStep;
        Debug.Log("view on");
    }

    /// <summary>
    /// Reset the walk manager when going to a new level
    /// Establish the new word and definition as well
    /// </summary>
    void reset(string w, string d)
    {
        hasWon = false;
        possibleNext.Clear();
        startingTiles.Clear();
        currWord = w;
        currDef = d;
        topBar.ResetBar();
        setClue();
        maxReachedRow = -1;
        playerManager.setToStartingPosition();
        fogSheet.transform.position = new Vector3(fogSheet.transform.position.x, fogSheet.transform.position.y, GameManagerSc.foggyVision * GenMethod.ySpacing + 48);
    }

    // (generation) set local copy of which tiles can be stepped on
    void setCorrect(List<Tile> corrects)
    {
        correctTiles.Clear();
        correctTiles = corrects;
    }

    // (generation) set local copy of which tiles you can start the level by stepping on
    void setStartingTiles(List<Tile> starters)
    {
        startingTiles = new List<Tile>(starters);
        possibleNext = starters;
        highlightAllInPossibleNext();

        if (GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.FOG))
        {
            atCurrentRow.Invoke(maxReachedRow);
        }
    }

    /// <summary>
    /// Remove old highlights - don't set new ones just yet (due to fog)
    /// </summary>
    public IEnumerator prepareNextMovement(Tile toTile)
    {
        //Remove all next highlights
        removeAllHighlightsInPossibleNext();

        possibleNext.Clear();

        if (!toTile.isBackRow)
        {
            foreach (Adjacency adj in toTile.adjacencies)
            {
                if(!adj.tile.stepped || adj.tile.correct) possibleNext.Add(adj.tile);
            }
        }

        yield return null;
    }


    /// <summary>
    /// Several animations play when you move to a new tile. This triggers them
    /// </summary>
    public IEnumerator manageStep(Tile t)
    {
        if (t.correct)
        {
            if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.TIMER) && t.coords.r == 0)
            {
                timeManager.startIntervalTimer();
            }

            currTile = t;
            t.pressAnimation();

            // Will add to topbar unless (A) you already stepped on it, or (B) it's an empty/blank tile
            if (!t.stepped && t.specType != Tile.SpecialTile.BLANK)
            {
                addLetterToTopWord(t);
            }

            //If it's in the back row, you win!
            t.stepMaterial(tileMats.correctTile);
            if (t.isBackRow)
            {
                onWin();
            }
            else
            {
                // If this is the "furthest" we've gone, remove fog, if applicable
                if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.FOG))
                {
                    if (t.coords.r > maxReachedRow)
                    {
                        maxReachedRow = t.coords.r;
                        atCurrentRow.Invoke(maxReachedRow);
                        StartCoroutine(moveFog(maxReachedRow));
                    }
                }
            }
            highlightAllInPossibleNext();
        }

        // When stepping on an incorrect tile, lose a totem if you have one, otherwise game over!
        else
        {
            preventMovement = true;
            queuedMoves.Clear();
            t.stepMaterial(tileMats.incorrectTile);
            //addLetterToTopWord(t); // TODO - eventually we might have a "not this one" typa animation.
            t.pressAnimation();

            if (onIncorrectChoice())
            {
                GameManagerSc.signifyWrongStep();
                animationManager.drawbackAnim(currTile);
                queuedMoves.Clear();
                highlightAllInPossibleNext();
            }
        }
        

        yield return null;
    }

    /// <summary>
    /// When clicked on a tile add it to the queue
    /// </summary>
    void whenTileClickedMakeStep(Tile t)
    {
        if(!t.marked)
        {
            if (possibleNext.Contains(t) && !preventMovement)
            {
                queuedMoves.Enqueue(t);
            }
            else
            {
                //TODO some sort of warning
            }
        }
        else
        {
            Debug.Log("Not doing anything, this tile is marked");
        }
    }

    /// <summary>
    /// (Marker mode) mark tile as dangerous (or unmark if already marked)
    /// </summary>
    void whenTileClickedMarkAsDangerous(Tile t)
    {
        if (!t.stepped)
        {
            if (!t.marked) t.markAsDangerous(tileMats.incorrectTile);
            else t.unmarkAsDangerous(tileMats.getCurrentBase(false, t.stepped, t.correct, t.specType));
        }
    }

    /// <summary>
    /// Spawn highlights (which can vary) for all tiles in possible next
    /// </summary>
    void highlightAllInPossibleNext()
    {
        foreach (Tile t in possibleNext)
        {
            if (!t.stepped)
            {
                t.highlightMaterial(tileMats.getCurrentHighlight(t.marked, t.stepped, t.correct, t.specType));
            }
        }
    }

    /// <summary>
    /// Remove highlights for whatever is in the active possibleNext
    /// </summary>
    void removeAllHighlightsInPossibleNext()
    {
        foreach (Tile next in possibleNext)
        {
            next.changeMaterial(tileMats.getCurrentBase(next.marked, next.stepped, next.correct, next.specType));
        }
    }

    /// <summary>
    /// Automatically set "possibleNext" to the startingTiles - used for the drawback animation if you get the first tile wrong.
    /// </summary>
    public void returnToStart()
    {
        removeAllHighlightsInPossibleNext();

        possibleNext.Clear();

        foreach(Tile t in startingTiles)
        {
            Debug.Log(startingTiles);
            if (t.correct || !t.stepped) possibleNext.Add(t);
        }

        highlightAllInPossibleNext();
    }

    /// <summary>
    /// Move the sheet of fog
    /// </summary>
    IEnumerator moveFog(int row)
    {
        yield return new WaitUntil(() => !fogIsMoving);
        fogIsMoving = true;
        float steps = 10;
        float takeTime = 0.5f;

        Vector3 start = fogSheet.transform.position;
        Vector3 dest = fogSheet.transform.position + new Vector3(0, 0, GenMethod.ySpacing);
        if(row + GameManagerSc.foggyVision == GenMethod.settledRows)
        {
            dest += new Vector3(0, 0, 25);
        }

        for (float i = 0; i <= steps; i++)
        {
            fogSheet.transform.position = Vector3.Lerp(start, dest, i / steps);

            yield return new WaitForSeconds(1 / steps * takeTime);
        }
        fogSheet.transform.position = dest;

        fogIsMoving = false;
        yield return null;
    }

    /// <summary>
    /// With the timing challenge, if you are currently standing on the collapsed row, you lose immediately!
    /// </summary>
    void ifOnFallenTileLoseImmediately(int row)
    {
        StartCoroutine(ifOnFallenWaiter(row));
    }

    IEnumerator ifOnFallenWaiter(int row)
    {
        yield return new WaitUntil(() => isActivelyMoving == false);
        if (currTile.coords.r <= row)
        {
            // There is one last chance at salvation...if you are currently moving away from said tile towards a correct one
            if (queuedMoves.Count == 0 || !queuedMoves.Peek().correct || queuedMoves.Peek().coords.r <= row)
            {
                animationManager.instaFalling();
                onLose(GameManagerSc.LossReason.TIME);
            }
        }
    }


    /// <summary>
    /// Add letter to progress bar of topBar
    /// </summary>
    void addLetterToTopWord(Tile t)
    {
        if(t.specType != Tile.SpecialTile.SPLIT)
            topBar.AddLetterToProgress(t.letter, ' ');
        else
        {
            string[] twoSides = t.display.Split('\n');
            topBar.AddLetterToProgress(twoSides[0][0], twoSides[1][0]);
        }
            
    }

    /// <summary>
    /// Set the clue for this new level
    /// </summary>
    void setClue()
    {
        scrollUI.setClue(currDef);
        //TODO clueBook?
    }

    /// <summary>
    /// Things to do the exact moment you win a level
    /// </summary>
    void onWin()
    {
        if (GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.TIMER))
        {
            timeManager.stopIntervalTimer();
        }

        Debug.Log("The exact win moment");
        hasWon = true;
        GameManagerSc.signifyLevelWon();
        topBar.SetAnswer(this.correctTiles, true);
        topBar.kickOffRotation();
        animationManager.playEndingAnimation();
    }

    /// <summary>
    /// Things to do the exact moment you done goof and step on an incorrect tile
    /// </summary>
    bool onIncorrectChoice()
    {
        GameManagerSc.changeTotems(1, false);
        if (GameManagerSc.getNumTotems() < 0 || GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.IRON_MAN))
        {
            animationManager.realization();
            onLose(GameManagerSc.LossReason.TOTEMS);
            return false;
        }
        else return true;
    }

    /// <summary>
    /// Things to do the exact moment you lose the game
    /// </summary>
    void onLose(GameManagerSc.LossReason lr)
    {
        preventMovement = true;
        playerManager.setFreeCamera(false);

        if (GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.TIMER))
        {
            timeManager.stopIntervalTimer();
        }

        removeAllHighlightsInPossibleNext();
        GameManagerSc.signifyGameOver(lr);
        topBar.SetAnswer(this.correctTiles, false);
        topBar.kickOffRotation();
    }


}
