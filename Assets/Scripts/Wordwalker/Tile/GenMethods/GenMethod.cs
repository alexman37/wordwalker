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

    public static int settledRows;  //Number of rows generated may be less than the length of the word.
    protected string word;
    protected string[] alternates;
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

    /// Physical aids with generation
    private static TileDivot startingDivot;
    private static TileDivot endingDivot;
    private static GameObject divotsContainer;
    private static GameObject endSide;
    private static GameObject doorBlack;
    private static GameObject doorWhite;

    // If one day we wish to use different shaped tiles, change this...
    protected static GameObject baseTile;

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

        // There should only be 2
        TileDivot[] divots = FindObjectsOfType<TileDivot>();
        foreach(TileDivot d in divots)
        {
            if (d.starting) startingDivot = d;
            else endingDivot = d;
        }
        divotsContainer = new GameObject();
        divotsContainer.name = "Divots container";

        baseTile = GameObject.FindGameObjectWithTag("BaseTile");
        endSide = GameObject.FindGameObjectWithTag("EndSide");
        if(doorBlack == null && doorWhite == null)
        {
            doorBlack = GameObject.FindGameObjectWithTag("DoorBlack");
            doorWhite = GameObject.FindGameObjectWithTag("DoorWhite");
            doorWhite.SetActive(false);
        }
    }


    /// <summary>
    /// Generates the complete shape from start to finish. Generally this is the only method of the class you would call externally.
    /// </summary>
    public abstract Dictionary<(int, int), Tile> generateShape(float difficulty, string word, int maxBacks);

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

    protected virtual void generateStartAndEndDivots(List<Tile> startingTiles, List<Tile> endingTiles)
    {
        if(startingDivot != null && endingDivot != null)
        {
            startingDivot.GetComponent<MeshRenderer>().enabled = true;
            GameObject.Destroy(divotsContainer);
            divotsContainer = new GameObject();
            divotsContainer.name = "Divots container";

            // STARTING DIVOTS
            float allZ = -4.5f;

            // generate a divot "above" and "below" every tile, unless we already have one there
            HashSet<(float, float)> divotsTaken = new HashSet<(float, float)>();
            for (int i = 0; i < startingTiles.Count; i++)
            {
                (float x, float y) to = startingTiles[i].absolutePosition;
                if(!divotsTaken.Contains((to.x - xSpacing / 2f, allZ)))
                {
                    GameObject anotherDivot = GameObject.Instantiate(startingDivot.gameObject);
                    anotherDivot.gameObject.transform.position = new Vector3(to.x - xSpacing / 2f, startingDivot.transform.position.y, allZ);
                    anotherDivot.transform.SetParent(divotsContainer.transform);
                    anotherDivot.name = "Divot at " + anotherDivot.gameObject.transform.position;
                    divotsTaken.Add((to.x - xSpacing / 2f, allZ));
                }
                if (!divotsTaken.Contains((to.x + xSpacing / 2f, allZ)))
                {
                    GameObject anotherDivot = GameObject.Instantiate(startingDivot.gameObject);
                    anotherDivot.gameObject.transform.position = new Vector3(to.x + xSpacing / 2f, startingDivot.transform.position.y, allZ);
                    anotherDivot.transform.SetParent(divotsContainer.transform);
                    anotherDivot.name = "Divot at " + anotherDivot.gameObject.transform.position;
                    divotsTaken.Add((to.x + xSpacing / 2f, allZ));
                }
            }
            startingDivot.GetComponent<MeshRenderer>().enabled = false;
            endingDivot.GetComponent<MeshRenderer>().enabled = true;

            endSide.transform.position = new Vector3(endSide.transform.position.x, endSide.transform.position.y, 8f + 3.2f * settledRows);

            // ENDING DIVOTS
            allZ = endingTiles[0].absolutePosition.Item2 + 4.5f;

            // generate a divot "above" and "below" every tile, unless we already have one there
            divotsTaken.Clear();
            for (int i = 0; i < endingTiles.Count; i++)
            {
                (float x, float y) to = endingTiles[i].absolutePosition;
                if (!divotsTaken.Contains((to.x - xSpacing / 2f, allZ)))
                {
                    GameObject anotherDivot = GameObject.Instantiate(endingDivot.gameObject);
                    anotherDivot.gameObject.transform.position = new Vector3(to.x - xSpacing / 2f, endingDivot.transform.position.y, allZ);
                    anotherDivot.transform.SetParent(divotsContainer.transform);
                    anotherDivot.name = "Divot at " + anotherDivot.gameObject.transform.position;
                    divotsTaken.Add((to.x - xSpacing / 2f, allZ));
                }
                if (!divotsTaken.Contains((to.x + xSpacing / 2f, allZ)))
                {
                    GameObject anotherDivot = GameObject.Instantiate(endingDivot.gameObject);
                    anotherDivot.gameObject.transform.position = new Vector3(to.x + xSpacing / 2f, endingDivot.transform.position.y, allZ);
                    anotherDivot.transform.SetParent(divotsContainer.transform);
                    anotherDivot.name = "Divot at " + anotherDivot.gameObject.transform.position;
                    divotsTaken.Add((to.x + xSpacing / 2f, allZ));
                }
            }
            endingDivot.GetComponent<MeshRenderer>().enabled = false;
        } else
        {
            Debug.LogWarning("Couldn't set divots! (Failed to find them)");
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
        if(GameManagerSc.state.selectedChallenges.Contains(MenuScript.Challenge.SPECIAL_TILES))
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

        // Set the order here so we can use it in later steps
        for(int i = 0; i < corrects.Count; i++)
        {
            corrects[i].order = i;
        }

        return corrects;
    }

    /// <summary>
    /// Fill in all tiles besides the correct ones with...something (depending on algorithm of your choice).
    /// The general pattern follows 3 steps:
    ///   1. Start by filling in all tiles not connected to the correct path with anything, it doesn't matter.
    ///   2. If this word has alternate accepted spellings, find the different range of tiles and ensure that no tiles in that range
    ///         could accidentally make an alternate spelling.
    ///   3. For all tiles connected to the correct path (which still have not been filled in), fill them in while keeping in mind
    ///         their correct neighbors, and alternate spellings.
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
                    // Letters lists' special generation pattern: All front-row tiles start with same letter
                    if(GameManagerSc.state.lettersList && row == 0)
                    {
                        curr.setLetter(word[0], false);
                        continue;
                    }

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
                    else if(curr.specType != Tile.SpecialTile.BLANK)
                    {
                        //Otherwise just do whatever ya want
                        char letter;
                        do
                        {
                            letter = LetterGen.getProportionallyRandomLetter();
                        } while (curr.adjacencies.Exists((Adjacency adj) => adj.tile.letter == letter));

                        curr.setLetter(letter, false);

                        // If a fake or blank tile we deferred on earlier, we must fill it in now with a second valid letter
                        if(curr.specType == Tile.SpecialTile.FAKE || curr.specType == Tile.SpecialTile.SPLIT)
                        {
                            char letter2;
                            do
                            {
                                letter2 = LetterGen.getProportionallyRandomLetterExcept(letter);
                            } while (curr.adjacencies.Exists((Adjacency adj) => adj.tile.letter == letter));

                            curr.setAsSpecialTile2(curr.specType, letter2);
                        }
                    } else
                    {
                        curr.setLetter('_', false);
                    }
                } else if(curr != null)
                {
                    curr.resetDisplay();
                }
            }
        }

        // If the word has alternate legal ways of being spelled, ensure those are not generated on the map.
        // (We can also display something to show this after the fact.)
        bool hasAlternates = alternates != null && alternates.Length > 0;
        HashSet<char> banned = new HashSet<char>();
        int finalStartBreak = -10;
        int finalEndBreak = -10;
        if (hasAlternates)
        {
            // First the "spelling comparer" - to find out how much of the words we can guarantee are the same.
            // Start at the beginning of the string.
            foreach(string alt in alternates)
            {
                int startBreak = -10;
                int endBreak = -10;

                for(int letter = 0; letter < Mathf.Min(word.Length, alt.Length); letter++)
                {
                    if(word[letter] != alt[letter])
                    {
                        startBreak = letter - 1;
                        break;
                    }
                }

                // Edge case where the word is identical to an alternate up to the alternate's entire length
                if (startBreak == -10)
                {
                    startBreak = Mathf.Min(word.Length, alt.Length);
                    endBreak = Mathf.Max(word.Length, alt.Length);
                } else
                {
                    // Now go to the end of the word, and work backwards. How much on the end can we salvage?
                    for(int letter = 0; letter < Mathf.Min(word.Length, alt.Length); letter++)
                    {
                        int i1 = word.Length - letter - 1;
                        int i2 = alt.Length - letter - 1;
                        if(word[i1] != alt[i2])
                        {
                            endBreak = i1 + 1;
                            break;
                        }
                    }
                    if(endBreak == -10)
                    {
                        startBreak = 0;
                        endBreak = Mathf.Abs(word.Length - alt.Length);
                    }
                }

                int diff = word.Length - alt.Length;
                for(int i = startBreak - diff - 1; i < endBreak - diff + 1; i++)
                {
                    banned.Add(alt[Mathf.Clamp(i, 0, alt.Length - 1)]);
                }

                finalStartBreak = Mathf.Max(finalStartBreak, startBreak);
                finalEndBreak = Mathf.Max(finalEndBreak, endBreak);
            }
            string tilesPrintout = "";
            foreach(char b in banned)
            {
                tilesPrintout += (b + ",");
            }
            Debug.Log("For all neighbors of correct tiles " + finalStartBreak + "-" + finalEndBreak + " do not use the tiles: " + tilesPrintout);
        }


        // Any tiles still not filled in are connected to the correct path. Fill them in and be careful about what they will become.
        for (int row = 0; row < settledRows; row++)
        {
            for (int sub = 0; sub <= subInterval; sub++)
            {
                Tile curr = tileMap[(row, sub)];
                // Special case, row = 0: check for instances of getting too close to generating the word again in another place
                if (row == 0 && curr != null && !curr.correct)
                {
                    if(curr.letter == word[0]) // TODO: eliminate this same possibility for special tiles?
                    {
                        bool tooCloseToDuplicate = false;
                        foreach(Adjacency adj in curr.adjacencies)
                        {
                            if(adj.tile.letter == word[1])
                            {
                                tooCloseToDuplicate = true;
                            }
                        }
                        if(tooCloseToDuplicate)
                        {
                            // For letters lists this will have the silly effect of...not having it start with the letter.
                            // And i'm ok with that. rather be silly than innaccurate.
                            curr.letter = LetterGen.getProportionallyRandomLetterExcept(word[0]);
                            curr.display = curr.letter.ToString();
                            curr.resetDisplay();
                            continue;
                        }
                    }
                }

                if (curr != null && !curr.isFinalized())
                {
                    if(curr.specType == Tile.SpecialTile.BLANK)
                    {
                        curr.setLetter('_', false);
                    } else
                    {
                        char letter;
                        // TODO callback?

                        // We cannot allow for alternate words to generate by accident
                        if (curr.adjacencies.Exists(adj => adj.tile.order >= finalStartBreak && adj.tile.order <= finalEndBreak))
                        {
                            Debug.Log("Using the restrictive list for " + curr);
                            letter = LetterGen.getCooperativeRandomLetter(curr, word, LetterGen.getProportionallyRandomLetter, banned);
                            curr.setLetter(letter, false);

                            if (curr.specType == Tile.SpecialTile.FAKE || curr.specType == Tile.SpecialTile.SPLIT)
                            {
                                char letter2;
                                do
                                {
                                    letter2 = LetterGen.getCooperativeRandomLetter(curr, word, LetterGen.getProportionallyRandomLetter, banned);
                                } while (letter2 == letter);

                                curr.setAsSpecialTile2(curr.specType, letter2);
                            }
                        }
                        else
                        {
                            letter = LetterGen.getCooperativeRandomLetter(curr, word, LetterGen.getProportionallyRandomLetter);
                            curr.setLetter(letter, false);

                            if (curr.specType == Tile.SpecialTile.FAKE || curr.specType == Tile.SpecialTile.SPLIT)
                            {
                                char letter2;
                                do
                                {
                                    letter2 = LetterGen.getCooperativeRandomLetter(curr, word, LetterGen.getProportionallyRandomLetter);
                                } while (letter2 == letter);

                                curr.setAsSpecialTile2(curr.specType, letter2);
                            }
                        }
                    }
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
        if(maxAllowed > 0)
        {
            while (UnityEngine.Random.value < chancePer)
            {
                currBacktracks++;
                if (currBacktracks >= maxAllowed) break;
            }
        }
        return currBacktracks;
    }

    /// <summary>
    /// Add special tiles. They can include the path itself
    /// </summary>
    protected virtual void markSpecialTiles()
    {
        if(GameManagerSc.state.selectedChallenges.Contains(MenuScript.Challenge.SPECIAL_TILES)) {
            // RANDOM tiles - these appear as ? and are unknown until stepped on.
            int numRandoms = Mathf.RoundToInt(4f * difficulty);
            int numRandomsChosen = UnityEngine.Random.Range(0, numRandoms + 1);
            for (int i = 0; i < numRandomsChosen; i++)
            {
                Tile t = getRandomSpecialTile(0.10f);
                //Special rules:
                //  - Cannot be in the last row
                //  - Cannot directly border another random, fake, or split tile
                if (t != null && !t.isBackRow && t.adjacencies.FindAll(adj => adj.tile.specType != Tile.SpecialTile.NONE).Count == 0)
                {
                    t.setAsSpecialTile1(Tile.SpecialTile.RANDOM);
                    t.changeMaterial(tilemapGen.tileMaterials.spec_random);
                }
            }


            // FAKE tiles - these have a certain chance of being what they actually say they are
            int numFakes = Mathf.RoundToInt(8f * difficulty);
            int numFakesChosen = UnityEngine.Random.Range(0, numFakes + 1);
            for (int i = 0; i < numFakesChosen; i++)
            {
                Tile t = getRandomSpecialTile(0.10f);
                //Special rules:
                //  - Cannot be in the last row
                //  - Cannot directly border another random, fake, or split tile
                if (t != null && !t.isBackRow && t.adjacencies.FindAll(adj => adj.tile.specType != Tile.SpecialTile.NONE).Count == 0)
                {
                    // NOT YET - wait until all tiles are filled in to finish.
                    //t.setAsSpecialTile(Tile.SpecialTile.FAKE);
                    t.specType = Tile.SpecialTile.FAKE;
                    if (t.correct) t.setAsSpecialTile2(Tile.SpecialTile.FAKE, LetterGen.getProportionallyRandomLetterExcept(t.letter));
                    t.changeMaterial(tilemapGen.tileMaterials.spec_fake);
                }
            }

            // SPLIT tiles - may be one letter or another
            int numSplits = Mathf.RoundToInt(6f * difficulty);
            int numSplitsChosen = UnityEngine.Random.Range(0, numSplits + 1);
            for (int i = 0; i < numSplitsChosen; i++)
            {
                Tile t = getRandomSpecialTile(0.10f);
                //Special rules:
                //  - Cannot be in the last row
                //  - Cannot directly border another random, fake, or split tile
                if (t != null && !t.isBackRow && t.adjacencies.FindAll(adj => adj.tile.specType != Tile.SpecialTile.NONE).Count == 0)
                {
                    // NOT YET - wait until all tiles are filled in to finish.
                    //t.setAsSpecialTile(Tile.SpecialTile.SPLIT);
                    t.specType = Tile.SpecialTile.SPLIT;
                    if (t.correct) t.setAsSpecialTile2(Tile.SpecialTile.SPLIT, LetterGen.getProportionallyRandomLetterExcept(t.letter));
                    t.changeMaterial(tilemapGen.tileMaterials.spec_split);
                }
            }

            // BLANK tiles - these can (and often should) intercede with the path itself - so we may MOVE part of this into the generateWordPath method...
            int numBlanks = Mathf.RoundToInt(3f * difficulty);
            int numBlanksChosen = UnityEngine.Random.Range(0, numBlanks + 1);
            for (int i = 0; i < numBlanksChosen; i++)
            {
                Tile t = getRandomSpecialTile(0.10f);
                //Special rules:
                //  - Cannot overtake the path itself (that should have been done earlier.)
                if (t != null && !t.correct && !t.isBackRow)
                {
                    t.setAsSpecialTile1(Tile.SpecialTile.BLANK);
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
    public Dictionary<(int, int), Tile> generateTileMap(float difficulty, WordGen.Word word, int maxBacks)
    {
        tileMap.Clear();
        allTiles.Clear();

        //TODO: definitions currently aren't defined.
        alternates = word.alternateSpellings;
        regenerate.Invoke(word.word, word.getClue());
        tileMap = generateShape(difficulty, word.word, maxBacks);
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

    public void switchDoorType(bool toBlack)
    {
        doorWhite.SetActive(!toBlack);
        doorBlack.SetActive(toBlack);
    }
}
