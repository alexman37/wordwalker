using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

/// <summary>
/// Manages overall gameplay state such as score, status and level count.
/// </summary>
public class GameManagerSc : MonoBehaviour
{
    public static bool transitioning; // if true, the transition splash will be on-screen at the start

    private static bool IN_TESTING = false;

    public static WWGameState state;

    private static string firstTimeWordsLoad = null;

    public static TilemapGen Tilemap;
    public static WordwalkerUIScript uiManager;

    private static ScrollUI scrollUI;
    private static ClueBookUI clueBookUI;

    private static bool numLevelsBool = true;
    private static DatabaseItem localDBcopy;
    private static bool checkingManagerGreenlights = true;

    public static event Action newGame;
    public static event Action<WordGen.Word> wordPrepared;
    public static event Action levelReady;
    public static event Action levelWon;
    public static event Action wrongStep;
    public static event Action<int> changeInTotems;
    public static event Action<LossReason> gameOver;
    public static event Action levelReset;
    public static event Action onLastLevel;
    public static event Action<int, int, int, int> updatePostgameScoreSheet;
    public static event Action<bool> transition;

    private void Start()
    {
        newGame += () => { };
        wordPrepared += (_) => { };
        levelReady += () => { };
        levelWon += () => { };
        wrongStep += () => { };
        changeInTotems += (_) => { };
        gameOver += (_) => { };
        levelReset += () => { };
        onLastLevel += () => { };
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
        // firstTimeWordsLoad = "trivia/flags";
        // localDBcopy = new DatabaseItem("trivia/flags", "Flags", null, null, 1, 100, "base/flags");
        // IN_TESTING = true;
    }

    private void OnEnable()
    {
        //SceneManager.sceneLoaded += onReentry; // TODO remove?
        AnimationManager.readyForNextLevelGen += goToNextLevel;
    }

    private void OnDisable()
    {
        //SceneManager.sceneLoaded -= onReentry;
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

    // TODO something with intensity once we improve tile gen a lil bit
    public static void setDailyWordParams(string word, string defn, HashSet<MenuScript.Challenge> challenges, int intensity)
    {
        // RESET STATE
        Debug.Log("Setting daily parameters");
        state = new WWGameState(1, 0, 3, true, challenges);

        WordGen.Skip();

        GlobalStatMap.AddFlag("dailyWordPlayedToday");

        // Word list for daily word is just the lone word we're playing
        state.setWordList(new WordGen.Word[1] { new WordGen.Word(word, defn) });
    }

    public static void setParametersOnStart(int numLvl, DatabaseItem dbItem, HashSet<MenuScript.Challenge> challenges)
    {
        // RESET STATE
        Debug.Log("Setting parameters");
        state = new WWGameState(numLvl, 3, 3, false, challenges);

        localDBcopy = dbItem;
        
        firstTimeWordsLoad = dbItem.databaseId;
    }

    // Loading into the scene after the first time TODO good to remove?
    /*private void onReentry(Scene scene, LoadSceneMode mode)
    {
        if(!checkingManagerGreenlights && scene.buildIndex == 1)
        {
            Debug.Log("REENTRY POINT");
            //goToNextLevel();
        }
    }*/

    //this is such a dumb way of doing it, but i simply don't care
    private void managerSetup()
    {
        // If all managers are ready, begin the game
        if (WalkManager.greenlight &&
            AnimationManager.greenlight &&
            WordwalkerUIScript.greenlight &&
            TilemapGen.greenlight &&
            PlayerManager.greenlight &&
            WordGen.greenlight)
        {
            /// DAILY WORD
            if(state.dailyWord)
            {
                clueBookUI.gameObject.SetActive(false);

                // TODO Keep track of streak? maybe?
                // DatabaseTracker.startNewGame(localDBcopy.databaseId);
            }

            /// FREE PLAY
            else
            {
                // Depending on if it's a text or image DB we will enable either the scroll or clueBook
                if (localDBcopy.imageDB != null)
                {
                    clueBookUI.setImageAssetBundlePath(localDBcopy.imageDB);
                    scrollUI.gameObject.SetActive(false);
                }
                else { clueBookUI.gameObject.SetActive(false); }

                // Oftentimes in debugging we like to start the game from the wordwalk scene, so this check is necessary for that
                if (IN_TESTING)
                {
                    state.setWordList(WordGen.getTailoredList(state.getNumLevels()));
                }
                else
                {
                    state.setWordList(WordGen.getTailoredList(state.getNumLevels(), DatabaseTracker.databaseTracker.databaseStorages[localDBcopy.databaseId].wordCycle.ToList()));
                }

                DatabaseTracker.startNewGame(localDBcopy.databaseId);
            }

            Debug.Log("Starting the game");
            checkingManagerGreenlights = false;

            //Last thing to do, reset the greenlights to allow for "reloading" the scene in the same way upon next startup.
            WalkManager.greenlight = false;
            WordwalkerUIScript.greenlight = false;
            TilemapGen.greenlight = false;
            PlayerManager.greenlight = false;
            WordGen.greenlight = false;
            AnimationManager.greenlight = false;

            transitioning = false;
            transition.Invoke(false);
            newGame.Invoke();
            goToNextLevel();
        }
    }

    // Losing or winning the game gives you the option to replay it with the exact same settings
    public static void retry()
    {
        // TODO Would be nice to have transition here, but not necessary.
        //transition.Invoke(true);

        WWGameState oldState = state;
        int newTotemsThisGame = 3; // TODO ???
        state = new WWGameState(oldState.getNumLevels(), newTotemsThisGame, oldState.foggyVision, false, oldState.selectedChallenges);
        state.setWordList(WordGen.getTailoredList(state.getNumLevels(), DatabaseTracker.databaseTracker.databaseStorages[localDBcopy.databaseId].wordCycle.ToList()));
        uiManager.withNewState(state);
        changeInTotems.Invoke(newTotemsThisGame);

        newGame.Invoke();
        goToNextLevel();
        //transition.Invoke(false);
    }
    
    public static void goToNextLevel()
    {
        levelReset.Invoke();

        if (numLevelsBool) {
            numLevelsBool = false;
            uiManager.SetLevelAmount(state.getNumLevels());
        }

        Debug.Log("going to next level: level " + (state.getCurrentLevel() + 1));
        
        if(state.getCurrentLevel() == state.getNumLevels())
        {
            // TODO WINNING STUFF
            Debug.LogError("You should never have been able to click this button...");

        } else
        {
            state.nextLevel();
            if (state.getCurrentLevel() == state.getNumLevels())
            {
                Debug.Log("LAST LEVEL");
                onLastLevel.Invoke();
                // TODO a little more with this...should be the "treasure room"
            }
            uiManager.SetNewRoom(state.getCurrentLevel());

            WordGen.Word nextWord = state.getWordAt(state.getCurrentLevel() - 1);
            if(nextWord.word.Length > state.funStuff.longestWord.Length)
            {
                state.funStuff.longestWord = nextWord.word;
            }

            /// DAILY WORD
            if (state.dailyWord)
            {
                string onlyWord = nextWord.word;
                Tilemap.regenerateTileMap(nextWord, Mathf.FloorToInt(onlyWord.Length / 7) + Mathf.FloorToInt(onlyWord.Length / 10));
            }

            /// FREE PLAY
            else
            {
                Tilemap.regenerateTileMap(nextWord, localDBcopy.maxBacktracks);
                DatabaseTracker.addToCycle(localDBcopy.databaseId, nextWord);
                // Checks exactly when to reset the word cycle
                if (nextWord.word == WordGen.resetCycleOnThisWord)
                {
                    DatabaseTracker.resetCycle(localDBcopy.databaseId);
                }
            }

            wordPrepared.Invoke(nextWord);
            
            levelReady.Invoke();
        }
    }

    public static int getCurrentLevel()
    {
        return state.getCurrentLevel();
    }

    public static int getNumLevels()
    {
        return state.getNumLevels();
    }

    public static void changeScore(int amount, bool add)
    {
        int prior = amount;
        state.changeScore(amount, add);
        uiManager.ChangeScore(prior, amount, add);
        state.changeRank(uiManager.GetNewRank(state.numMistakes, state.getNumLevels() - state.getCurrentLevel()));
    }

    public static void changeTotems(int amount, bool add)
    {
        state.changeTotems(amount, add);
        changeInTotems.Invoke(state.getNumTotems());
        uiManager.ChangeTotems(state.getNumTotems(), amount, add);
    }

    public static int getScore()
    {
        return state.getScore();
    }

    public static int getRank()
    {
        return state.getRank();
    }

    public static HighScore getOfficialScore()
    {
        string formattedDate = DateTime.Today.ToString("d");

        // Award "gold star" if you win with all 5 challenges enabled and make no mistakes during entire run.
        if(state.selectedChallenges.Count == 5 && state.numMistakes == 0)
        {
            uiManager.AwardGoldStar();
            DatabaseTracker.goldStar(localDBcopy.databaseId);

            return new HighScore(state.getScore(), 14, formattedDate, 5);
        } else
        {
            return new HighScore(state.getScore(), RankBox.getFinalRank(state.numMistakes), formattedDate, state.selectedChallenges.Count);
        }
    }

    public static int getNumTotems()
    {
        return state.getNumTotems();
    }

    public static void signifyLevelWon(int numTimeSeconds, int numMistakes)
    {
        state.totalTime += numTimeSeconds;
        state.numMistakes += numMistakes;

        if(state.getCurrentLevel() == state.getNumLevels()) {
            if(state.dailyWord)
            {
                changeScore(100 - (25 * numMistakes), true);
                GlobalStatMap.IncrementInt("dailyWordStreak", 1);
            } else
            {
                changeScore(100 - (25 * numMistakes), true);

                DatabaseTracker.winGame(localDBcopy.databaseId, getOfficialScore());
            }
        } else
        {
            changeScore(100 - (25 * numMistakes), true);
        }

        updatePostgameScoreSheet.Invoke(numTimeSeconds, numMistakes, 25 * numMistakes, state.getScore());
        levelWon.Invoke();
    }

    public static void signifyWrongStep()
    {
        wrongStep.Invoke();
    }

    public static void signifyGameOver(LossReason lr)
    {
        gameOver.Invoke(lr);
        if(state.dailyWord)
        {
            GlobalStatMap.AddOrModifyInt("dailyWordStreak", 0);
        }
    }

    public static void returnToMainMenu()
    {
        WidgetPopup.resetWidgets();
        MusicManager.inGameMusicFade(false);

        MenuScript.transitioning = true;
        transition.Invoke(true);
    }

    public enum LossReason
    {
        TOTEMS,
        TIME,
        JUMP
    }
}

/// <summary>
/// Everything in relation to playing a single round of Wordwalker
/// For instance - what level you're on, how many totems you have left...
/// Purpose: If you start a new game or restart/retry a game, you should reset this.
/// </summary>
public class WWGameState
{
    private int numLevels = 10;
    private int currLevel = 0;
    private int totems = 0;
    private int score = 0;
    private int rank = -1;

    // stat tracking
    public int totalTime = 0;
    public int numMistakes = 0;
    public FunStatState funStuff;

    public int foggyVision = 3;   // How far ahead can you see when fog is enabled?

    public bool dailyWord = false;  // Daily word mode has some key differences from adventure / free play
    public HashSet<MenuScript.Challenge> selectedChallenges = new HashSet<MenuScript.Challenge>(); // Mostly used by tile generation

    private WordGen.Word[] wordList;

    public WWGameState(int levels, int startingTotems, int fogVision, bool daily, HashSet<MenuScript.Challenge> challenges)
    {
        numLevels = levels;
        currLevel = 0;
        totems = startingTotems;
        score = 0;
        rank = -1;

        // stat tracking
        totalTime = 0;
        numMistakes = 0;
        funStuff = new FunStatState();

        foggyVision = fogVision;   // How far ahead can you see when fog is enabled?

        dailyWord = daily;  // Daily word mode has some key differences from adventure / free play
        selectedChallenges = challenges; // Mostly used by tile generation
    }

    /// SETTERS
    public void nextLevel()
    {
        currLevel += 1;
    }

    public void setWordList(WordGen.Word[] words)
    {
        wordList = words;
    }

    public void changeScore(int amount, bool add)
    {
        int prior = score;
        score = score + (add ? amount : -amount);
    }

    public void changeTotems(int amount, bool add)
    {
        totems = totems + (add ? amount : -amount);
    }

    public void changeRank(int nextRank)
    {
        rank = nextRank;
    }


    /// GETTERS
    public int getScore()
    {
        return score;
    }

    public int getRank()
    {
        return rank;
    }

    public int getCurrentLevel()
    {
        return currLevel;
    }

    public int getNumLevels()
    {
        return numLevels;
    }

    public int getNumTotems()
    {
        return totems;
    }

    public WordGen.Word getWordAt(int index)
    {
        return wordList[index];
    }
}

// The stats in state that would only possibly matter for the fun stat portion.
public class FunStatState
{
    public int tilesStepped = 0;
    public int totemsFound = 0;
    public int itemsUsed = 0;
    public string longestWord = "";
    public (string word, int timeTaken) toughestWord = ("", 0);

    public FunStatState()
    {

    }
}