using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WordGen
{
    public enum WordDB
    {
        STANDARD,
        RIDDLE,
        CROSSWORD,
        HARD,
        PLACES,
        PEOPLE
    }

    public static string getRandomWord(WordDB database)
    {
        switch(database)
        {
            case WordDB.STANDARD: return standard();
            case WordDB.RIDDLE: return filler();
            case WordDB.CROSSWORD: return filler();
            case WordDB.HARD: return filler();
            case WordDB.PLACES: return filler();
            case WordDB.PEOPLE: return filler();
            default:
                Debug.LogError("The word DB supplied is unrecognized or not yet implemented");
                return filler();
        }
    }

    public static string standard()
    {
        return "";
    }

    public static string filler()
    {
        return "BASICWORD";
    }
}
