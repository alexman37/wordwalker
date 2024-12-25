using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerSc : MonoBehaviour
{
    private static int numLevels = 5; //TODO: Increase default
    private static int currLevel = 0;

    private static WordGen.WordDB database;

    public static TilemapGen Tilemap;
    public static WordwalkerUIScript uiManager;

    private static bool numLevelsBool = true;

    private void Start()
    {
        //unfortunately the only way i can think of
        Tilemap = FindObjectOfType<TilemapGen>();
        uiManager = FindObjectOfType<WordwalkerUIScript>();
    }

    public static void setParametersOnStart(int numLvl, WordGen.WordDB db)
    {
        Debug.Log("Setting parameters");
        numLevels = numLvl;
        database = db;

        //goToNextLevel();
    }
    
    public static void goToNextLevel()
    {
        uiManager.ResetPostgamePosition();

        if (numLevelsBool) {
            numLevelsBool = false;
            uiManager.SetLevelAmount(numLevels);
        }

        Debug.Log("going to next level: level " + currLevel);
        
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
}
