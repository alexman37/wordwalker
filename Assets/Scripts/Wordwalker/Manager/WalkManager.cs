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
    private int numMistakes = 0;
    private bool timerStarted = false;

    public Queue<Tile> queuedMoves;   // so that we can pre-click multiple tiles
    public bool isActivelyMoving = false;
    private bool preventMovement = false; //for instance, if you're about to die
    private bool jumping = false;       // used for blue item
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
    private List<Tile> whitelist; // correct tiles not yet stepped on- used by green item
    private List<Tile> allTiles; // every tile out there - used by red item

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

        ItemsScript.greenItemUsed += onUsedGreenItem;
        ItemsScript.redItemUsed += onUsedRedItem;
        ItemsScript.blueItemUsed += onUsedBlueItem;
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

        ItemsScript.greenItemUsed -= onUsedGreenItem;
        ItemsScript.redItemUsed -= onUsedRedItem;
        ItemsScript.blueItemUsed -= onUsedBlueItem;
    }

    private void Update()
    {
        if(queuedMoves.Count != 0)
        {
            if (!isActivelyMoving)
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


    /// USE OF ITEMS
    /// Green: Reveal a single correct tile not yet discovered
    /// Red: Reveal multiple incorrect tiles not yet discovered
    /// Blue: For your next move you may jump to any tile up to 3 spots away
    void onUsedGreenItem()
    {
        // Create whitelist if not yet done
        if (whitelist == null) whitelist = new List<Tile>(correctTiles);

        while(whitelist.Count > 0)
        {
            Tile revealMe = whitelist[UnityEngine.Random.Range(0, whitelist.Count)];

            // Not fallen off the map, not stepped on either
            if (!revealMe.banned && !revealMe.revealed)
            {
                // Shortened version of manage step
                revealTile(revealMe);
                return;
            }
            else
            {
                whitelist.Remove(revealMe);
            }
        }
        Debug.LogWarning("Could not find any tiles to reveal!");
    }

    void onUsedRedItem()
    {
        // Create all list if not yet done
        if(allTiles == null)
        {
            allTiles = new List<Tile>();
            foreach (Tile t in TilemapGen.tileMap.Values)
            {
                if(t != null) allTiles.Add(t);
            }
        }
        
        for(int i = 0; i < 5; i++) // TODO how many?
        {
            while (allTiles.Count > 0)
            {
                Tile revealMe = allTiles[UnityEngine.Random.Range(0, allTiles.Count)];

                // Not fallen off the map, not stepped on either
                Debug.Log(revealMe);
                if (!revealMe.banned && !revealMe.revealed && !revealMe.correct)
                {
                    // Shortened version of manage step
                    revealTile(revealMe);
                    break;
                }
                else
                {
                    allTiles.Remove(revealMe);
                }
            }
            if (allTiles.Count == 0) {
                Debug.LogWarning("Could not find any tiles to reveal!");
                return;
            }
        }
    }

    void onUsedBlueItem()
    {
        //TODO undo
        jumping = true;

        // First, get all tiles within a certain radius (of either start, or currTile)
        int radius = GameManagerSc.foggyVision;
        removeAllHighlightsInPossibleNext();
        possibleNext.Clear();

        // Case: anywhere in the middle
        if (currTile != null)
        {
            HashSet<Tile> locked = new HashSet<Tile>();
            Dictionary<Tile, int> discoveredDepths = new Dictionary<Tile, int>();

            searchAdjacenciesHelper(currTile, locked, discoveredDepths, 0, radius);
            discoveredDepths.Clear();
            possibleNext = new List<Tile>(locked);
        } 
        // Case: at the start
        else
        {
            HashSet<Tile> locked = new HashSet<Tile>();
            Dictionary<Tile, int> discoveredDepths = new Dictionary<Tile, int>();
            HashSet<Tile> superSet = new HashSet<Tile>();

            for(int i = 0; i < startingTiles.Count; i++)
            {
                searchAdjacenciesHelper(startingTiles[i], locked, discoveredDepths, 1, radius);
                superSet.UnionWith(locked);
                discoveredDepths.Clear();
                locked.Clear();
            }
            possibleNext = new List<Tile>(superSet);
        }
        highlightAllInPossibleNext();

        // And prepare the jumping animation
        animationManager.prepareJump();
    }

    void searchAdjacenciesHelper(Tile curr, HashSet<Tile> locked, Dictionary<Tile, int> discoveredDepths, int depth, int allowedRad)
    {
        if (depth <= allowedRad)
        {
            locked.Add(curr);
            foreach (Adjacency adj in curr.adjacencies)
            {
                if (!discoveredDepths.ContainsKey(curr)) discoveredDepths.Add(curr, depth);
                else if (discoveredDepths[curr] > depth) discoveredDepths[curr] = depth;

                searchAdjacenciesHelper(adj.tile, locked, discoveredDepths, depth + 1, allowedRad);
            }
        }
    }

    /// <summary>
    /// Reset the walk manager when going to a new level
    /// Establish the new word and definition as well
    /// </summary>
    void reset(string w, string d)
    {
        possibleNext.Clear();
        startingTiles.Clear();
        timerStarted = false;
        numMistakes = 0;
        currWord = w;
        currDef = d;
        topBar.ResetBar();
        setClue();
        maxReachedRow = -1;
        playerManager.setToStartingPosition();
        fogSheet.transform.position = new Vector3(fogSheet.transform.position.x, fogSheet.transform.position.y, GameManagerSc.foggyVision * GenMethod.ySpacing + 48);
        allTiles = null;
        whitelist = null;
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
    /// Show a tile as correct or not and "step" on it, but do not move to there.
    /// </summary>
    private void revealTile(Tile t)
    {
        t.revealAnimation();
        if (t.correct)
        {
            t.revealMaterial(tileMats.correctTile);
            

            // TODO Will add to topbar unless (A) you already stepped on it, or (B) it's an empty/blank tile
            /*if (!t.stepped && t.specType != Tile.SpecialTile.BLANK)
            {
                addLetterToTopWord(t);
            }*/
        }

        // When stepping on an incorrect tile, lose a totem if you have one, otherwise game over!
        else
        {
            t.revealMaterial(tileMats.incorrectTile);
        }
    }


    /// <summary>
    /// Several animations play when you move to a new tile. This triggers them
    /// </summary>
    public IEnumerator manageStep(Tile t, bool jumped)
    {
        // On first step
        if(!timerStarted)
        {
            timerStarted = true;
            TimeManager.startNamedTimer("walk_time");
        }

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
            // You lose immediately if you jump on the wrong tile no matter what
            if(jumped)
            {
                preventMovement = true;
                queuedMoves.Clear();
                t.stepMaterial(tileMats.incorrectTile);
                t.incorrectAndLoseImmediately();
                animationManager.playFallingAnimation(false, false);
                onLose(GameManagerSc.LossReason.JUMP);
            }
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
                if (!jumping)
                    queuedMoves.Enqueue(t);
                else
                {
                    isActivelyMoving = true;
                    animationManager.launchJump(t);

                    // Will have to update Y-coord for camera in PlayerManager (according to zoom)
                    playerManager.LerpCameraTo(new Vector3(t.absolutePosition.Item1, 0, t.absolutePosition.Item2), 0.5f);
                    jumping = false;
                }
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
            if (!t.revealed && !t.marked)
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
            if(!next.revealed && !next.marked)
            {
                next.changeMaterial(tileMats.getCurrentBase(next.marked, next.stepped, next.correct, next.specType));
            }
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
        if(scrollUI.gameObject.activeSelf)
        {
            scrollUI.setClue(currDef);
        } else
        {
            // TODO hopefully nothing bad here, bc the cluebook is disabled if this isnt an image clue list...
            clueBookUI.setPage(currDef); // TODO caption
        }
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
        float timeTotal = TimeManager.stopNamedTimer("walk_time");
        GameManagerSc.signifyLevelWon((int)timeTotal, numMistakes);
        topBar.SetAnswer(this.correctTiles, true);
        topBar.kickOffRotation();
        animationManager.playEndingAnimation();
    }

    /// <summary>
    /// Things to do the exact moment you done goof and step on an incorrect tile
    /// </summary>
    bool onIncorrectChoice()
    {
        numMistakes += 1;
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
