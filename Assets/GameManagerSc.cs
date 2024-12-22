using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerSc : MonoBehaviour
{
    private static int numLevels = 5; //TODO: Increase default
    private static int currLevel = 0;

    private static WordGen.WordDB database;
    public static TilemapGen Tilemap;

    private void Start()
    {
        //unfortunately the only way i can think of
        Tilemap = FindObjectOfType<TilemapGen>();
    }

    public static void setParametersOnStart(int numLvl, WordGen.WordDB db)
    {
        Debug.Log("Setting parameters");
        numLevels = numLvl;
        database = db;
    }
    
    public void goToNextLevel()
    {
        Debug.Log("going to next level: level " + currLevel);
        
        if(currLevel == numLevels)
        {
            Debug.Log("youre a winner");

        } else
        {
            currLevel += 1;
            Tilemap.regenerateTileMap();
        }
    }

    public WordGen.WordDB getWordDB()
    {
        return database;
    }

    public int getNumLevels()
    {
        return numLevels;
    }
}
