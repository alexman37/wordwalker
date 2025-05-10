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
        Debug.Log("Setting up new game");
        GameManagerSc.setParametersOnStart(numLevels, database, selectedChallenges);
        SceneManager.LoadScene(1);
    }

    private void OnEnable()
    {
        DBClick.dbSelected += updateDatabase;
        AdventureMenu.challengeEnabled += changedChallengeStatus;
    }

    private void OnDisable()
    {
        DBClick.dbSelected -= updateDatabase;
        AdventureMenu.challengeEnabled -= changedChallengeStatus;
    }

    private void changedChallengeStatus(string id, bool enabled)
    {
        if (enabled) selectedChallenges.Add(getChallengeFromId(id));
        else selectedChallenges.Remove(getChallengeFromId(id));
    }


    private Challenge getChallengeFromId(string id)
    {
        switch (id)
        {
            case "iron_man": return Challenge.IRON_MAN;
            case "timer": return Challenge.TIMER;
            case "fog": return Challenge.FOG;
            case "no_items": return Challenge.NO_ITEMS;
            case "special_tiles": return Challenge.SPECIAL_TILES;
            case "gen_plus": return Challenge.GEN_PLUS;
            default: return Challenge.IRON_MAN;
        }
    }

    public enum Challenge
    {
        IRON_MAN,
        TIMER,
        FOG,
        SPECIAL_TILES,
        GEN_PLUS,
        NO_ITEMS
    }
}
