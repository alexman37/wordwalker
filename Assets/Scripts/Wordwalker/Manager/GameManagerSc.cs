using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManagerSc : MonoBehaviour
{
    private static int numLevels = 5; //TODO: Increase default
    private static int currLevel = 0;
    private static int coins = 0;
    private static int totems = 0;

    private static string database = "crossword";
    private static WordGen.Word[] wordList = new WordGen.Word[numLevels];

    public static TilemapGen Tilemap;
    public static WordwalkerUIScript uiManager;

    private static bool numLevelsBool = true;
    private static bool checkingManagerGreenlights = true;

    public static event Action levelWon;
    public static event Action wrongStep;
    public static event Action gameOver;

    private void Start()
    {
        levelWon += () => { };
        wrongStep += () => { };
        gameOver += () => { };

        //unfortunately the only way i can think of
        Tilemap = FindObjectOfType<TilemapGen>();
        uiManager = FindObjectOfType<WordwalkerUIScript>();

        StartCoroutine(WordGen.LoadAsset("worddbs/" + database, database));

        SceneManager.sceneLoaded += onReentry;
    }

    private void Update()
    {
        if (checkingManagerGreenlights) managerSetup();
    }

    public static void setParametersOnStart(int numLvl, string db)
    {
        // Reset in-game variables to defaults
        coins = 0;
        totems = 0;
        currLevel = 0;

        Debug.Log("Setting parameters");
        numLevels = numLvl;
        database = db;
    }

    // Loading into the scene after the first time
    private void onReentry(Scene scene, LoadSceneMode mode)
    {
        if(!checkingManagerGreenlights && scene.buildIndex == 1)
        {
            Debug.Log("REENTRY POINT");
            goToNextLevel();
        }
    }

    //this is such a dumb way of doing it, but i simply don't care
    private void managerSetup()
    {
        // If all managers are ready, begin the game
        if (WalkManager.greenlight &&
            WordwalkerUIScript.greenlight &&
            TilemapGen.greenlight &&
            PlayerManager.greenlight &&
            WordGen.greenlight)
        {
            //TODO: eventually we want to "track" which words the player has already seen
            wordList = WordGen.getTailoredList(numLevels);

            Debug.Log("Starting the game");
            checkingManagerGreenlights = false;
            goToNextLevel();
        }
    }
    
    public static void goToNextLevel()
    {

        uiManager.ResetPostgamePosition();

        if (numLevelsBool) {
            numLevelsBool = false;
            uiManager.SetLevelAmount(numLevels);
        }

        Debug.Log("going to next level: level " + (currLevel + 1));
        
        if(currLevel == numLevels)
        {
            Debug.Log("youre a winner");

        } else
        {
            currLevel += 1;
            uiManager.SetNewRoom(currLevel);
            Tilemap.regenerateTileMap(wordList[currLevel - 1]);
        }
    }

    public static string getWordDB()
    {
        return database;
    }

    public static int getNumLevels()
    {
        return numLevels;
    }

    public static void changeCoins(int amount, bool add)
    {
        coins = coins + (add ? amount : -amount);
    }

    public static void changeTotems(int amount, bool add)
    {
        totems = totems + (add ? amount : -amount);
        uiManager.ChangeTotems(totems, amount, add);
    }

    public static int getNumCoins()
    {
        return coins;
    }

    public static int getNumTotems()
    {
        return totems;
    }

    public static void signifyLevelWon()
    {
        levelWon.Invoke();
    }

    public static void signifyWrongStep()
    {
        wrongStep.Invoke();
    }

    public static void signifyGameOver()
    {
        gameOver.Invoke();
    }

    public static void returnToMainMenu()
    {

        SceneManager.LoadScene(0);
    }
}
