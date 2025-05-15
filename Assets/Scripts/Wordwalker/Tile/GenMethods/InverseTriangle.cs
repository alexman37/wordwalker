using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseTriangle : GenMethod
{
    // BEFORE RUN
    private int subsOnStartingRow;
    private int maxSubs;
    private int minSubs; // also serves as subs on ending row

    // Maybe base tile will be different for different gen methods? Maybe not...whatever
    public GameObject baseTile;

    // Same logic here
    public GameObject endSide;


    public override Dictionary<(int, int), Tile> generateShape(float difficulty, string word)
    {
        this.difficulty = difficulty;
        subsOnStartingRow = getRandomInput(difficulty, 4, 9, false);
        maxSubs = getRandomInput(difficulty, 5, 10, true);
        minSubs = getRandomInput(difficulty, 1, 3, true);

        this.word = word;

        Destroy(container);
        container = new GameObject();

        // We'll have to keep track of the mins and maxes
        float minX = 1000;
        float maxX = -1000;
        float minZ = 1000;
        float maxZ = -1000;

        //TODO configure
        int backTracks = generateNumBacktracks(word.Length, 0.4f, 3);
        settledRows = word.Length - backTracks;

        endSide.transform.position = new Vector3(endSide.transform.position.x, endSide.transform.position.y, 13f + 3f * settledRows);

        //First loop - generate decreasing rows
        List<Tile> starters = new List<Tile>();
        List<Tile> enders = new List<Tile>();
        float numSubs = 0;
        float oddRowOffset = 0;

        for (int row = 0; row < settledRows; row++)
        {
            oddRowOffset = row % 2 == 1 ? xSpacing / 2.0f : 0;

            // DETERMINE NUM SUBS - Num subs may go up, stay constant, or down from previous row depending on random chance (where allowed)
            if (row == 0) numSubs = subsOnStartingRow;
            // In some cases you must start decreasing
            else if (settledRows - row <= numSubs - minSubs)
            {
                numSubs = Mathf.Max(numSubs - 1, minSubs);
            }
            // In some cases you only can't increase
            else if (settledRows - row <= numSubs - minSubs + 1)
            {
                float chanceOfDecreasing = 0.8f;
                if (Random.value < chanceOfDecreasing)
                {
                    numSubs = Mathf.Max(numSubs - 1, minSubs);
                }
            }
            // In other cases you are allowed to do anything
            else
            {
                float chanceOfDecreasing = 0.4f;
                float chanceOfStaying = 0.4f;

                float roll = Random.value;
                if (roll < chanceOfDecreasing)
                {
                    numSubs = Mathf.Max(numSubs - 1, minSubs);
                } else if(roll > chanceOfDecreasing + chanceOfStaying)
                {
                    numSubs = Mathf.Min(numSubs + 1, maxSubs);
                }
            }

            float minAllowedSub = row % 2 == 0 ? Mathf.Ceil(maxSubs / 2 - (numSubs / 2)) : Mathf.Floor(maxSubs / 2 - (numSubs / 2));
            float maxAllowedSub = minAllowedSub + numSubs - 1;

            // Instantiate the tiles at proper positions
            for (int sub = 0; sub <= maxSubs; sub++)
            {
                // We have to "center" the subs if we get to a point where there would be more than allowed.
                // Do this by simply not drawing tiles that fall out of the range.
                
                if (!(sub >= minAllowedSub && sub <= maxAllowedSub))
                {
                    tileMap[(row, sub)] = null;
                    continue;
                }

                float xPos = -xSpacing * maxSubs / 2.0f + sub * xSpacing + oddRowOffset;
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

        corrects = generateWordPath(enders, word, backTracks);
        fillInOtherTiles(maxSubs);
        addSpecialTiles();
        done(starters);

        return tileMap;
    }

    protected override List<Tile> generateWordPath(List<Tile> finalRow, string word, int backTracksRemaining)
    {
        // Since the player would likely want to work backwards for this genMethod, we will also generate backwards
        int currRow = settledRows - 1;
        int currLetter = word.Length - 1;
        List<Tile> corrects = new List<Tile>();

        //pick a starter on the back row
        Tile curr = finalRow[UnityEngine.Random.Range(0, finalRow.Count)];

        //TODO: Will only go upwards for now
        List<Adjacency> nextCandidates = new List<Adjacency>();

        while (currLetter >= 0)
        {
            nextCandidates.Clear();
            corrects.Add(curr);

            curr.setLetter(word[currLetter], true);

            foreach (Adjacency adj in curr.adjacencies)
            {
                // First, you can't overwrite tiles already used in the path
                if (!adj.tile.isFinalized())
                {
                    // Next, just go down for the rest of the path if you have no choice
                    if (backTracksRemaining == 0 || currRow == settledRows - 1)
                    {
                        if (adj.direction == Adjacency.Direction.SW || adj.direction == Adjacency.Direction.SE)
                        {
                            nextCandidates.Add(adj);
                        }
                    }
                    // Or go anywhere BUT down if you're looking at the last row but have more than 1 letter to go
                    else if (currLetter > 1 && currRow == 1)
                    {
                        if (adj.direction == Adjacency.Direction.E || adj.direction == Adjacency.Direction.W)
                        {
                            nextCandidates.Add(adj);
                        }
                        //You can go backwards as long as you have more than just 1 backtrack remaining (going backwards is two backtracks)
                        if (backTracksRemaining > 1 &&
                            (adj.direction == Adjacency.Direction.NE || adj.direction == Adjacency.Direction.NW))
                        {
                            nextCandidates.Add(adj);
                        }
                    }
                    // Otherwise feel free to go in any direction.
                    // TODO: Prevent being "trapped"
                    else
                    {
                        if (backTracksRemaining > 1 && (adj.direction == Adjacency.Direction.NE || adj.direction == Adjacency.Direction.NW))
                            nextCandidates.Add(adj);
                        else if (adj.direction != Adjacency.Direction.NE && adj.direction != Adjacency.Direction.NW)
                            nextCandidates.Add(adj);
                    }
                }
            }
            currLetter--;
            if (currLetter >= 0)
            {
                Adjacency chosenAdj = nextCandidates[UnityEngine.Random.Range(0, nextCandidates.Count)];
                switch (chosenAdj.direction)
                {
                    case Adjacency.Direction.SW: case Adjacency.Direction.SE: currRow++; break;
                    case Adjacency.Direction.E: case Adjacency.Direction.W: backTracksRemaining--; break;
                    case Adjacency.Direction.NW: case Adjacency.Direction.NE: backTracksRemaining -= 2; currRow--; break;
                }
                curr = chosenAdj.tile;
            }
        }
        corrects.Reverse();
        return corrects;
    }

    // (fill other tiles is the default)

    // (generate num backtracks is the default)
}
