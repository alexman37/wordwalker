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
    public DatabaseItem dbItem;

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
    }



    private void updateDatabase(DatabaseItem database)
    {
        this.dbItem = database;
    }

    // Play the daily word
    public void startDailyWordGame(string word, string defn)
    {
        Debug.Log("Starting Daily word game");
        MusicManager.inGameMusicFade(true);
        GameManagerSc.setDailyWordParams(word, defn, selectedChallenges);

        GameManagerSc.transitioning = true;
        transition.Invoke(true);
    }

    // Start new adventure / free play game
    public void startNewGame()
    {
        Debug.Log("Setting up new game with DB " + dbItem.databaseId);
        MusicManager.inGameMusicFade(true);
        GameManagerSc.setParametersOnStart(numLevels, dbItem, selectedChallenges);
        WidgetPopup.resetWidgets();

        GameManagerSc.transitioning = true;
        transition.Invoke(true);
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
