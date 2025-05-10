using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : GenMethod
{
    // BEFORE RUN
    public int subsOnStartingRow;
    public int maxSubs;

    // IN-USE
    private int settledRows;  //Number of rows generated may be less than the length of the word.
    private string word;

    // TODO: MOVE TO A SEPARATE, RELATED SCRIPT DICTATING TILE SHAPE / GRID PATTERN
    private static float xSpacing = 4.15f; //4f;
    private static float ySpacing = 1.7f * 2f; //1.633f * 2f;
    public GameObject baseTile;

    // (TODO?) Map / setting generation
    public GameObject endSide;


    public override Dictionary<(int, int), Tile> generateShape(string word)
    {
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

        //First loop - generate increasing rows
        List<Tile> starters = new List<Tile>();
        for (int row = 0; row < settledRows; row++)
        {
            float numSubs = Mathf.Min(row + subsOnStartingRow - 1, maxSubs - 1);
            for (int sub = 0; sub <= row; sub++)
            {
                // We have to "center" the subs if we get to a point where there would be more than allowed.
                // Do this by simply not drawing tiles that fall out of the range.
                if (numSubs == maxSubs - 1)
                {
                    float minAllowedSub = row / 2 - (numSubs / 2);
                    float maxAllowedSub = row / 2 + (numSubs / 2);
                    if (!(sub >= minAllowedSub && sub <= maxAllowedSub))
                    {
                        tileMap[(row, sub)] = null;
                        continue;
                    }
                }

                float xPos = (-xSpacing * (row / 2.0f) + sub * xSpacing);
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
            }
        }

        playerManager.setBounds(minX, maxX, minZ, maxZ, settledRows);

        //Second loop - set adjacencies
        for (int row = 0; row < settledRows; row++)
        {
            for (int sub = 0; sub <= row; sub++)
            {
                Debug.Log((row, sub));
                Tile curr = tileMap[(row, sub)];

                if (curr != null)
                {
                    // Left and right adjacencies
                    if (existsInDictionary(tileMap, (row, sub + 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.E, tileMap[(row, sub + 1)])); }
                    if (existsInDictionary(tileMap, (row, sub - 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.W, tileMap[(row, sub - 1)])); }

                    // Other adjacencies
                    //Debug.Log("NW expected at:" + (pos.y + ySpacing));
                    if (existsInDictionary(tileMap, (row - 1, sub))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.SE, tileMap[(row - 1, sub)])); }
                    if (existsInDictionary(tileMap, (row - 1, sub - 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.SW, tileMap[(row - 1, sub - 1)])); }

                    if (existsInDictionary(tileMap, (row + 1, sub))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.NW, tileMap[(row + 1, sub)])); }
                    if (existsInDictionary(tileMap, (row + 1, sub + 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.NE, tileMap[(row + 1, sub + 1)])); }
                }
                else
                {
                    //Debug.LogError("Error in tile generation - there should be a tile at " + (row, sub));
                }
            }
        }

        corrects = generateWordPath(starters, word, backTracks);
        fillInOtherTiles();
        done(starters);

        return tileMap;
    }

    public override List<Tile> generateWordPath(List<Tile> startingCandidates, string word, int backTracksRemaining)
    {
        int currRow = 0;
        int currLetter = 0;
        List<Tile> corrects = new List<Tile>();

        //pick a starter
        Tile curr = startingCandidates[UnityEngine.Random.Range(0, subsOnStartingRow)];

        //TODO: Will only go upwards for now
        List<Adjacency> nextCandidates = new List<Adjacency>();

        while (currLetter < word.Length)
        {
            nextCandidates.Clear();
            corrects.Add(curr);

            curr.setLetter(word[currLetter], true);
            //Debug.Log("Curr letter #" + currLetter + " is " + word[currLetter] + ", there are " + backTracksRemaining + " back tracks remaining");

            foreach (Adjacency adj in curr.adjacencies)
            {
                // First, you can't overwrite tiles already used in the path
                if (!adj.tile.isFinalized())
                {
                    // Next, just go up for the rest of the path if you have no choice
                    if (backTracksRemaining == 0)
                    {
                        if (adj.direction == Adjacency.Direction.NW || adj.direction == Adjacency.Direction.NE)
                        {
                            nextCandidates.Add(adj);
                        }
                    }
                    // Or go anywhere BUT up if you're looking at the last row but have more than 1 letter to go
                    else if (word.Length - currLetter > 1 && settledRows - currRow == 2)
                    {
                        if (adj.direction == Adjacency.Direction.E || adj.direction == Adjacency.Direction.W)
                        {
                            nextCandidates.Add(adj);
                        }
                        //You can go backwards as long as you have more than just 1 backtrack remaining (going backwards is two backtracks)
                        if (backTracksRemaining > 1 &&
                            (adj.direction == Adjacency.Direction.SE || adj.direction == Adjacency.Direction.SW))
                        {
                            nextCandidates.Add(adj);
                        }
                    }
                    // Otherwise feel free to go in any direction.
                    // TODO: Prevent being "trapped"
                    else
                    {
                        if (backTracksRemaining > 1 && (adj.direction == Adjacency.Direction.SE || adj.direction == Adjacency.Direction.SW))
                            nextCandidates.Add(adj);
                        else if (adj.direction != Adjacency.Direction.SE && adj.direction != Adjacency.Direction.SW)
                            nextCandidates.Add(adj);
                    }
                }
            }
            currLetter++;
            if (currLetter < word.Length)
            {
                Adjacency chosenAdj = nextCandidates[UnityEngine.Random.Range(0, nextCandidates.Count)];
                switch (chosenAdj.direction)
                {
                    case Adjacency.Direction.NW: case Adjacency.Direction.NE: currRow++; break;
                    case Adjacency.Direction.E: case Adjacency.Direction.W: backTracksRemaining--; break;
                    case Adjacency.Direction.SW: case Adjacency.Direction.SE: backTracksRemaining -= 2; currRow--; break;
                }
                curr = chosenAdj.tile;
            }
        }
        return corrects;
    }

    public override void fillInOtherTiles()
    {
        for (int row = 0; row < settledRows; row++)
        {
            for (int sub = 0; sub <= row; sub++)
            {
                Tile curr = tileMap[(row, sub)];
                if (curr != null && !curr.isFinalized())
                {
                    bool neighborToPath = false;
                    foreach (Adjacency adj in curr.adjacencies)
                    {
                        if (adj.tile.correct)
                        {
                            neighborToPath = true;
                            break;
                        }
                    }

                    if (neighborToPath)
                    {
                        //If the tile borders the path we have to be more careful about what letter we choose
                        //Save it for the next loop- it seems inefficient but it's actually fine - don't ask questions!
                    }
                    else
                    {
                        //Otherwise just do whatever ya want
                        char letter;
                        do
                        {
                            letter = LetterGen.getProportionallyRandomLetter();
                        } while (curr.adjacencies.Exists((Adjacency adj) => adj.tile.letter == letter));


                        curr.setLetter(letter, false);
                    }
                }
            }
        }

        for (int row = 0; row < settledRows; row++)
        {
            for (int sub = 0; sub <= row; sub++)
            {
                Tile curr = tileMap[(row, sub)];
                if (curr != null && !curr.isFinalized())
                {
                    //Don't allow for the same letter to be adjacent to the same tile
                    char letter;
                    do
                    {
                        letter = LetterGen.getCooperativeRandomLetter(curr, word);
                    } while (curr.adjacencies.Exists((Adjacency adj) => adj.tile.letter == letter));

                    curr.setLetter(letter, false);
                }
            }
        }
    }

    public override int generateNumBacktracks(int wordLen, float chancePer, int maxAllowed)
    {
        int currBacktracks = 0;
        while (UnityEngine.Random.value < chancePer)
        {
            currBacktracks++;
            if (currBacktracks >= maxAllowed) break;
        }
        return currBacktracks;
    }
}
