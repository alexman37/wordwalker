using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Manages overall gameplay state such as score, status and level count.
/// </summary>
public class GameManagerSc : MonoBehaviour
{
    private static int numLevels = 5; //TODO: Increase default
    private static int currLevel = 0;
    private static int totems = 0;
    private static int score = 0;

    public static int foggyVision = 3;   // How far ahead can you see when fog is enabled?

    public static HashSet<MenuScript.Challenge> selectedChallenges = new HashSet<MenuScript.Challenge>(); // Mostly used by tile generation

    private static string firstTimeWordsLoad = null;
    private static WordGen.Word[] wordList = new WordGen.Word[numLevels];

    public static TilemapGen Tilemap;
    public static WordwalkerUIScript uiManager;

    private static ScrollUI scrollUI;
    private static ClueBookUI clueBookUI;

    private static bool numLevelsBool = true;
    private static DatabaseItem localDBcopy;
    private static bool checkingManagerGreenlights = true;

    public static event Action levelReady;
    public static event Action levelWon;
    public static event Action wrongStep;
    public static event Action<LossReason> gameOver;
    public static event Action levelReset;
    public static event Action<int, int, int, int> updatePostgameScoreSheet;

    private void Start()
    {
        levelReady += () => { };
        levelWon += () => { };
        wrongStep += () => { };
        gameOver += (_) => { };
        levelReset += () => { };
        updatePostgameScoreSheet += (_,__,___,____) => { };

        //unfortunately the only way i can think of
        Debug.Log("Find at start");
        Tilemap = FindObjectOfType<TilemapGen>();
        uiManager = FindObjectOfType<WordwalkerUIScript>();
        scrollUI = FindObjectOfType<ScrollUI>();
        clueBookUI = FindObjectOfType<ClueBookUI>();
        Debug.Log(scrollUI);

        // We'll also have to rebuild everything in the scene
        checkingManagerGreenlights = true;

        // INFLECTION POINT!
        // UNCOMMENT THIS IF YOU WANT TO BE ABLE TO START FROM THE WORDWALK SCENE.
        // COMMENT OUT IF YOU WANT TO BE ABLE TO SELECT A DATABASE OF YOUR LIKING FROM THE MENU
        // firstTimeWordsLoad = "letters/c";
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += onReentry;
        AnimationManager.readyForNextLevelGen += goToNextLevel;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= onReentry;
        AnimationManager.readyForNextLevelGen -= goToNextLevel;
    }

    private void Update()
    {
        if (firstTimeWordsLoad != null)
        {
            string[] nameOfFile = firstTimeWordsLoad.Split('/');
            StartCoroutine(WordGen.LoadAsset("worddbs/" + firstTimeWordsLoad, nameOfFile[nameOfFile.Length - 1]));
            firstTimeWordsLoad = null;
        }
        if (checkingManagerGreenlights)
        {
            Debug.Log("Man setup");
            managerSetup();
        }
    }

    public static void setParametersOnStart(int numLvl, DatabaseItem dbItem, HashSet<MenuScript.Challenge> challenges)
    {
        // Reset in-game variables to defaults
        score = 0;
        totems = 3;
        currLevel = 0;

        localDBcopy = dbItem;

        Debug.Log("Setting parameters");
        numLevels = numLvl;
        firstTimeWordsLoad = dbItem.databaseId;
        selectedChallenges = challenges;
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
            // Depending on if it's a text or image DB we will enable either the scroll or clueBook
            if (localDBcopy.imageDB != null) {
                clueBookUI.setImageAssetBundlePath(localDBcopy.imageDB);
                scrollUI.gameObject.SetActive(false);
            }
            else { clueBookUI.gameObject.SetActive(false);}

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

    public static int getCurrentLevel()
    {
        return currLevel;
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

    public static void signifyLevelWon(int numTimeSeconds, int numMistakes)
    {
        changeScore(100 - (25 * numMistakes), true);
        updatePostgameScoreSheet.Invoke(numTimeSeconds, numMistakes, 25 * numMistakes, score);

        levelWon.Invoke();
    }

    public static void signifyWrongStep()
    {
        wrongStep.Invoke();
    }

    public static void signifyGameOver(LossReason lr)
    {
        gameOver.Invoke(lr);
    }

    public static void returnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public enum LossReason
    {
        TOTEMS,
        TIME,
        JUMP
    }
}
