using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manages animations for your character.
/// Closely in coordination with WalkManager as to when animations are played
/// </summary>
public class AnimationManager : MonoBehaviour
{
    public WalkManager walkManager;
    public PlayerManager playerManager;

    public GameObject playerCharacter;
    private Animator playerAnimator;
    public Vector3 startingPlayerPos;
    public Vector3 ledgeStartingPlayerPos;
    public Vector3 ledgeEndingPlayerPos;
    public Vector3 endingPlayerPos;

    public static event Action<bool> setPreventPlayerMovement;
    public static event Action<bool> setActivelyMoving;
    public static event Action openedScroll;         // Only when the scroll is opened can we start moving (TODO: should it be when animation done instead?)
    public static event Action readyForNextLevelGen;  // Send when we are ready to start generating next level

    private Coroutine activeMovingCoroutine;  // Constantly updated - there should only be one going at a time.

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = playerCharacter.GetComponentInChildren<Animator>();
        openedScroll += () => { };
        readyForNextLevelGen += () => { };
        setPreventPlayerMovement += (_) => { };
        setActivelyMoving += (_) => { };
    }

    private void OnEnable()
    {
        GameManagerSc.levelReady += playStartingAnimation;
        Tile.fallAllTiles += playFallingAnimation;
    }

    private void OnDisable()
    {
        GameManagerSc.levelReady -= playStartingAnimation;
        Tile.fallAllTiles -= playFallingAnimation;
    }

    /// <summary>
    /// When first loading into a level, play this animation
    /// </summary>
    void playStartingAnimation()
    {
        setPreventPlayerMovement.Invoke(true);
        activeMovingCoroutine = StartCoroutine(startingAnimation());
    }

    /// <summary>
    /// When exiting a level, play this animation
    /// </summary>
    public void playEndingAnimation()
    {
        setPreventPlayerMovement.Invoke(true);
        activeMovingCoroutine = StartCoroutine(clearLevel(0)); //TODO direction
    }

    // TODO: When this compiles set "Next" in postgame to call this method
    public void startWalkingToNextLevel()
    {
        activeMovingCoroutine = StartCoroutine(walkIntoNextLevel(0));
    }

    /// <summary>
    /// At beginning of a level
    /// </summary>
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

        setPreventPlayerMovement.Invoke(false);
    }

    public void moveAnim(Tile toTile)
    {
        activeMovingCoroutine = StartCoroutine(moveCharacter(toTile));
    }

    /// <summary>
    /// Walk character to this tile
    /// </summary>
    private IEnumerator moveCharacter(Tile toTile)
    {
        float steps = 20;
        float timeSec = 0.4f;

        Vector3 start = playerCharacter.transform.position;
        Vector3 target = new Vector3(toTile.absolutePosition.Item1, 0.5f, toTile.absolutePosition.Item2);

        // Once we decide to move to a tile we IMMEDIATELY set highlights and lay groundwork for moving to others.
        yield return walkManager.prepareNextMovement(toTile);


        this.playerAnimator.SetInteger("Direction", 0); //TODO: other directions
        this.playerAnimator.SetBool("Moving", true);

        for (float i = 0; i <= steps; i++)
        {
            playerCharacter.transform.position = Vector3.Lerp(start, target, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return walkManager.manageStep(toTile);

        //If no moves coming up afterwards, stop walking
        if (walkManager.queuedMoves.Count == 0)
        {
            this.playerAnimator.SetBool("Moving", false);
            this.playerAnimator.SetTrigger("Idle");
        }

        setActivelyMoving.Invoke(false);
    }

    public void realization()
    {
        this.playerAnimator.SetTrigger("Realization");
    }

    public void instaFalling()
    {
        //this.playerCharacter.GetComponent<BoxCollider>().isTrigger = true;
        this.playerAnimator.SetTrigger("Realization");
        if(activeMovingCoroutine != null) StopCoroutine(activeMovingCoroutine);
        playFallingAnimation(false,false);
    }

    public void drawbackAnim(Tile backToTile)
    {
        activeMovingCoroutine = StartCoroutine(drawbackCharacter(backToTile));
    }

    /// <summary>
    /// Character returns to original tile when they step on a wrong one
    /// </summary>
    private IEnumerator drawbackCharacter(Tile backToTile)
    {
        float steps = 20;
        float timeSec = 0.4f;

        Vector3 start;
        Vector3 target;

        // If you get a tile in the first row wrong (but survive) you go back to the ledge.
        if(backToTile == null)
        {
            start = playerCharacter.transform.position;
            target = ledgeStartingPlayerPos;

            walkManager.returnToStart();
        } else
        {
            start = playerCharacter.transform.position;
            target = new Vector3(backToTile.absolutePosition.Item1, 0.5f, backToTile.absolutePosition.Item2);

            // It's like we are moving back to the tile we just came from.
            yield return walkManager.prepareNextMovement(backToTile);
        }

        this.playerAnimator.SetInteger("Direction", 0); //TODO: other directions
        this.playerAnimator.SetBool("Moving", true);

        for (float i = 0; i <= steps; i++)
        {
            playerCharacter.transform.position = Vector3.Lerp(start, target, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        // Will stop movement immediately
        this.playerAnimator.SetBool("Moving", false);
        this.playerAnimator.SetTrigger("Idle");

        setActivelyMoving.Invoke(false);
        setPreventPlayerMovement.Invoke(false);
    }

    public void prepareJump()
    {
        //TODO we have to ensure the player can only do this when theyre not moving
        this.playerAnimator.SetTrigger("JumpPrep");
    }

    public void launchJump(Tile toTile)
    {
        StartCoroutine(jumpingFlight(toTile));
    }

    IEnumerator jumpingFlight(Tile toTile)
    {
        this.playerAnimator.SetTrigger("JumpLaunch");

        float steps = 20;
        float timeSec = 0.4f;

        Vector3 start = playerCharacter.transform.position;
        Vector3 target = new Vector3(toTile.absolutePosition.Item1, 0.5f, toTile.absolutePosition.Item2);

        // Once we decide to move to a tile we IMMEDIATELY set highlights and lay groundwork for moving to others.
        yield return walkManager.prepareNextMovement(toTile);

        this.playerAnimator.SetInteger("Direction", 0); //TODO: other directions
        this.playerAnimator.SetBool("Moving", true);

        for (float i = 0; i <= steps; i++)
        {
            playerCharacter.transform.position = Vector3.Lerp(start, target, i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        yield return walkManager.manageStep(toTile);

        //If no moves coming up afterwards, stop walking
        if (walkManager.queuedMoves.Count == 0)
        {
            this.playerAnimator.SetBool("Moving", false);
            this.playerAnimator.SetTrigger("Idle");
        }

        setActivelyMoving.Invoke(false);

        yield return null;
    }

    //TODO direction needs to be accounted for
    /// <summary>
    /// Walk off the tileset and begin postgame animations
    /// </summary>
    IEnumerator clearLevel(int direction)
    {
        yield return new WaitUntil(() => !walkManager.isActivelyMoving);

        // TODO moving is often left at "true" in this moment, it has to be set to false somewhere else before then.
        playerManager.walterWhitePan();

        playerAnimator.ResetTrigger("Idle");
        playerAnimator.SetBool("Moving", true);
        playerAnimator.SetInteger("Direction", direction);

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

        playerAnimator.SetTrigger("WinRound");
        playerAnimator.SetBool("Moving", false);

        yield return null;
    }

    /// <summary>
    /// Leave this level entirely
    /// </summary>
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

    void playFallingAnimation(bool _, bool __)
    {
        playerCharacter.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        playerAnimator.SetTrigger("Falling");
        playerManager.walterWhitePan();
    }
}
