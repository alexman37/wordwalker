using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public static class DatabaseTracker
{
    // TODO might need to load in a smarter way
    public static DatabasePersistentStorage databaseTracker = new DatabasePersistentStorage();
    private static string dataTrackerFilePath = Path.Combine(Application.persistentDataPath, "wordwalker/databaseTracker/");

    // Reset after each game
    private static bool DEBUG_RESET = false;

    public static void saveDatabaseTracker(string id)
    {
        try
        {
            // Get the stats - either from our local tracker, or, create it now.
            DatabasePersistentStats getStats;
            if(databaseTracker.databaseStorages.ContainsKey(id))
            {
                getStats = databaseTracker.databaseStorages[id];
            } else
            {
                getStats = new DatabasePersistentStats(0, 0, 3, new HighScoresList(), new HashSet<WordGen.Word>(), 0);
                databaseTracker.databaseStorages[id] = getStats;
            }

            string dataToStore = JsonConvert.SerializeObject(getStats);
            Debug.Log(dataToStore);

            string jsonPath = Path.Combine(dataTrackerFilePath, id + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));

            using (FileStream stream = new FileStream(jsonPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
            Debug.Log("Saved to " + jsonPath);
        }
        catch (Exception e)
        {
            Debug.LogError("FAILED to save database tracker: " + e);
        }
    }

    public static DatabasePersistentStats loadDatabaseTracker(string id)
    {
        DatabasePersistentStats loadedData = null;
        string jsonPath = Path.Combine(dataTrackerFilePath, id + ".json");
        if (File.Exists(jsonPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(jsonPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                loadedData = JsonConvert.DeserializeObject<DatabasePersistentStats>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("FAILED to load global stats: " + e);
            }
        }
        return loadedData;
    }

    // TODO remove completely...just wanna see the setup...
    public static void initializeDatabaseTracker(string id)
    {
        string jsonPath = Path.Combine(dataTrackerFilePath, id + ".json");
        try
        {
            // Get the stats - either from our local tracker, or, create it now.
            
            if (File.Exists(jsonPath) && !DEBUG_RESET)
            {
                databaseTracker.databaseStorages[id] = loadDatabaseTracker(id);
            }
            else
            {
                saveDatabaseTracker(id);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("FAILED to init database tracker: " + e);
        }
    }

    // Declare a new attempt for this database
    public static void startNewGame(string id)
    {
        // It always should, unless you're starting from WW for debugging purposes
        if (databaseTracker.databaseStorages.ContainsKey(id))
        {
            databaseTracker.databaseStorages[id].attempts += 1;
            saveDatabaseTracker(id);
        }
    }

    // Declare that you've won! At this current database.
    public static void winGame(string id, HighScore score)
    {
        // It always should, unless you're starting from WW for debugging purposes
        if(databaseTracker.databaseStorages.ContainsKey(id))
        {
            databaseTracker.databaseStorages[id].wins += 1;
            databaseTracker.databaseStorages[id].highScores.addNewHighScore(score);
            saveDatabaseTracker(id);
        }
    }

    public static void addToCycle(string id, WordGen.Word word)
    {
        // It always should, unless you're starting from WW for debugging purposes
        if (databaseTracker.databaseStorages.ContainsKey(id))
        {
            databaseTracker.databaseStorages[id].wordCycle.Add(word);
            if(!databaseTracker.databaseStorages[id].allWordsSeen) databaseTracker.databaseStorages[id].wordsDiscovered += 1;
            saveDatabaseTracker(id);
        }
    }

    public static void resetCycle(string id)
    {
        // It always should, unless you're starting from WW for debugging purposes
        if (databaseTracker.databaseStorages.ContainsKey(id))
        {
            databaseTracker.databaseStorages[id].wordCycle.Clear();
            databaseTracker.databaseStorages[id].allWordsSeen = true;
            saveDatabaseTracker(id);
        }
    }
}

/*
 * Persistent vars about a database:
 * High scores, W/L ratio, current words cycle
 */
[System.Serializable]
public class DatabasePersistentStorage
{
    public Dictionary<string, DatabasePersistentStats> databaseStorages;

    public DatabasePersistentStorage()
    {
        databaseStorages = new Dictionary<string, DatabasePersistentStats>();
    }
}

[System.Serializable]
public class DatabasePersistentStats
{
    public HashSet<WordGen.Word> wordCycle;
    public int wins;
    public int attempts;
    public int wordsDiscovered;
    public bool allWordsSeen;

    public HighScoresList highScores;

    public DatabasePersistentStats(int w, int a, int hrank, HighScoresList highs, HashSet<WordGen.Word> cycle, int wordsDisc)
    {
        wordCycle = cycle;
        wins = w;
        attempts = a;
        highScores = highs;
        wordsDiscovered = wordsDisc;
    }
}


// List of high scores - add a new one (maybe), display them, etc...
[System.Serializable]
public class HighScoresList
{
    public HighScore[] highScores;
    public int highestRank; // for ease

    public HighScoresList()
    {
        highScores = new HighScore[5];
        for(int i = 0; i < 5; i++)
        {
            highScores[i] = null; // first time for everything...
        }
        highestRank = -1;
    }

    public HighScoresList(HighScore[] highScores)
    {
        this.highScores = highScores;
        sortHighScores();
    }

    // Add (or attempt to add) a new high score...if it doesn't make the list then return false
    public bool addNewHighScore(HighScore hs)
    {
        if (highScores == null)
        {
            highScores = new HighScore[5];
            highScores[0] = hs;
            return true;
        }


        for (int i = 0; i < highScores.Length; i++)
        {
            // If there's still not 5 high scores, add this and sort immediately- easy peasy
            if (highScores[i] == null)
            {
                highScores[i] = hs;
                sortHighScores();
                return true;
            }
            // If the score numerically beats anything on the list, immediately replace the lowest score then sort
            else if (hs.value >= highScores[i].value)
            {
                highScores[4] = hs;
                sortHighScores();
                return true;
            }
        }

        return false;
    }

    // Sort scores in ascending order of value
    public void sortHighScores()
    {
        HighScore[] sorted = new HighScore[5];

        bool putInPlace = false;
        HighScore temp = null;

        if (highScores != null)
        {
            for (int i = 0; i < highScores.Length; i++)
            {
                if (highScores[i] != null)
                {
                    putInPlace = false;
                    for (int j = 0; j < 5; j++)
                    {
                        if (putInPlace)
                        {
                            HighScore whatever = sorted[j];
                            sorted[j] = temp;
                            temp = whatever;
                        }
                        else if (sorted[j] == null || sorted[j].value <= highScores[i].value)
                        {
                            temp = sorted[j];
                            sorted[j] = highScores[i];
                            putInPlace = true;
                        }
                    }
                }
            }
        }

        highScores = sorted;
        highestRank = highScores[0].rank;
    }
}

[System.Serializable]
public class HighScore
{
    public int value;
    public int rank;
    public string dateAchieved;

    public HighScore(int value, int rank, string dateAchieved)
    {
        this.value = value;
        this.rank = rank;
        this.dateAchieved = dateAchieved;
    }
}
