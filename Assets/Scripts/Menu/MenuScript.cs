using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class MenuScript : MonoBehaviour
{
    // Signifies start of a new game (should enter loading phase)
    public static event Action<int, string> newGame;

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
        SceneManager.LoadScene(1);
    }

    // Start new adventure / free play game
    public void startNewGame()
    {
        Debug.Log("Setting up new game with DB " + dbItem.databaseId);
        MusicManager.inGameMusicFade(true);
        GameManagerSc.setParametersOnStart(numLevels, dbItem, selectedChallenges);
        SceneManager.LoadScene(1);
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
