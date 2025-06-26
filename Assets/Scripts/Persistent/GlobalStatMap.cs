using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public static class GlobalStatMap
{
    public static StatMap statMap = new StatMap();
    private static string globalStatsFilePath = Path.Combine(Application.persistentDataPath, "wordwalker", "globalStats.json");

    public static void saveGlobalStatMap()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(globalStatsFilePath));

            string dataToStore = JsonConvert.SerializeObject(statMap);

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

                loadedData = JsonConvert.DeserializeObject<StatMap>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("FAILED to load global stats: " + e);
            }
        } else
        {
            Debug.LogWarning("Could not load global stat map.");
        }
        return loadedData;
    }

    public static void AddOrModifyInt(string varName, int val)
    {
        if(statMap.intMap.ContainsKey(varName))
        {
            statMap.intMap[varName] = val;
        } else
        {
            statMap.intMap.Add(varName, val);
        }
        saveGlobalStatMap();
    }

    public static void AddOrModifyFloat(string varName, float val)
    {
        if (statMap.floatMap.ContainsKey(varName))
        {
            statMap.floatMap[varName] = val;
        }
        else
        {
            statMap.floatMap.Add(varName, val);
        }
        saveGlobalStatMap();
    }

    public static void AddOrModifyText(string varName, string txt)
    {
        if (statMap.textMap.ContainsKey(varName))
        {
            statMap.textMap[varName] = txt;
        }
        else
        {
            statMap.textMap.Add(varName, txt);
        }
        saveGlobalStatMap();
    }

    public static void AddOrModifyBool(string varName, bool bs)
    {
        if (statMap.boolMap.ContainsKey(varName))
        {
            statMap.boolMap[varName] = bs;
        }
        else
        {
            statMap.boolMap.Add(varName, bs);
        }
        saveGlobalStatMap();
    }

    public static void AddFlag(string flagName)
    {
        statMap.flags.Add(flagName);
        saveGlobalStatMap();
    }

    public static void RemoveFlag(string flagName)
    {
        statMap.flags.Remove(flagName);
        saveGlobalStatMap();
    }

    public static void ModifySettings(SettingsValues settingsValues)
    {
        statMap.settingsValues = settingsValues;
        saveGlobalStatMap();
    }

    public static void resetAllData()
    {
        // Delete everything but your preferences
        SettingsValues settings = statMap.settingsValues;
        File.Delete(globalStatsFilePath);
        statMap = new StatMap();
        statMap.settingsValues = settings;
    }
}

[System.Serializable]
public class StatMap
{
    ///  Settings
    public SettingsValues settingsValues;

    ///  Other stats
    public Dictionary<string, int> intMap;
    public Dictionary<string, float> floatMap;
    public Dictionary<string, string> textMap;
    public Dictionary<string, bool> boolMap;
    public HashSet<string> flags;

    public StatMap()
    {
        settingsValues = new SettingsValues();
        intMap = new Dictionary<string, int>();
        floatMap = new Dictionary<string, float>();
        textMap = new Dictionary<string, string>();
        boolMap = new Dictionary<string, bool>();
        flags = new HashSet<string>();
    }
}
