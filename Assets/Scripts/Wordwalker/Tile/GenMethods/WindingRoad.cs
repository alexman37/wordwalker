using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindingRoad : GenMethod
{
    // BEFORE RUN
    private int minSubs;
    private int maxSubs;

    // IN-USE
    private int settledSubs;

    public override Dictionary<(int, int), Tile> generateShape(float difficulty, string word, int maxBacks)
    {
        this.difficulty = difficulty;
        minSubs = getRandomInput(difficulty, 1, 3, true);
        maxSubs = getRandomInput(difficulty, 3, 6, true);

        this.word = word;

        GameObject.Destroy(container);
        container = new GameObject();

        // We'll have to keep track of the mins and maxes
        float minX = 1000;
        float maxX = -1000;
        float minZ = 1000;
        float maxZ = -1000;

        //TODO configure
        int backTracks = generateNumBacktracks(word.Length, 0.4f, maxBacks);
        int numBlanks = 0;
        if (GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.SPECIAL_TILES))
        {
            numBlanks = generateNumBacktracks(word.Length, 0.25f, maxBacks+1);
        }
        settledRows = word.Length - backTracks + numBlanks;

        //TODO maybe we end up changing this? Or maybe we do that in fat snake instead.
        settledSubs = Random.Range(minSubs, maxSubs + 1);

        // To allow for maneuverability of the winding road we need to have a bigger grid
        float gridSubs = maxSubs * 2;
        int windingOffset = 0;

        //First loop - generate constant number of subs in each row
        List<Tile> starters = new List<Tile>();
        List<Tile> enders = new List<Tile>();
        for (int row = 0; row < settledRows; row++)
        {
            float oddRowOffset = row % 2 == 1 ? xSpacing / 2.0f : 0;

            float minAllowedSub = row % 2 == 0 ? Mathf.Ceil(gridSubs / 2 - (settledSubs / 2)) : Mathf.Floor(gridSubs / 2 - (settledSubs / 2));
            float maxAllowedSub = minAllowedSub + settledSubs - 1;

            windingOffset += (row % 2 == 1 ? Random.Range(-1, 1) : Random.Range(0, 2));
            minAllowedSub += windingOffset;
            maxAllowedSub += windingOffset;

            for (int sub = 0; sub <= gridSubs; sub++)
            {
                // We have to "center" the subs if we get to a point where there would be more than allowed.
                // Do this by simply not drawing tiles that fall out of the range.
                if (!(sub >= minAllowedSub && sub <= maxAllowedSub))
                {
                    tileMap[(row, sub)] = null;
                    continue;
                }

                // Set position of real object
                float xPos = (gridSubs * ySpacing / 2) - sub * xSpacing - oddRowOffset;
                Vector3 pos = new Vector3(xPos, 0, ySpacing * row);
                GameObject next = GameObject.Instantiate(baseTile, pos, baseTile.transform.rotation);
                next.transform.parent = container.transform;

                // Track overall stats
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.z < minZ) minZ = pos.z;
                if (pos.z > maxZ) maxZ = pos.z;

                // Set Tile properties
                Tile t = next.GetComponent<Tile>();
                t.absolutePosition = (pos.x, pos.z);
                t.coords = new Coordinate(row, sub);
                t.physicalObject = next;

                next.name = t.absolutePosition.ToString();

                // The last, and only the last, letter of the word is always in the back row
                // If you manage to get to it by any means necessary you win
                if (row == settledRows - 1) t.isBackRow = true;

                //Once done modifying the new tile, put it in the tileMap
                tileMap[(t.coords.r, t.coords.s)] = t;
                if (row == 0) starters.Add(t);
                if (row == settledRows - 1) enders.Add(t);

                allTiles.Add(t);
            }
        }

        playerManager.setBounds(minX, maxX, minZ, maxZ, settledRows);

        //Second loop - set adjacencies
        findAdjacencies((int)gridSubs);

        generateStartAndEndDivots(starters, enders);

        corrects = generateWordPath(starters, word, backTracks, numBlanks);
        fillInOtherTiles((int)gridSubs);
        addSpecialTiles();
        done(starters);

        return tileMap;
    }
}
