using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public static class WordGen
{
    public static bool greenlight = false;
    public static string resetCycleOnThisWord = "";

    public class Word
    {
        public string word;
        public WordClue[] clues;
        public string definition; //TODO: could also be a picture

        // Specified with only one clue
        public Word(string w, string c)
        {
            word = w;
            clues = new WordClue[] { new WordClue(c) };
        }

        public Word(string w, string c, string d)
        {
            word = w;
            clues = new WordClue[] { new WordClue(c) };
            definition = d;
        }

        //Specified with multiple clues
        public Word(string w, string[] c)
        {
            word = w;
            clues = new WordClue[c.Length];
            for(int i = 0; i < c.Length; i++)
            {
                clues[i] = new WordClue(c[i]);
            }
        }

        public Word(string w, string[] c, string d)
        {
            word = w;
            clues = new WordClue[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                clues[i].clue = c[i];
            }
            // TODO multiple different definitions??
            definition = d;
        }

        public Word(string w, WordClue[] c, string d)
        {
            word = w;
            definition = d;
            clues = c;
        }

        [JsonConstructor]
        public Word(string word)
        {
            this.word = word;
        }

        /// <summary>
        /// Get a random clue for this word, as there can be multiple clues associated with each
        /// </summary>
        public string getClue()
        {
            return clues[Random.Range(0, clues.Length)].clue;
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

    // Clue for a given word - can basically be text or an image
    public class WordClue
    {
        public string clue; // if given alongside an image clue, it will be a caption

        [JsonConstructor]
        public WordClue(string clue)
        {
            this.clue = clue;
        }
    }






    // Set of related words.
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

        // Text clues
        public void parseFromFileRaw(string[] split)
        {
            for (int i = 0; i < split.Length; i++)
            {
                string[] val = split[i].Split('|');

                //Element 0: The word itself (in preferred spelling)
                //Element 1: All descriptions of the word separated by a slash (pick one)
                //Element 2*: Hard bool
                //Element 3*: Alternative spellings (accept these)
                string[] allDescriptions = val[1].Trim().Split('/');
                words[i] = new Word(val[0].Trim().ToUpper(), allDescriptions);
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
        resetCycleOnThisWord = "";
        List<Word> workingCopy = new List<Word>(activeDatabase.getEntireList());

        // If there simply aren't enough words in the DB to handle this sized game, throw an error. That shouldn't happen.
        if (length > workingCopy.Count) throw new System.Exception("FATAL ERROR - not enough words in DB!");

        foreach (Word wx in excludeThese){
            workingCopy.Remove(wx);
        }
        
        HashSet<Word> currentChosen = new HashSet<Word>();
        Word[] returned = new Word[length];

        // If there aren't enough words in the cycle to last the whole game, we'll have to reset the words cycle and go again.
        if (length > workingCopy.Count)
        {
            // First get all remaining unused words.
            for(int i = 0; i < workingCopy.Count; i++)
            {
                returned[i] = workingCopy[i];
            }
            resetCycleOnThisWord = workingCopy[workingCopy.Count-1].word;
            // Now get a random list of words from the leftovers
            List<Word> secondSubset = new List<Word>(activeDatabase.getEntireList());
            foreach (Word wx in returned)
            {
                secondSubset.Remove(wx);
            }
            for (int i = workingCopy.Count; i < length; i++)
            {
                int randIdx;
                do
                {
                    randIdx = Random.Range(0, secondSubset.Count);
                } while (currentChosen.Contains(secondSubset[randIdx]));

                Word chosen = secondSubset[randIdx];
                secondSubset.RemoveAt(randIdx);
                returned[i] = chosen;
            }
            // Now get a random subset of all future words - excluding ones we just used.
        }
        // If there are enough words to last us the whole game, we're fine.
        else
        {
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
        }

        return returned;
    }

    // You can skip WordGen completely in the case of Daily word.
    public static void Skip()
    {
        Debug.Log("Word Gen READY (skipped)");
        greenlight = true;
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

    // Call this on the full list of words when using it for the first time.
    public static IEnumerator LoadAsset(string assetBundleName, string objectNameToLoad)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);

        //Load designated AssetBundle (word group)
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

        //Load the text file proper
        AssetBundleRequest asset = assetBundle.LoadAssetAsync<TextAsset>(objectNameToLoad);
        yield return asset;

        //Retrieve the object
        TextAsset raw = asset.asset as TextAsset;

        //We only have one active database loaded at a time- so load this one up.
        activeDatabase = setupDatabase(raw);

        assetBundle.Unload(false);
        
        Debug.Log("Word Gen READY");
        greenlight = true;
    }



    // Call this when loading each image you want to use in an image DB.
    public static IEnumerator LoadImageAsset(string assetBundleName, string imageToLoad, Image clueBookPicture, (float maxW, float maxH) maxes)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);
        Debug.Log("Attempting to load " + filePath);

        //Load designated AssetBundle (word group)
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

        //Load the text file proper
        AssetBundleRequest asset = assetBundle.LoadAssetAsync<Sprite>(imageToLoad);
        yield return asset;

        //Retrieve the object
        Sprite raw = asset.asset as Sprite;

        //Display the image (and scale appropriately).
        clueBookPicture.sprite = raw;
        if (raw.rect.width > maxes.maxW || raw.rect.height > maxes.maxH)
        {
            float compareW = raw.rect.width / maxes.maxW;
            float compareH = raw.rect.height / maxes.maxH;
            if(compareW > compareH)
            {
                clueBookPicture.rectTransform.sizeDelta = new Vector2(maxes.maxW, raw.rect.height * (maxes.maxW / raw.rect.width));
            } else
            {
                clueBookPicture.rectTransform.sizeDelta = new Vector2(raw.rect.width * (maxes.maxH / raw.rect.height), maxes.maxH);
            }
        } else
        {
            clueBookPicture.rectTransform.sizeDelta = new Vector2(raw.rect.width, raw.rect.height);
        }

        Debug.Log("Ended with size " + clueBookPicture.rectTransform.rect.width + "," + clueBookPicture.rectTransform.rect.height);

        assetBundle.Unload(false);
        
        yield return null;
    }
}
