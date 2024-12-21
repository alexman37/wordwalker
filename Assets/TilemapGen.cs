using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TilemapGen : MonoBehaviour
{
    public GameManagerSc gameManager;

    private string word = "HAZELHEATHERS";

    private float xSpacing = 2f; //2.3
    private float ySpacing = 1.633f; //1.8

    public int subsOnStartingRow;
    public int maxSubs;
    private int settledRows;  //Number of rows generated may be less than the length of the word.

    public GameObject baseTile;
    public static Dictionary<(int, int), Tile> tileMap;

    private GameObject container;

    // Map / setting generation
    public GameObject endSide;

    public static event Action<List<Tile>> finishedGeneration;
    public static event Action<string, string> regenerate;

    // Start is called before the first frame update
    void Start()
    {
        tileMap = new Dictionary<(int, int), Tile>();
        container = new GameObject();

        void dummy(List<Tile> t) {};

        finishedGeneration += dummy;
        regenerate += (string a, string b) => { };

        //Get words - all databases go here
        StartCoroutine(WordGen.LoadAsset("worddbs/crossword", "crossword"));

        generateTriangle(word);
        regenerateTileMap();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            regenerateTileMap();
        }
    }

    public void regenerateTileMap()
    {
        (string w, string d) = WordGen.getRandomWord(gameManager.getWordDB());
        word = w;

        regenerate.Invoke(w, d);
        tileMap.Clear();

        generateTriangle(word);
    }


    void generateTriangle(string word)
    {
        Debug.Log("DESTROYING OLD CONTAINER");
        Destroy(container);
        container = new GameObject();

        //TODO configure
        int backTracks = randomBacktracks(word.Length, 0.4f, 3);
        settledRows = word.Length - backTracks;

        endSide.transform.position = new Vector3(endSide.transform.position.x, endSide.transform.position.y, 11.5f + 1.5f * settledRows);

        //First loop - generate increasing rows
        List<Tile> starters = new List<Tile>();
        for (int row = 0; row < settledRows; row++)
        {
            for(int sub = 0; sub <= Mathf.Min(row + subsOnStartingRow - 1,  maxSubs - 1); sub++)
            {
                Vector3 pos = new Vector3(-xSpacing * (row / 2.0f) + sub * xSpacing, 0, ySpacing * row);
                GameObject next = GameObject.Instantiate(baseTile, pos, baseTile.transform.rotation);
                next.transform.parent = container.transform;

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

        //Second loop - set adjacencies
        for (int row = 0; row < settledRows; row++)
        {
            for (int sub = 0; sub <= Mathf.Min(row + subsOnStartingRow - 1, maxSubs - 1); sub++)
            {
                Tile curr = tileMap[(row, sub)];

                if(curr != null)
                {
                    // Left and right adjacencies
                    if (tileMap.ContainsKey((row, sub + 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.E, tileMap[(row, sub + 1)])); }
                    if (tileMap.ContainsKey((row, sub - 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.W, tileMap[(row, sub - 1)])); }

                    // Other adjacencies
                    //Debug.Log("NW expected at:" + (pos.y + ySpacing));
                    if (tileMap.ContainsKey((row - 1, sub))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.SE, tileMap[(row - 1, sub)])); }
                    if (tileMap.ContainsKey((row - 1, sub - 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.SW, tileMap[(row - 1, sub - 1)])); }

                    if (tileMap.ContainsKey((row + 1, sub))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.NW, tileMap[(row + 1, sub)])); }
                    if (tileMap.ContainsKey((row + 1, sub + 1))) { curr.adjacencies.Add(new Adjacency(Adjacency.Direction.NE, tileMap[(row + 1, sub + 1)])); }
                } else
                {
                    Debug.LogError("Error in tile generation - there should be a tile at " + (row, sub));
                }
            }
        }

        generateTriangleWordPath(starters, word, backTracks);
        fillInOtherTiles();
        done(starters);
    }


    void generateTriangleWordPath(List<Tile> startingCandidates, string word, int backTracksRemaining)
    {
        int currRow = 0;
        int currLetter = 0;

        //pick a starter
        Tile curr = startingCandidates[UnityEngine.Random.Range(0, subsOnStartingRow)];

        //TODO: Will only go upwards for now
        List<Adjacency> nextCandidates = new List<Adjacency>();

        while (currLetter < word.Length)
        {
            nextCandidates.Clear();
            curr.setLetter(word[currLetter], true);
            //Debug.Log("Curr letter #" + currLetter + " is " + word[currLetter] + ", there are " + backTracksRemaining + " back tracks remaining");
            
            foreach(Adjacency adj in curr.adjacencies)
            {
                // First, you can't overwrite tiles already used in the path
                if (!adj.tile.isFinalized()) {
                    // Next, just go up for the rest of the path if you have no choice
                    if(backTracksRemaining == 0)
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
                        if(backTracksRemaining > 1 && 
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
                        else if(adj.direction != Adjacency.Direction.SE && adj.direction != Adjacency.Direction.SW)
                            nextCandidates.Add(adj);
                    }
                }
            }
            currLetter++;
            if (currLetter < word.Length)
            {
                Adjacency chosenAdj = nextCandidates[UnityEngine.Random.Range(0, nextCandidates.Count)];
                switch(chosenAdj.direction)
                {
                    case Adjacency.Direction.NW: case Adjacency.Direction.NE: currRow++; break;
                    case Adjacency.Direction.E: case Adjacency.Direction.W: backTracksRemaining--; break;
                    case Adjacency.Direction.SW: case Adjacency.Direction.SE: backTracksRemaining-=2; currRow--; break;
                }
                curr = chosenAdj.tile;
            }
        }
    }

    void fillInOtherTiles()
    {
        for (int row = 0; row < settledRows; row++)
        {
            for (int sub = 0; sub <= Mathf.Min(row + subsOnStartingRow - 1, maxSubs - 1); sub++)
            {
                Tile curr = tileMap[(row, sub)];
                if (!curr.isFinalized())
                {
                    bool neighborToPath = false;
                    foreach(Adjacency adj in curr.adjacencies)
                    {
                        if (adj.tile.correct) { 
                            neighborToPath = true; 
                            break; 
                        }
                    }

                    if(neighborToPath)
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

        for(int row = 0; row < settledRows; row++)
        {
            for(int sub = 0; sub <= Mathf.Min(row + subsOnStartingRow - 1, maxSubs - 1); sub++)
            {
                Tile curr = tileMap[(row, sub)];
                if(!curr.isFinalized())
                {
                    //Don't allow for the same letter to be adjacent to the same tile
                    char letter;
                    do
                    {
                        letter = LetterGen.getCooperativeRandomLetter(curr.coords, word);
                    } while (curr.adjacencies.Exists((Adjacency adj) => adj.tile.letter == letter));

                    curr.setLetter(letter, false);
                }
            }
        }
    }

    void done(List<Tile> starters)
    {
        finishedGeneration.Invoke(starters);
    }
    
    int randomBacktracks(int wordLen, float chancePer, int maxAllowed)
    {
        int currBacktracks = 0;
        while(UnityEngine.Random.value < chancePer)
        {
            currBacktracks++;
            if (currBacktracks >= maxAllowed) break;
        }
        return currBacktracks;
    }
}
