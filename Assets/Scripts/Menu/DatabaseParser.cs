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
                HighScore[] highScores = new HighScore[5];
                string[] scores = vars[5].Split(';');

                if(scores.Length > 1) //defaults to empty element if no scores given
                {
                    for (int s = 0; s < scores.Length; s++)
                    {
                        string[] scoresData = scores[s].Split(',');
                        int thisScore = Convert.ToInt32(scoresData[1]);
                        highScores[s] = new HighScore(thisScore, RankBox.getRank(thisScore), scoresData[3]);
                    }
                }
                
                DatabaseItem item = new DatabaseItem(vars[0], vars[2], pic, desc, highScores);
                databaseSet[0].AddDatabase(item); //TODO which set
            }

        }

        //TODO which set
        databaseSet[0].build();
    }

    // Start is called before the first frame update
    void Start()
    {
        parseDatabasesAtStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
