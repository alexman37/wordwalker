using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class WalkManager : MonoBehaviour
{
    public static bool greenlight = false;

    public GameManagerSc gameManager;
    public PlayerManager playerManager;

    public Material correctTile;
    public Material incorrectTile;
    public Material highlightTile;
    public Material baseTile;

    // Keep track of where we're going to move next (cannot run these coroutines simultaneously)
    private Tile currTile;
    private Queue<Tile> queuedMoves;
    private bool isActivelyMoving = false;
    private bool preventMovement = false; //for instance, if you're about to die
    private bool hasWon = false;

    public TopBarUI topBar;
    public TextMeshProUGUI definition;

    public GameObject playerCharacter;
    private Animator playerAnimator;
    public Vector3 startingPlayerPos;
    public Vector3 ledgeStartingPlayerPos;
    public Vector3 ledgeEndingPlayerPos;
    public Vector3 endingPlayerPos;

    public static Action openedScroll;
    public static Action readyForNextLevelGen;

    private List<Tile> possibleNext;

    private string currWord;
    private string currDef;
    private List<Tile> correctTiles;


    // Start is called before the first frame update
    void Start()
    {
        readyForNextLevelGen += () => { };
        openedScroll += () => { };

        possibleNext = new List<Tile>();
        correctTiles = new List<Tile>();

        queuedMoves = new Queue<Tile>();
        playerAnimator = playerCharacter.GetComponentInChildren<Animator>();

        greenlight = true;
    }

    private void OnEnable()
    {
        GameManagerSc.levelReady += playStartingAnimation;
        Tile.fallAllTiles += playFallingAnimation;
        TilemapGen.finishedGeneration += setStartingTiles;
        TilemapGen.regenerate += reset;
        TilemapGen.setCorrects += setCorrect;

        ModeToolUI.inMarkerMode += markerEnabled;
        ModeToolUI.inStepperMode += stepperEnabled;
        ModeToolUI.inViewMode += viewEnabled;
    }

    private void OnDisable()
    {
        GameManagerSc.levelReady -= playStartingAnimation;
        Tile.tileClicked -= whenTileClickedMakeStep;
        Tile.tileClicked -= whenTileClickedMarkAsDangerous;
        Tile.fallAllTiles -= playFallingAnimation;
        TilemapGen.finishedGeneration -= setStartingTiles;
        TilemapGen.regenerate -= reset;
        TilemapGen.setCorrects -= setCorrect;

        ModeToolUI.inMarkerMode -= markerEnabled;
        ModeToolUI.inStepperMode -= stepperEnabled;
        ModeToolUI.inViewMode -= viewEnabled;
    }

    private void Update()
    {
        if(queuedMoves.Count != 0)
        {
            if(!isActivelyMoving)
            {
                isActivelyMoving = true;
                Tile pos = queuedMoves.Dequeue();
                StartCoroutine(moveCharacter(pos));

                // Will have to update Y-coord for camera in PlayerManager (according to zoom)
                playerManager.LerpCameraTo(new Vector3(pos.absolutePosition.Item1, 0, pos.absolutePosition.Item2), 0.5f);
            }
        }
    }

    // MODE CHANGES
    // Stepper / View: Clicks move as normal
    // Marker: Clicks make the tile red and "unsteppable"
    // TODO different callback functions for clicks.
    void markerEnabled()
    {
        Tile.tileClicked -= whenTileClickedMakeStep;
        Tile.tileClicked += whenTileClickedMarkAsDangerous;
        Debug.Log("marker on");
    }

    void stepperEnabled()
    {
        Tile.tileClicked -= whenTileClickedMarkAsDangerous;
        Tile.tileClicked += whenTileClickedMakeStep;
        Debug.Log("stepper on");
    }

    void viewEnabled()
    {
        Debug.Log("view on");
    }

    void reset(string w, string d)
    {
        hasWon = false;
        possibleNext.Clear();
        currWord = w;
        currDef = d;
        topBar.ResetBar();
        setClue();
        playerManager.setToStartingPosition();
    }

    void setCorrect(List<Tile> corrects)
    {
        correctTiles.Clear();
        correctTiles = corrects;
    }

    void setStartingTiles(List<Tile> starters)
    {
        //TODO: Not set on the first go
        //Debug.Log("START TILES SET!");
        possibleNext = starters;
    }

    void playStartingAnimation()
    {
        preventMovement = true;
        StartCoroutine(startingAnimation());
    }

    void playEndingAnimation()
    {
        preventMovement = true;
        StartCoroutine(clearLevel(0)); //TODO direction
    }

    public void startWalkingToNextLevel()
    {
        StartCoroutine(walkIntoNextLevel(0));
    }

    IEnumerator startingAnimation()
    {
        playerAnimator.SetBool("Moving", true);
        playerAnimator.SetInteger("Direction", 0); // TODO direction

        float steps = 50;
        float timeSec = 1.5f;

        for (float i = 0; i <= steps; i++)
        {
            playerCharacter.transform.position = Vector3.Lerp(startingPlayerPos, ledgeStartingPlayerPos, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        playerAnimator.SetBool("Moving", false);
        playerAnimator.SetTrigger("Idle");
        playerAnimator.SetTrigger("StartReading");
        openedScroll.Invoke();
        yield return new WaitForSeconds(3);
        playerAnimator.SetTrigger("StopReading");

        preventMovement = false;
    }

    IEnumerator moveCharacter(Tile toTile)
    {
        float steps = 20;
        float timeSec = 0.4f;

        Vector3 start = playerCharacter.transform.position;
        Vector3 target = new Vector3(toTile.absolutePosition.Item1, 0.5f, toTile.absolutePosition.Item2);

        // Once we decide to move to a tile we IMMEDIATELY set highlights and lay groundwork for moving to others.
        yield return prepareNextMovement(toTile);


        this.playerAnimator.SetInteger("Direction", 0); //TODO: other directions
        this.playerAnimator.SetBool("Moving", true);

        for (float i = 0; i <= steps; i++)
        {
            playerCharacter.transform.position = Vector3.Lerp(start, target, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return manageStep(toTile);

        //If no moves coming up afterwards, stop walking
        if (queuedMoves.Count == 0)
        {
            this.playerAnimator.SetBool("Moving", false);
            this.playerAnimator.SetTrigger("Idle");
        }

        isActivelyMoving = false;

        if (hasWon)
        {
            playEndingAnimation();
        }
    }

    IEnumerator prepareNextMovement(Tile toTile)
    {
        //Remove all next highlights
        foreach (Tile next in possibleNext)
        {
            next.highlightMaterial(baseTile);
        }

        //TODO: instead of this hacky workaround we should have a backtracking option.
        possibleNext.Clear();

        if (!toTile.isBackRow)
        {
            foreach (Adjacency adj in toTile.adjacencies)
            {
                if (!adj.tile.stepped)
                {
                    possibleNext.Add(adj.tile);
                    adj.tile.highlightMaterial(highlightTile);
                }
            }
        }

        yield return null;
    }

    // Once you've moved to a tile you will play the stepping animation
    IEnumerator manageStep(Tile t)
    {
        if (t.correct)
        {
            currTile = t;
            t.pressAnimation();

            addLetterToTopWord(t);

            //If it's in the back row, you win!
            t.stepMaterial(correctTile);
            if (t.isBackRow)
            {
                onWin();
            }
            else
            {
                
            }
        }

        // When stepping on an incorrect tile, lose a totem if you have one, otherwise game over!
        else
        {
            preventMovement = true;
            queuedMoves.Clear();
            addLetterToTopWord(t);
            t.stepMaterial(incorrectTile);
            t.pressAnimation();

            if (onIncorrectChoice())
            {
                GameManagerSc.signifyWrongStep();
                yield return drawbackCharacter(currTile);
            }
        }

        yield return null;
    }

    IEnumerator drawbackCharacter(Tile backToTile)
    {
        float steps = 20;
        float timeSec = 0.4f;

        Vector3 start = playerCharacter.transform.position;
        Vector3 target = new Vector3(backToTile.absolutePosition.Item1, 0.5f, backToTile.absolutePosition.Item2);

        // Once we decide to move to a tile we IMMEDIATELY set highlights and lay groundwork for moving to others.
        yield return prepareNextMovement(backToTile);

        this.playerAnimator.SetInteger("Direction", 0); //TODO: other directions
        this.playerAnimator.SetBool("Moving", true);

        for (float i = 0; i <= steps; i++)
        {
            playerCharacter.transform.position = Vector3.Lerp(start, target, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        // Will stop movement immediately
        queuedMoves.Clear();
        this.playerAnimator.SetBool("Moving", false);
        this.playerAnimator.SetTrigger("Idle");

        isActivelyMoving = false;
        preventMovement = false;
    }

    //TODO direction needs to be accounted for
    IEnumerator clearLevel(int direction)
    {
        playerAnimator.SetBool("Moving", true);
        playerAnimator.SetInteger("Direction", direction);
        playerAnimator.ResetTrigger("Idle");

        float steps = 50;
        float timeSec = 1f;

        Vector3 lastKnownPlayerPos = playerCharacter.transform.position;
        this.ledgeEndingPlayerPos = new Vector3(lastKnownPlayerPos.x, lastKnownPlayerPos.y, lastKnownPlayerPos.z + 7f);
        this.endingPlayerPos = new Vector3(0, lastKnownPlayerPos.y, lastKnownPlayerPos.z + 17f);

        for (float i = 0; i <= steps; i++)
        {
            playerCharacter.transform.position = Vector3.Lerp(lastKnownPlayerPos, ledgeEndingPlayerPos, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        playerAnimator.SetBool("Moving", false);
        playerAnimator.SetTrigger("WinRound");
        yield return null;
    }

    IEnumerator walkIntoNextLevel(int direction)
    {
        playerAnimator.SetBool("Moving", true);
        playerAnimator.SetInteger("Direction", direction);

        float steps = 50;
        float timeSec = 1f;

        for (float i = 0; i <= steps; i++)
        {
            playerCharacter.transform.position = Vector3.Lerp(ledgeEndingPlayerPos, endingPlayerPos, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        playerAnimator.SetInteger("Direction", 1);
        playerAnimator.SetTrigger("Idle");
        playerAnimator.ResetTrigger("WinRound");
        readyForNextLevelGen.Invoke();
        yield return null;
    }

    void playFallingAnimation()
    {
        playerCharacter.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        playerAnimator.SetTrigger("Falling");
        playerManager.walterWhitePan();
    }

    void whenTileClickedMakeStep(Tile t)
    {
        //Debug.Log("Clicked");
        if(possibleNext.Contains(t) && !preventMovement)
        {
            queuedMoves.Enqueue(t);
        }
        else
        {
            //TODO some sort of warning
        }
    }

    void whenTileClickedMarkAsDangerous(Tile t)
    {
        if (!t.stepped)
        {
            if (t.marked) t.markAsDangerous(incorrectTile);
            else t.unmarkAsDangerous(incorrectTile);
        }
    }

    //TODO do more with color, etc.
    void addLetterToTopWord(Tile t)
    {
        topBar.AddLetterToProgress(t.letter);
    }

    void setClue()
    {
        definition.text = currDef;
    }

    void onWin()
    {
        hasWon = true;
        GameManagerSc.signifyLevelWon();
        topBar.SetAnswer(this.correctTiles, true);
        topBar.kickOffRotation();
    }

    bool onIncorrectChoice()
    {
        GameManagerSc.changeTotems(1, false);
        if (GameManagerSc.getNumTotems() < 0)
        {
            playerAnimator.SetTrigger("Realization");
            playerManager.setFreeCamera(false);
            onLose();
            return false;
        }
        else return true;
    }

    void onLose()
    {
        GameManagerSc.signifyGameOver();
        topBar.SetAnswer(this.correctTiles, false);
        topBar.kickOffRotation();
    }


}
