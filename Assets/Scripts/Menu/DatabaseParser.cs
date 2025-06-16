using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class DatabaseParser : MonoBehaviour
{
    public TextAsset dbFile;

    public Sprite defaultImg;

    public DatabaseSet[] databaseSet;

    private AdventureMenu adventureMenu;

    public void parseDatabasesAtStart()
    {
        HashSet<DatabaseItem> dbItemFullSet = new HashSet<DatabaseItem>();

        string[] raw = dbFile.text.Split('\n');

        for (int i = 0; i < raw.Length; i++)
        {
            string currLine = raw[i];
            currLine = currLine.Trim();

            // Ignore comments and empty lines.
            if (currLine.Length > 0 && currLine[0] != '#')
            {
                // Create a new database item from the data we have been given
                string[] vars = currLine.Split('|');
                // Load database icon from the 'dbicon' asset bundle.

                string pic = null;
                if(vars[3] != null && vars[3] != "") {
                    pic = vars[3];
                }
                string desc = vars[4]; // TODO better error handling if we're doing custom databases.

                // Max backtracks allowed in generation (generally, smaller words get less)
                int maxBacktracks = 0;
                if (vars.Length > 5 && vars[5] != null && vars[5] != "") maxBacktracks = Convert.ToInt32(vars[5]);

                // We hard-code size to avoid having to actually iterate through these databases before using them.
                int dbSize = -1;
                if(vars.Length > 6 && vars[6] != null && vars[6] != "") dbSize = Convert.ToInt32(vars[6]);

                // Is it an image database or not?
                string imageDB = null;
                if (vars.Length > 7 && vars[7] != null)
                {
                    if(vars[7].Trim() != "")
                    {
                        imageDB = vars[7].Trim();
                    }
                }

                // Add the item to the correct database.
                DatabaseItem item = new DatabaseItem(vars[1], vars[2], pic, desc, maxBacktracks, dbSize, imageDB);
                item.loadedIcon = defaultImg;
                dbItemFullSet.Add(item);
                foreach(DatabaseSet dbSet in databaseSet)
                {
                    if(dbSet.dbName == vars[0])
                    {
                        dbSet.AddDatabase(item);
                        break;
                    }
                }

                DatabaseTracker.initializeDatabaseTracker(item.databaseId);
            }
        }

        for(int i = 0; i < databaseSet.Length; i++)
        {
            databaseSet[i].build(i);
        }

        StartCoroutine(LoadIconAsset(dbItemFullSet));

        // Automatically queue up the first in the list for viewing
        adventureMenu.displayDatabase(databaseSet[0].getFirst());
    }

    // Start is called before the first frame update
    void Start()
    {
        adventureMenu = FindObjectOfType<AdventureMenu>();
        parseDatabasesAtStart();
    }


    // Try to get the icon for a database, if one is specified.
    // TODO, some error handling would be nice, we just use defaultImg if something goes wrong
    // Apparently can't use try/catch with yields...what i get for trying to be proactive...
    private IEnumerator LoadIconAsset(HashSet<DatabaseItem> dbitems)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, "dbicon");

        //Load designated AssetBundle (word group)
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

        //Load the text file proper
        foreach (DatabaseItem dbitem in dbitems)
        {
            if(dbitem.iconPath != null)
            {
                AssetBundleRequest asset = assetBundle.LoadAssetAsync<Sprite>(dbitem.iconPath);
                yield return asset;

                //Retrieve the object
                Sprite raw = asset.asset as Sprite;

                dbitem.RequestRedraw(raw);

                Debug.Log("Completed loading " + dbitem.iconPath);
            }
        }

        assetBundle.Unload(false);
    }
}
