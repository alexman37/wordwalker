using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManagerSc : MonoBehaviour
{
    private static int numLevels = 5; //TODO: Increase default
    private static int currLevel = 0;
    private static int coins = 0;
    private static int totems = 0;

    private static WordGen.WordDB database;

    public static TilemapGen Tilemap;
    public static WordwalkerUIScript uiManager;

    private static bool numLevelsBool = true;
    private static bool checkingManagerGreenlights = true;

    private void Start()
    {
        //unfortunately the only way i can think of
        Tilemap = FindObjectOfType<TilemapGen>();
        uiManager = FindObjectOfType<WordwalkerUIScript>();
    }

    private void Update()
    {
        if (checkingManagerGreenlights) managerSetup();
    }

    public static void setParametersOnStart(int numLvl, WordGen.WordDB db)
    {
        Debug.Log("Setting parameters");
        numLevels = numLvl;
        database = db;

        //goToNextLevel();
    }

    //this is such a dumb way of doing it, but i simply don't care
    private void managerSetup()
    {

        // If all managers are ready, begin the game
        if(WalkManager.greenlight && WordwalkerUIScript.greenlight && TilemapGen.greenlight && PlayerManager.greenlight)
        {
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
            Tilemap.regenerateTileMap();
        }
    }

    public static WordGen.WordDB getWordDB()
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
        uiManager.ChangeCoins(coins, amount, add);
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
}
