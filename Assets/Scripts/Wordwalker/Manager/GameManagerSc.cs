using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManagerSc : MonoBehaviour
{
    private static int numLevels = 5; //TODO: Increase default
    private static int currLevel = 0;
    private static int totems = 0;
    private static int score = 0;

    private static string database = "crossword";
    private static WordGen.Word[] wordList = new WordGen.Word[numLevels];

    public static TilemapGen Tilemap;
    public static WordwalkerUIScript uiManager;

    private static bool numLevelsBool = true;
    private static bool checkingManagerGreenlights = true;

    public static event Action levelReady;
    public static event Action levelWon;
    public static event Action wrongStep;
    public static event Action gameOver;
    public static event Action levelReset;

    private void Start()
    {
        levelReady += () => { };
        levelWon += () => { };
        wrongStep += () => { };
        gameOver += () => { };
        levelReset += () => { };

        //unfortunately the only way i can think of
        Tilemap = FindObjectOfType<TilemapGen>();
        uiManager = FindObjectOfType<WordwalkerUIScript>();

        StartCoroutine(WordGen.LoadAsset("worddbs/" + database, database));
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += onReentry;
        WalkManager.readyForNextLevelGen += goToNextLevel;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= onReentry;
        WalkManager.readyForNextLevelGen -= goToNextLevel;
    }

    private void Update()
    {
        if (checkingManagerGreenlights) managerSetup();
    }

    public static void setParametersOnStart(int numLvl, string db)
    {
        // Reset in-game variables to defaults
        score = 0;
        totems = 0;
        currLevel = 0;

        Debug.Log("Setting parameters");
        numLevels = numLvl;
        database = db;

        // We'll also have to rebuild everything in the scene
        checkingManagerGreenlights = true;
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

            //Last thing to do, reset the greenlights to allow for "reloading" the scene in the same way upon next startup.
            WalkManager.greenlight = false;
            WordwalkerUIScript.greenlight = false;
            TilemapGen.greenlight = false;
            PlayerManager.greenlight = false;
            WordGen.greenlight = false;

            goToNextLevel();
        }
    }
    
    public static void goToNextLevel()
    {
        levelReset.Invoke();

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
            levelReady.Invoke();
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

    public static void changeScore(int amount, bool add)
    {
        int prior = score;
        score = score + (add ? amount : -amount);
        uiManager.ChangeScore(prior, amount, add);
    }

    public static void changeTotems(int amount, bool add)
    {
        totems = totems + (add ? amount : -amount);
        uiManager.ChangeTotems(totems, amount, add);
    }

    public static int getScore()
    {
        return score;
    }

    public static int getNumTotems()
    {
        return totems;
    }

    public static void signifyLevelWon()
    {
        changeScore(100, true);
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
