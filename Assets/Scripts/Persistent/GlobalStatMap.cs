using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class GlobalStatMap
{
    public static StatMap statMap;
    private static string globalStatsFilePath = Path.Combine(Application.persistentDataPath, "wordwalker/globalStats.json");

    public static void saveGlobalStatMap()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(globalStatsFilePath));

            string dataToStore = JsonUtility.ToJson(statMap, true);

            using (FileStream stream = new FileStream(globalStatsFilePath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        } catch(Exception e)
        {
            Debug.LogError("FAILED to save global stats: " + e);
        }
    }

    public static StatMap loadGlobalStatMap()
    {
        StatMap loadedData = null;
        if (File.Exists(globalStatsFilePath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(globalStatsFilePath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                loadedData = JsonUtility.FromJson<StatMap>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("FAILED to load global stats: " + e);
            }
        }
        return loadedData;
    }
}

[System.Serializable]
public class StatMap
{
    public static Dictionary<string, int> intMap;
    public static Dictionary<string, float> floatMap;
    public static Dictionary<string, string> textMap;
    public static Dictionary<string, bool> boolMap;
    public static HashSet<string> flags;

    public StatMap()
    {

    }
}
