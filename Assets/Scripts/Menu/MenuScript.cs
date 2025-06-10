using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MenuScript : MonoBehaviour
{
    // Signifies start of a new game (should enter loading phase)
    public static event Action<int, string> newGame;

    // The challenges you currently have selected
    public HashSet<Challenge> selectedChallenges;

    public int numLevels = 5;
    public string database;

    public GameObject titleCard;
    public GameObject playButtons;
    public GameObject adventureMenu;

    private float mult = 1;

    // Start is called before the first frame update
    void Start()
    {
        newGame += (_,__) => { };
        selectedChallenges = new HashSet<Challenge>();
    }


    public void toggleAdventureMenu(bool onOrOff)
    {
        adventureMenu.gameObject.SetActive(onOrOff);
    }






    private void updateDatabase(string database)
    {
        this.database = database;
    }

    private void updateMult(float newMult)
    {
        mult = newMult;
    }

    public void startNewGame()
    {
        Debug.Log("Setting up new game with DB " + database);
        GameManagerSc.setParametersOnStart(numLevels, database, selectedChallenges);
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
