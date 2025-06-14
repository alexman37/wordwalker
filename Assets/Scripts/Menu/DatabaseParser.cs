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
                // TODO: How to load these images in the first place? Should probably asset bundle...yup, i hate it too
                Sprite pic = vars[3] != null && File.Exists(vars[1]) ? null : defaultImg;
                string desc = vars[4]; // TODO better error handling

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
                DatabaseItem item = new DatabaseItem(vars[1], vars[2], pic, desc, dbSize, imageDB);
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
            databaseSet[i].build(i);

        // Automatically queue up the first in the list for viewing
        adventureMenu.displayDatabase(databaseSet[0].getFirst());
    }

    // Start is called before the first frame update
    void Start()
    {
        adventureMenu = FindObjectOfType<AdventureMenu>();
        parseDatabasesAtStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
