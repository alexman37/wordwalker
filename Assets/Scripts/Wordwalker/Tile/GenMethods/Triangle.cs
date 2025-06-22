using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Starts in a single area (potentially multiple tiles), then branches out from there.
/// </summary>
public class Triangle : GenMethod
{
    // BEFORE RUN
    private int subsOnStartingRow;
    private int maxSubs;


    public override Dictionary<(int, int), Tile> generateShape(float difficulty, string word, int maxBacks)
    {
        this.difficulty = difficulty;
        subsOnStartingRow = getRandomInput(difficulty, 1, 3, true);
        maxSubs = getRandomInput(difficulty, 4, 8, true);

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

        //First loop - generate increasing rows
        List<Tile> starters = new List<Tile>();
        List<Tile> enders = new List<Tile>();
        float oddRowOffset = 0;

        for (int row = 0; row < settledRows; row++)
        {
            oddRowOffset = row % 2 == 1 ? xSpacing / 2.0f : 0;

            float numSubs = Mathf.Min(row + subsOnStartingRow - 1, maxSubs - 1);
            float proposedSubs = row % 2 == 0 ? Mathf.Ceil(maxSubs / 2 - (numSubs / 2)) : Mathf.Floor(maxSubs / 2 - (numSubs / 2));
            float minAllowedSub = proposedSubs;
            float maxAllowedSub = proposedSubs + numSubs;

            for (int sub = 0; sub <= maxSubs; sub++)
            {
                // We have to "center" the subs if we get to a point where there would be more than allowed.
                // Do this by simply not drawing tiles that fall out of the range.
                if (!(sub >= minAllowedSub && sub <= maxAllowedSub))
                {
                    tileMap[(row, sub)] = null;
                    continue;
                }

                float xPos =  (maxSubs * xSpacing / 2) - sub * xSpacing - oddRowOffset;
                Vector3 pos = new Vector3(xPos, 0, ySpacing * row);
                GameObject next = GameObject.Instantiate(baseTile, pos, baseTile.transform.rotation);
                next.transform.parent = container.transform;

                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.z < minZ) minZ = pos.z;
                if (pos.z > maxZ) maxZ = pos.z;

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
        findAdjacencies(maxSubs);

        generateStartAndEndDivots(starters, enders);
        
        corrects = generateWordPath(starters, word, backTracks, numBlanks);
        fillInOtherTiles(maxSubs);
        addSpecialTiles();
        done(starters);

        return tileMap;
    }

    // (Word path generation is the default)

    // (fill other tiles is the default)

    // (generate num backtracks is the default)
}
