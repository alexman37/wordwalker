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

    /// Actions indicating various phases of the tile gen process.
    public static event Action<List<Tile>> finishedGeneration;
    public static event Action<string, string> regenerate;
    public static event Action<List<Tile>> setCorrects;

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

    /// <summary>
    /// Generates a path for the word - as often times this needs to be done independently from the rest of the shape
    /// </summary>
    public abstract List<Tile> generateWordPath(List<Tile> startingCandidates, string word, int backTracksRemaining);

    /// <summary>
    /// Fill in all tiles besides the correct ones with...something (depending on algorithm of your choice)
    /// </summary>
    public abstract void fillInOtherTiles();

    /// <summary>
    /// Generate number of backtracks this word will have.
    /// </summary>
    public abstract int generateNumBacktracks(int wordLen, float chancePer, int maxAllowed);


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
