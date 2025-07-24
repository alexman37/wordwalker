using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Experimental.U2D.Animation;

/// <summary>
/// Manages animations for your character.
/// Closely in coordination with WalkManager as to when animations are played
/// </summary>
public class AnimationManager : MonoBehaviour
{
    public static bool greenlight = false;

    public WalkManager walkManager;
    public PlayerManager playerManager;

    public GameObject playerCharacter;
    private Animator playerAnimator;
    public Vector3 startingPlayerPos;
    public Vector3 ledgeStartingPlayerPos;
    public Vector3 ledgeEndingPlayerPos;
    public Vector3 endingPlayerPos;

    // depending on screen orientation this tells your character what "direction" to face
    private DirectionSuite LeftSuite, TopSuite, BottomSuite;
    private DirectionSuite activeSuite;

    public static event Action<bool> setPreventPlayerMovement;
    public static event Action<bool> setActivelyMoving;
    public static event Action openedScroll;         // Only when the scroll is opened can we start moving (TODO: should it be when animation done instead?)
    public static event Action readyForNextLevelGen;  // Send when we are ready to start generating next level

    private Coroutine activeMovingCoroutine;  // Constantly updated - there should only be one going at a time.

    [SerializeField] private SpriteLibraryAsset[] spriteLibAssets;

    [SerializeField] private AudioClip realizationClip;
    [SerializeField] private AudioClip collapseClip;
    [SerializeField] private AudioClip footstepsClip;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = playerCharacter.GetComponentInChildren<Animator>();
        openedScroll += () => { };
        readyForNextLevelGen += () => { };
        setPreventPlayerMovement += (_) => { };
        setActivelyMoving += (_) => { };

        // Direction suites.
        LeftSuite = new DirectionSuite(0, 2, 3, 1);
        TopSuite = new DirectionSuite(3, 1, 2, 0);
        BottomSuite = new DirectionSuite(1, 3, 0, 2);
        changeDirectionSuite(GlobalStatMap.loadGlobalStatMap().settingsValues.screenOrientationSetting);

        // Set the sprite to whichever character you are playing as.
        playerAnimator.GetComponent<SpriteLibrary>().spriteLibraryAsset = spriteLibAssets[(int) CharSelectPopup.activeCharSprite];

        greenlight = true;
    }

    private void OnEnable()
    {
        GameManagerSc.newGame += resetGame;
        GameManagerSc.levelReady += playStartingAnimation;
        Tile.fallAllTiles += playFallingAnimation;

        SettingsMenu.toggledScreenOr += changeDirectionSuite;
        PauseMenu.toggledScreenOr += changeDirectionSuite;
    }

    private void OnDisable()
    {
        GameManagerSc.newGame -= resetGame;
        GameManagerSc.levelReady -= playStartingAnimation;
        Tile.fallAllTiles -= playFallingAnimation;

        SettingsMenu.toggledScreenOr -= changeDirectionSuite;
        PauseMenu.toggledScreenOr -= changeDirectionSuite;
    }

    void changeDirectionSuite(ScreenOrientationSetting sor)
    {
        switch (sor)
        {
            case ScreenOrientationSetting.LEFT:
                activeSuite = LeftSuite;
                playerCharacter.transform.GetChild(0).transform.localRotation = Quaternion.Euler(180, 0, 180);
                break;
            case ScreenOrientationSetting.TOP:
                activeSuite = TopSuite;
                playerCharacter.transform.GetChild(0).transform.localRotation = Quaternion.Euler(180, 0, 90);
                break;
            case ScreenOrientationSetting.BOTTOM:
                activeSuite = BottomSuite;
                playerCharacter.transform.GetChild(0).transform.localRotation = Quaternion.Euler(180, 0, -90);
                break;
        }
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
        activeMovingCoroutine = StartCoroutine(clearLevel(activeSuite.forward));
    }

    // TODO: When this compiles set "Next" in postgame to call this method
    public void startWalkingToNextLevel()
    {
        activeMovingCoroutine = StartCoroutine(walkIntoNextLevel(activeSuite.forward));
    }

    /// <summary>
    /// At beginning of a level
    /// </summary>
    IEnumerator startingAnimation()
    {
        playerAnimator.SetBool("Moving", true);
        playerAnimator.SetInteger("Direction", activeSuite.forward);
        SfxManager.instance.beginSFXLoop("footsteps", footstepsClip, null, 1f);

        float timeSec = 1.5f;

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            playerCharacter.transform.position = Vector3.Lerp(startingPlayerPos, ledgeStartingPlayerPos, Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }

        SfxManager.instance.endSFXLoop("footsteps");

        playerAnimator.SetBool("Moving", false);
        playerAnimator.SetTrigger("Idle");
        playerAnimator.SetTrigger("StartReading");
        openedScroll.Invoke();
        yield return new WaitForSeconds(3);
        playerAnimator.SetTrigger("StopReading");

        yield return new WaitForSeconds(1.5f);
        setPreventPlayerMovement.Invoke(false);
    }

    public void moveAnim(Tile toTile, Adjacency.Direction dir)
    {
        activeMovingCoroutine = StartCoroutine(moveCharacter(toTile, dir));
    }

    /// <summary>
    /// Walk character to this tile
    /// </summary>
    private IEnumerator moveCharacter(Tile toTile, Adjacency.Direction dir)
    {
        float timeSec = 0.5f;

        Vector3 start = playerCharacter.transform.position;
        Vector3 target = new Vector3(toTile.absolutePosition.Item1, 0.5f, toTile.absolutePosition.Item2);

        // Once we decide to move to a tile we IMMEDIATELY set highlights and lay groundwork for moving to others.
        yield return walkManager.prepareNextMovement(toTile);

        Debug.Log("MOVING with direction " + activeSuite.dirToNumber(dir));
        this.playerAnimator.SetInteger("Direction", activeSuite.dirToNumber(dir));
        this.playerAnimator.SetBool("Moving", true);
        SfxManager.instance.beginSFXLoop("footsteps", footstepsClip, null, 1f);

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            playerCharacter.transform.position = Vector3.Lerp(start, target, Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }

        yield return walkManager.manageStep(toTile, false);

        //If no moves coming up afterwards, stop walking
        if (walkManager.queuedMoves.Count == 0)
        {
            SfxManager.instance.endSFXLoop("footsteps");
            this.playerAnimator.SetBool("Moving", false);
            this.playerAnimator.SetTrigger("Idle");
        }

        setActivelyMoving.Invoke(false);
    }

    public void realization()
    {
        SfxManager.instance.playSFX(realizationClip, this.playerCharacter.transform, 1f);
        this.playerAnimator.SetTrigger("Realization");
    }

    public void instaFalling()
    {
        //this.playerCharacter.GetComponent<BoxCollider>().isTrigger = true;
        this.playerAnimator.SetTrigger("Realization");
        if(activeMovingCoroutine != null) StopCoroutine(activeMovingCoroutine);
        playFallingAnimation(false,false);
    }

    public void drawbackAnim(Tile backToTile, Adjacency.Direction dir)
    {
        activeMovingCoroutine = StartCoroutine(drawbackCharacter(backToTile, dir));
    }

    /// <summary>
    /// Character returns to original tile when they step on a wrong one
    /// </summary>
    private IEnumerator drawbackCharacter(Tile backToTile, Adjacency.Direction dir)
    {
        float timeSec = 0.4f;

        Vector3 start;
        Vector3 target;

        this.playerAnimator.SetInteger("Direction", activeSuite.dirToNumber(dir));
        this.playerAnimator.SetBool("Moving", true);

        // If you get a tile in the first row wrong (but survive) you go back to the ledge.
        if (backToTile == null)
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

        SfxManager.instance.beginSFXLoop("footsteps", footstepsClip, null, 1f);

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            playerCharacter.transform.position = Vector3.Lerp(start, target, Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }

        SfxManager.instance.endSFXLoop("footsteps");

        // Will stop movement immediately
        this.playerAnimator.SetBool("Moving", false);
        //this.playerAnimator.SetTrigger("Idle");

        setActivelyMoving.Invoke(false);
        setPreventPlayerMovement.Invoke(false);
    }

    public void prepareJump()
    {
        //TODO we have to ensure the player can only do this when theyre not moving
        this.playerAnimator.SetTrigger("JumpPrep");
        this.playerAnimator.SetBool("Moving", false);
    }

    public void cancelJump()
    {
        //TODO we have to ensure the player can only do this when theyre not moving
        this.playerAnimator.SetTrigger("JumpCancel");
        this.playerAnimator.SetBool("Moving", false);
    }

    public void launchJump(Tile toTile)
    {
        StartCoroutine(jumpingFlight(toTile));
    }

    IEnumerator jumpingFlight(Tile toTile)
    {
        this.playerAnimator.SetTrigger("JumpLaunch");

        float timeSec = 1f;
        float maxHeightOfJump = 5;

        Vector3 start = playerCharacter.transform.position;
        Vector3 target = new Vector3(toTile.absolutePosition.Item1, 0.5f, toTile.absolutePosition.Item2);

        // Once we decide to move to a tile we IMMEDIATELY set highlights and lay groundwork for moving to others.
        yield return walkManager.prepareNextMovement(toTile);

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            Vector3 xAndz = Vector3.Lerp(start, target, Mathf.Clamp(i / timeSec, 0, 1));
            // Y should follow an exponential path, peaking at the midway point
            xAndz.y = -(maxHeightOfJump * 4) * Mathf.Pow((Mathf.Clamp(i / timeSec, 0, 1)) - 0.5f, 2) + maxHeightOfJump + 0.5f;
            Debug.Log(xAndz.y);
            playerCharacter.transform.position = xAndz;
            yield return null;
        }

        yield return walkManager.manageStep(toTile, true);

        setActivelyMoving.Invoke(false);

        this.playerAnimator.ResetTrigger("JumpPrep");
        this.playerAnimator.ResetTrigger("JumpLaunch");
        this.playerAnimator.SetTrigger("Idle");

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
        playerManager.walterWhitePan(1);

        playerAnimator.ResetTrigger("Idle");
        playerAnimator.SetBool("Moving", true);
        playerAnimator.SetInteger("Direction", direction);

        SfxManager.instance.beginSFXLoop("footsteps", footstepsClip, null, 1f);

        float timeSec = 1f;

        Vector3 lastKnownPlayerPos = playerCharacter.transform.position;
        this.ledgeEndingPlayerPos = new Vector3(lastKnownPlayerPos.x, lastKnownPlayerPos.y, lastKnownPlayerPos.z + 7f);
        this.endingPlayerPos = new Vector3(0, lastKnownPlayerPos.y, lastKnownPlayerPos.z + 17f);

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            playerCharacter.transform.position = Vector3.Lerp(lastKnownPlayerPos, ledgeEndingPlayerPos, Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }

        SfxManager.instance.endSFXLoop("footsteps");

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

        SfxManager.instance.beginSFXLoop("footsteps", footstepsClip, null, 1f);

        float timeSec = 1f;

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            playerCharacter.transform.position = Vector3.Lerp(ledgeEndingPlayerPos, endingPlayerPos, Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }

        SfxManager.instance.endSFXLoop("footsteps");

        playerAnimator.SetInteger("Direction", 1);
        playerAnimator.SetTrigger("Idle");
        playerAnimator.ResetTrigger("WinRound");
        readyForNextLevelGen.Invoke();
        yield return null;
    }

    public void playFallingAnimation(bool _, bool __)
    {
        playerCharacter.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        playerAnimator.SetTrigger("Falling");
        playerManager.walterWhitePan(1);
        SfxManager.instance.playSFX(collapseClip, this.playerCharacter.transform, 1f);
    }

    // Retry from the game over screen
    public void resetGame()
    {
        playerAnimator.Play("idle_down");
        playerCharacter.transform.position = startingPlayerPos;
        playerCharacter.transform.rotation = Quaternion.Euler(-90, 0, 90);
        playerCharacter.transform.GetChild(0).localRotation = Quaternion.Euler(180, 0, 180);
        playerCharacter.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        playerAnimator.ResetTrigger("Idle");
        //playerAnimator.SetInteger("Direction", 1);
        //playerAnimator.SetTrigger("Idle");
    }



    protected class DirectionSuite
    {
        public int forward;
        public int backward;
        public int left;
        public int right;

        public DirectionSuite(int f, int b, int l, int r)
        {
            forward = f; // forward as in "closer to the goal" (NE or SE in adj. list)
            backward = b; // backward as in "further from the goal" (NW or SW in adj. list)
            left = l;
            right = r;
        }

        public int dirToNumber(Adjacency.Direction dir)
        {
            switch(dir)
            {
                case Adjacency.Direction.NE: case Adjacency.Direction.NW:
                    return forward;
                case Adjacency.Direction.E: return left;
                case Adjacency.Direction.W: return right;
                case Adjacency.Direction.SE: case Adjacency.Direction.SW:
                    return backward;
                default: return 0;
            }
        }
    }
}