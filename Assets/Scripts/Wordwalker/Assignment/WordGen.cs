using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class WordGen
{
    public static bool greenlight = false;

    public class Word
    {
        public string word;
        public string clue;
        public string definition; //TODO: could also be a picture

        public Word(string w, string c)
        {
            word = w;
            clue = c;
        }

        public Word(string w, string c, string d)
        {
            word = w;
            clue = c;
            definition = d;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Word w2 = obj as Word;
            if ((System.Object)w2 == null)
                return false;

            return word == w2.word;
        }

        public bool Equals(Word w2)
        {
            return w2.word == word;
        }
    }

    public class WordDatabase
    {
        private int size;
        private Word[] words;

        public WordDatabase(int len)
        {
            size = len;
            words = new Word[len];

            // gotta fill with something for default values...
            for(int i = 0; i < words.Length; i++)
            {
                words[i] = new Word("BASICWORD", "A clue...");
            }
        }

        public void parseFromFileRaw(string[] split)
        {
            for (int i = 0; i < split.Length; i++)
            {
                string[] val = split[i].Split('-');

                //TODO: better logic for separating these files - hyphen isn't good enough
                words[i] = new Word(val[0].Trim().ToUpper(),  val[1].Trim());
            }
        }

        public Word getRandomWord()
        {
            int index = Random.Range(0, size);
            return words[index];
        }

        public Word[] getEntireList()
        {
            return words;
        }
    }

    //The active database - in some way or another, it's supplied in the main menu
    private static WordDatabase activeDatabase = new WordDatabase(1);

    // Get a single random word from the database
    public static Word getRandomWord()
    {
        return activeDatabase.getRandomWord();
    }

    // Return a list of unique words from the database
    public static Word[] getTailoredList(int length)
    {
        List<Word> workingCopy = new List<Word>(activeDatabase.getEntireList());
        HashSet<Word> currentChosen = new HashSet<Word>();
        Word[] returned = new Word[length];

        //TODO should handle this better
        if (length > workingCopy.Count) throw new System.Exception("FATAL ERROR - not enough words in DB!");

        for(int i = 0; i < length; i++)
        {
            int randIdx;
            do
            {
                randIdx = Random.Range(0, workingCopy.Count);
            } while (currentChosen.Contains(workingCopy[randIdx]));

            Word chosen = workingCopy[randIdx];
            workingCopy.RemoveAt(randIdx);
            returned[i] = chosen;
        }

        return returned;
    }

    // Return a list of unique words, excluding all words in the given set
    public static Word[] getTailoredList(int length, List<Word> excludeThese)
    {
        List<Word> workingCopy = new List<Word>(activeDatabase.getEntireList());
        foreach (Word wx in excludeThese){
            workingCopy.Remove(wx);
        }
        
        HashSet<Word> currentChosen = new HashSet<Word>();
        Word[] returned = new Word[length];

        //TODO should handle this better
        if (length > workingCopy.Count) throw new System.Exception("FATAL ERROR - not enough words in DB!");

        for (int i = 0; i < length; i++)
        {
            int randIdx;
            do
            {
                randIdx = Random.Range(0, workingCopy.Count);
            } while (currentChosen.Contains(workingCopy[randIdx]));

            Word chosen = workingCopy[randIdx];
            workingCopy.RemoveAt(randIdx);
            returned[i] = chosen;
        }

        return returned;
    }

    private static WordDatabase setupDatabase(TextAsset raw)
    {
        WordDatabase newDB;

        Debug.Log(raw);
        Debug.Log(raw.text);
        string[] split = raw.text.Split('\n');

        newDB = new WordDatabase(split.Length);
        newDB.parseFromFileRaw(split);

        return newDB;
    }

    public static IEnumerator LoadAsset(string assetBundleName, string objectNameToLoad)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);
        Debug.Log("Attempting to load " + filePath);

        //Load designated AssetBundle (word group)
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;
        Debug.Log(assetBundleCreateRequest);

        AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
        Debug.Log(assetBundle);

        //Load the text file proper
        AssetBundleRequest asset = assetBundle.LoadAssetAsync<TextAsset>(objectNameToLoad);
        yield return asset;
        Debug.Log(asset);
        Debug.Log(asset.asset);

        //Retrieve the object
        TextAsset raw = asset.asset as TextAsset;
        Debug.Log(raw);

        //We only have one active database loaded at a time- so load this one up.
        activeDatabase = setupDatabase(raw);

        assetBundle.Unload(false);

        //TODO: Remove this
        Debug.Log("the word database is set up");
        greenlight = true;
    }
}
