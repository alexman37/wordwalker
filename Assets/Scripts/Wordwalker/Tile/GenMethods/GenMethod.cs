using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class GenMethod : MonoBehaviour
{
    /// TileMap represents our code-based representation of which tiles are at which coordinates
    /// Container is just the GameObject that contains all the physical tiles
    public Dictionary<(int, int), Tile> tileMap;
    public static GameObject container;

    /// The list of all tiles you should step on
    protected List<Tile> corrects;

    protected int settledRows;  //Number of rows generated may be less than the length of the word.
    protected string word;
    protected List<Tile> allTiles; // We use this for challenges. We can free it if we don't need it

    /// Actions indicating various phases of the tile gen process.
    public static event Action<List<Tile>> finishedGeneration;
    public static event Action<string, string> regenerate;
    public static event Action<List<Tile>> setCorrects;

    public static float xSpacing = 4.15f; //4f;
    public static float ySpacing = 1.7f * 2f; //1.633f * 2f;

    /// This value, from 0-1, affects all the inputs to a given gen method such as how many tiles there will be, how much randomness, how difficult the letters.
    /// To actually apply it...that's up to each method.
    public float difficulty;

    /// These managers needed at several stages
    protected TilemapGen tilemapGen;
    protected GameManagerSc gameManager;
    protected PlayerManager playerManager;

    /// <summary>
    /// MUST BE CALLED WHEN THE GEN IS FIRST CREATED - to instantiate actions and the like.
    /// </summary>
    private void Start()
    {
        tileMap = new Dictionary<(int, int), Tile>();
        container = new GameObject();
        allTiles = new List<Tile>();

        gameManager = FindObjectOfType<GameManagerSc>();
        playerManager = FindObjectOfType<PlayerManager>();
        tilemapGen = FindObjectOfType<TilemapGen>();

        finishedGeneration += (_) => { };
        regenerate += (_,__) => { };
        setCorrects += (_) => { };
    }


    /// <summary>
    /// Generates the complete shape from start to finish. Generally this is the only method of the class you would call externally.
    /// </summary>
    public abstract Dictionary<(int, int), Tile> generateShape(float difficulty, string word);

    protected virtual void findAdjacencies(int subInterval)
    {
        for (int row = 0; row < settledRows; row++)
        {
            for (int sub = 0; sub <= subInterval; sub++)
            {
                Tile curr = tileMap[(row, sub)];

                if (curr != null)
                {
                    // Left and right adjacencies
                    if (existsInDictionary(tileMap, (row, sub + 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.E, tileMap[(row, sub + 1)])); }
                    if (existsInDictionary(tileMap, (row, sub - 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.W, tileMap[(row, sub - 1)])); }

                    // Other adjacencies depend completely on the row
                    if (row % 2 == 0) //EVEN
                    {
                        if (existsInDictionary(tileMap, (row - 1, sub))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.SE, tileMap[(row - 1, sub)])); }
                        if (existsInDictionary(tileMap, (row - 1, sub - 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.SW, tileMap[(row - 1, sub - 1)])); }

                        if (existsInDictionary(tileMap, (row + 1, sub - 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.NW, tileMap[(row + 1, sub - 1)])); }
                        if (existsInDictionary(tileMap, (row + 1, sub))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.NE, tileMap[(row + 1, sub)])); }
                    }
                    else // ODD
                    {
                        if (existsInDictionary(tileMap, (row - 1, sub + 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.SE, tileMap[(row - 1, sub + 1)])); }
                        if (existsInDictionary(tileMap, (row - 1, sub))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.SW, tileMap[(row - 1, sub)])); }

                        if (existsInDictionary(tileMap, (row + 1, sub))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.NW, tileMap[(row + 1, sub)])); }
                        if (existsInDictionary(tileMap, (row + 1, sub + 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.NE, tileMap[(row + 1, sub + 1)])); }
                    }
                }
                else
                {
                    //Debug.LogError("Error in tile generation - there should be a tile at " + (row, sub));
                }
            }
        }
    }

    /// <summary>
    /// Generates a path for the word - as often times this needs to be done independently from the rest of the shape
    /// </summary>
    protected virtual List<Tile> generateWordPath(List<Tile> startingCandidates, string word, int backTracksRemaining, int numBlanks)
    {
        int currRow = 0;
        int currLetter = 0;
        List<Tile> corrects = new List<Tile>();

        // Blank Tile interjection - only if the challenge is active
        if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.SPECIAL_TILES))
        {
            word = interjectBlanks(word, numBlanks);
        }

        //pick a starter
        Tile curr = startingCandidates[UnityEngine.Random.Range(0, startingCandidates.Count)];

        //TODO: Will only go upwards for now
        List<Adjacency> nextCandidates = new List<Adjacency>();

        while (currLetter < word.Length)
        {
            nextCandidates.Clear();
            corrects.Add(curr);

            Debug.Log("currLetter " + word[currLetter]);

            curr.setLetter(word[currLetter], true);

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
            ///Debug.Log("[" + currLetter + "] middle of the road");
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
            ///Debug.Log("[" + (currLetter-1) + "] end of the road");
        }
        return corrects;
    }

    /// <summary>
    /// Fill in all tiles besides the correct ones with...something (depending on algorithm of your choice)
    /// </summary>
    protected virtual void fillInOtherTiles(int subInterval)
    {
        for (int row = 0; row < settledRows; row++)
        {
            for (int sub = 0; sub <= subInterval; sub++)
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
            for (int sub = 0; sub <= subInterval; sub++)
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

    /// <summary>
    /// Generate number of backtracks this word will have.
    /// </summary>
    protected virtual int generateNumBacktracks(int wordLen, float chancePer, int maxAllowed)
    {
        int currBacktracks = 0;
        while (UnityEngine.Random.value < chancePer)
        {
            currBacktracks++;
            if (currBacktracks >= maxAllowed) break;
        }
        return currBacktracks;
    }

    /// <summary>
    /// Add special tiles. They can include the path itself
    /// </summary>
    protected virtual void addSpecialTiles()
    {
        if(GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.SPECIAL_TILES)) {
            // RANDOM tiles - these appear as ? and are unknown until stepped on.
            int numRandoms = Mathf.RoundToInt(4f * difficulty);
            int numRandomsChosen = UnityEngine.Random.Range(0, numRandoms + 1);
            for (int i = 0; i < numRandomsChosen; i++)
            {
                Tile t = getRandomSpecialTile((1 / (float)numRandoms));
                //Special rules:
                //  - Cannot be in the last row
                //  - Cannot directly border another random, fake, or split tile
                if(t != null && !t.isBackRow && t.adjacencies.FindAll(adj => adj.tile.specType != Tile.SpecialTile.NONE).Count == 0)
                {
                    t.setAsSpecialTile(Tile.SpecialTile.RANDOM);
                    t.changeMaterial(tilemapGen.tileMaterials.spec_random);
                }
            }


            // FAKE tiles - these have a certain chance of being what they actually say they are
            int numFakes = Mathf.RoundToInt(8f * difficulty);
            int numFakesChosen = UnityEngine.Random.Range(0, numFakes + 1);
            for (int i = 0; i < numFakesChosen; i++)
            {
                Tile t = getRandomSpecialTile((1 / (float)numFakes));
                //Special rules:
                //  - Cannot be in the last row
                //  - Cannot directly border another random, fake, or split tile
                if (t != null && !t.isBackRow && t.adjacencies.FindAll(adj => adj.tile.specType != Tile.SpecialTile.NONE).Count == 0)
                {
                    t.setAsSpecialTile(Tile.SpecialTile.FAKE);
                    t.changeMaterial(tilemapGen.tileMaterials.spec_fake);
                }
            }

            // SPLIT tiles - may be one letter or another
            int numSplits = Mathf.RoundToInt(6f * difficulty);
            int numSplitsChosen = UnityEngine.Random.Range(0, numSplits + 1);
            for (int i = 0; i < numSplitsChosen; i++)
            {
                Tile t = getRandomSpecialTile((1 / (float)numSplits));
                //Special rules:
                //  - Cannot be in the last row
                //  - Cannot directly border another random, fake, or split tile
                if (t != null && !t.isBackRow && t.adjacencies.FindAll(adj => adj.tile.specType != Tile.SpecialTile.NONE).Count == 0)
                {
                    t.setAsSpecialTile(Tile.SpecialTile.SPLIT);
                    t.changeMaterial(tilemapGen.tileMaterials.spec_split);
                }
            }

            // BLANK tiles - these can (and often should) intercede with the path itself - so we may MOVE part of this into the generateWordPath method...
            int numBlanks = Mathf.RoundToInt(3f * difficulty);
            int numBlanksChosen = UnityEngine.Random.Range(0, numBlanks + 1);
            for (int i = 0; i < numBlanksChosen; i++)
            {
                Tile t = getRandomSpecialTile((1 / (float)numBlanks));
                //Special rules:
                //  - Cannot overtake the path itself (that should have been done earlier.)
                if (t != null && !t.correct && !t.isBackRow)
                {
                    t.setAsSpecialTile(Tile.SpecialTile.BLANK);
                    t.changeMaterial(tilemapGen.tileMaterials.spec_blank);
                }
            }
        }
    }

    /// <summary>
    /// Interject blank tiles into the word, represent with an underscore "_"
    /// It's up to you to handle the underscore later on.
    /// </summary>
    protected string interjectBlanks(string word, int numInterjections)
    {
        // Find random spot for the interjections to occur - but not at the beginning or end.
        for (int i = 0; i < numInterjections; i++)
        {
            int s = UnityEngine.Random.Range(1, word.Length);
            string rest = word.Substring(s);
            word = word.Substring(0, s) + "_" + rest;
        }

        return word;
    }

    /// <summary>
    /// Select a random tile to be special. We should be careful (sorta) about selecting actually correct tiles.
    /// Also, can't return a tile that has already been made special (it's ok if you can't find one - return null)
    /// </summary>
    private Tile getRandomSpecialTile(float chanceOfBeingCorrect)
    {
        // return a tile directly from corrects
        if(UnityEngine.Random.value <= chanceOfBeingCorrect)
        {
            for(int i = 0; i < 10; i++)
            {
                Tile maybe = corrects[UnityEngine.Random.Range(0, corrects.Count)];
                if(maybe.specType == Tile.SpecialTile.NONE)
                {
                    return maybe;
                }
            }

            Debug.LogWarning("Could not find any correct tiles to make special!");
            return null;
        }

        // return any tile (might be correct anyways)
        else
        {
            for (int i = 0; i < 10; i++)
            {
                Tile maybe = allTiles[UnityEngine.Random.Range(0, allTiles.Count)];
                if (maybe.specType == Tile.SpecialTile.NONE)
                {
                    return maybe;
                }
            }

            Debug.LogWarning("Could not find any generic tiles to make special!");
            return null;
        }
    }

    /// <summary>
    /// Based on difficulty, will randomly give you a value
    /// </summary>
    protected int getRandomInput(float difficulty, int min, int max, bool minMeansEasier)
    {
        float interval = 1f / (float)(max - min);
        float chaos = interval / 5f;
        float v = Mathf.Clamp(Mathf.Abs(difficulty % interval) + chaos, 0, 1) * ((float)(max - min) / 2);

        float res = min + (float)(max - min) * difficulty + UnityEngine.Random.Range(-v, v);

        int intRes = Mathf.Clamp(Mathf.RoundToInt(res), min, max);
        if (!minMeansEasier) { intRes = max - (intRes - min); }
        Debug.Log(interval);
        Debug.Log(chaos);
        Debug.Log(v);
        Debug.Log(res);
        Debug.Log(intRes);
        Debug.Log("Diff " + difficulty + " with (" + min + "," + max + ")[" + minMeansEasier + "]: " + intRes);
        return intRes;
    }

    /// <summary>
    /// Get the list of all correct tiles - in some situations you may call this externally.
    /// </summary>
    public List<Tile> getCorrects()
    {
        return corrects;
    }

    /// <summary>
    /// Try regenerating the entire tileMap from scratch.
    /// Only useful in a debugging context for now - but maybe we use it to redo generation on faulty attempts.
    /// </summary>
    public Dictionary<(int, int), Tile> regenerateTileMap(float difficulty, WordGen.Word word)
    {
        tileMap.Clear();
        allTiles.Clear();

        //TODO: definitions currently aren't defined.
        regenerate.Invoke(word.word, word.getClue());
        tileMap = generateShape(difficulty, word.word);
        setCorrects.Invoke(corrects);

        return tileMap;
    }

    // Return true if it exists and is not null in the tileMap
    protected bool existsInDictionary(Dictionary<(int, int), Tile> tmap, (int, int) key)
    {
        return tmap.ContainsKey(key) && tmap[key] != null;
    }

    // Do these action items when finished generation completely. (Needs to know what tiles can be started on)
    protected void done(List<Tile> starters)
    {
        finishedGeneration.Invoke(starters);
    }
}
