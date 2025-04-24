using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DatabaseParser : MonoBehaviour
{
    public TextAsset dbFile;

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
                //Sprite pic = vars[2] != null && File.Exists(vars[2]) ? null : null;
                //DatabaseItem item = new DatabaseItem(vars[0], vars[1], pic, desc, highScores);
            }

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
