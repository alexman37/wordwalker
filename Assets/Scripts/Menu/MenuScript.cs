using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class MenuScript : MonoBehaviour
{
    public static bool transitioning; // if true, the transition splash will be on-screen at the start

    // Signifies start of a new game (should enter loading phase)
    public static event Action<int, string> newGame;

    public static event Action<bool> transition;

    // The challenges you currently have selected
    public HashSet<Challenge> selectedChallenges;

    public int numLevels = 5;
    public static DatabaseItem dbItem;

    public GameObject titleCard;
    public GameObject playButtons;

    // Start is called before the first frame update
    void Start()
    {
        newGame += (_,__) => { };
        selectedChallenges = new HashSet<Challenge>();
    }

    private void Awake()
    {
        if(transitioning)
        {
            StartCoroutine(AwaitTitleHexCreation());
        }
    }

    // Just need to wait for all transition tiles to be created before we can play it
    IEnumerator AwaitTitleHexCreation()
    {
        yield return new WaitUntil(() => TitleHex.tilesReady >= TitleHex.tilesRequired);
        transition.Invoke(false);
        //SfxManager.instance.playSFXbyName("slide-away", null, 0.5f);
    }



    private void updateDatabase(DatabaseItem database)
    {
        dbItem = database;
    }

    // Play the daily word.
    // The seed is just the date format of today with a 1 at the front- for example, 07/16/2025  -> 107162025
    // "Intensity" determines the difficulty. We also thought about using it to enable some challenges but...it's looking like we won't.
    public void startDailyWordGame(string word, string defn, int seed, int intensity)
    {
        SfxManager.instance.playSFXbyName("click-short", null, 1);
        Debug.Log("Starting Daily word game");
        MusicManager.inGameMusicFade(true);

        UnityEngine.Random.InitState(seed);

        GameManagerSc.setDailyWordParams(word, defn, selectedChallenges, intensity);

        GameManagerSc.transitioning = true;
        transition.Invoke(true);
        //SfxManager.instance.playSFXbyName("slide", null, 0.5f);
    }

    //TODO - possibly implement if we want challenges to be in the daily word.
    private HashSet<Challenge> getRandomChallengeList(int intensity)
    {
        HashSet<Challenge> toChooseFrom = new HashSet<Challenge>(new Challenge[] { 
            Challenge.FOG, Challenge.IRON_MAN, Challenge.SPECIAL_TILES, Challenge.TIMER, Challenge.GEN_PLUS });

        if (intensity > 5) return toChooseFrom;

        for(int i = 0; i < intensity; i++)
        {

        }

        return null;
    }

    // Start new adventure / free play game
    public void startNewGame()
    {
        SfxManager.instance.playSFXbyName("click-short", null, 1);
        Debug.Log("Setting up new game with DB " + dbItem.databaseId);
        MusicManager.inGameMusicFade(true);
        GameManagerSc.setParametersOnStart(numLevels, dbItem, selectedChallenges);
        WidgetPopup.resetWidgets();

        Debug.Log("Reset random seed to " + DateTime.Now.Millisecond);
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        GameManagerSc.transitioning = true;
        transition.Invoke(true);
        //SfxManager.instance.playSFXbyName("slide", null, 1);
    }

    private void OnEnable()
    {
        DBClick.dbSelected += updateDatabase;
        ChallengePopup.challengeEnabled += changedChallengeStatus;
        GameLengthSelect.lengthSelected += changeNumLevels;
    }

    private void OnDisable()
    {
        DBClick.dbSelected -= updateDatabase;
        ChallengePopup.challengeEnabled -= changedChallengeStatus;
        GameLengthSelect.lengthSelected -= changeNumLevels;
    }

    private void changeNumLevels(int numLs, string __)
    {
        Debug.Log(numLs);
        numLevels = numLs;
    }

    private void changedChallengeStatus(Challenge id, bool enabled)
    {
        lock (selectedChallenges)
        {
            if (enabled) selectedChallenges.Add(id);
            else selectedChallenges.Remove(id);
        }
    }

    public enum Challenge
    {
        IRON_MAN,
        TIMER,
        FOG,
        SPECIAL_TILES,
        GEN_PLUS
    }
}
