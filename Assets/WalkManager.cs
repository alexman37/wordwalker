using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WalkManager : MonoBehaviour
{
    public Material correctTile;
    public Material incorrectTile;
    public Material highlightTile;
    public Material baseTile;

    public TextMeshProUGUI topWord;

    private List<Tile> possibleNext;

    // Start is called before the first frame update
    void Start()
    {
        possibleNext = new List<Tile>();
        Tile.tileClicked += manageTileClick;
        TilemapGen.finishedGeneration += setStartingTiles;
        TilemapGen.regenerate += reset;
    }

    void reset()
    {
        possibleNext.Clear();
        topWord.text = "";
    }

    void setStartingTiles(List<Tile> starters)
    {
        possibleNext = starters;
    }

    void manageTileClick(Tile t)
    {
        if(possibleNext.Contains(t))
        {
            //Remove all next highlights
            foreach(Tile next in possibleNext)
            {
                next.highlightMaterial(baseTile);
            }

            possibleNext.Clear();
            addLetterToTopWord(t);
            if (t.correct)
            {
                t.stepMaterial(correctTile);
                foreach(Adjacency adj in t.adjacencies)
                {
                    if (!adj.tile.stepped) {
                        possibleNext.Add(adj.tile);
                        adj.tile.highlightMaterial(highlightTile);
                    }
                }
            } else
            {
                t.stepMaterial(incorrectTile);
            }
        }
        else
        {
            //TODO some sort of warning
        }
    }

    //TODO do more with color, etc.
    void addLetterToTopWord(Tile t)
    {
        topWord.text = topWord.text + t.letter.ToString();
    }


}
