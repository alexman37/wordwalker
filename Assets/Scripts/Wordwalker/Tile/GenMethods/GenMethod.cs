using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class GenMethod : MonoBehaviour
{
    /// TileMap represents our code-based representation of which tiles are at which coordinates
    /// Container is just the GameObject that contains all the physical tiles
    public Dictionary<(int, int), Tile> tileMap;
    public GameObject container;

    /// The list of all tiles you should step on
    public List<Tile> corrects;

    protected int settledRows;  //Number of rows generated may be less than the length of the word.
    protected string word;

    /// Actions indicating various phases of the tile gen process.
    public static event Action<List<Tile>> finishedGeneration;
    public static event Action<string, string> regenerate;
    public static event Action<List<Tile>> setCorrects;

    public static float xSpacing = 4.15f; //4f;
    public static float ySpacing = 1.7f * 2f; //1.633f * 2f;

    /// These managers needed at several stages
    public GameManagerSc gameManager;
    public PlayerManager playerManager;

    /// <summary>
    /// MUST BE CALLED WHEN THE GEN IS FIRST CREATED - to instantiate actions and the like.
    /// </summary>
    private void Start()
    {
        tileMap = new Dictionary<(int, int), Tile>();
        container = new GameObject();

        gameManager = FindObjectOfType<GameManagerSc>();
        playerManager = FindObjectOfType<PlayerManager>();

        finishedGeneration += (_) => { };
        regenerate += (_,__) => { };
        setCorrects += (_) => { };
    }

    /// <summary>
    /// Generates the complete shape from start to finish. Generally this is the only method of the class you would call externally.
    /// </summary>
    public abstract Dictionary<(int, int), Tile> generateShape(string word);

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
    protected virtual List<Tile> generateWordPath(List<Tile> startingCandidates, string word, int backTracksRemaining)
    {
        int currRow = 0;
        int currLetter = 0;
        List<Tile> corrects = new List<Tile>();

        //pick a starter
        Tile curr = startingCandidates[UnityEngine.Random.Range(0, startingCandidates.Count)];

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
    public Dictionary<(int, int), Tile> regenerateTileMap(WordGen.Word word)
    {
        tileMap.Clear();

        //TODO: definitions currently aren't defined.
        regenerate.Invoke(word.word, word.getClue());
        tileMap = generateShape(word.word);
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
