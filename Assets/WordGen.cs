using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    private class WordDatabase
    {
        // Must be indexed in the same positions!
        private int size;
        private string[] words;
        private string[] definitions;

        public WordDatabase(int len)
        {
            size = len;
            words = new string[len];
            definitions = new string[len];
        }

        public void parseFromFileRaw(string[] split)
        {
            for (int i = 0; i < split.Length; i++)
            {
                string[] val = split[i].Split('-');
                words[i] = val[0].Trim().ToUpper();
                definitions[i] = val[1].Trim();
            }
        }

        public (string, string) getRandomWord()
        {
            int index = Random.Range(0, size);
            return (words[index], definitions[index]);
        }
    }

    //The databases
    private static WordDatabase crosswordDB;

    public static (string word, string def) getRandomWord(WordDB database)
    {
        switch(database)
        {
            case WordDB.STANDARD: return filler();
            case WordDB.RIDDLE: return filler();
            case WordDB.CROSSWORD: return crossword();
            case WordDB.HARD: return filler();
            case WordDB.PLACES: return filler();
            case WordDB.PEOPLE: return filler();
            default:
                Debug.LogError("The word DB supplied is unrecognized or not yet implemented");
                return filler();
        }
    }

    public static void setup()
    {
        crosswordDB = setupDatabase("Assets/wordDBs/crossword.txt");
    }

    private static WordDatabase setupDatabase(string filename)
    {
        WordDatabase newDB;

        var sr = new StreamReader(filename);
        string fileContents = sr.ReadToEnd();
        sr.Close();

        string[] split = fileContents.Split('\n');
        newDB = new WordDatabase(split.Length);
        newDB.parseFromFileRaw(split);

        return newDB;
    }

    public static (string, string) crossword()
    {
        return crosswordDB.getRandomWord();
    }

    public static (string, string) filler()
    {
        return ("BASICWORD", "A clue...");
    }
}
