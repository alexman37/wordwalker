using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WalkManager : MonoBehaviour
{
    public GameManagerSc gameManager;

    public Material correctTile;
    public Material incorrectTile;
    public Material highlightTile;
    public Material baseTile;

    public TopBarUI topBar;
    public TextMeshProUGUI definition;

    public Image playerCharacter;

    private List<Tile> possibleNext;

    private string currWord;
    private string currDef;
    private List<Tile> correctTiles;


    // Start is called before the first frame update
    void Start()
    {
        possibleNext = new List<Tile>();
        Tile.tileClicked += manageTileClick;
        TilemapGen.finishedGeneration += setStartingTiles;
        TilemapGen.regenerate += reset;
        TilemapGen.setCorrects += setCorrect;
        correctTiles = new List<Tile>();
    }

    void reset(string w, string d)
    {
        possibleNext.Clear();
        currWord = w;
        currDef = d;
        topBar.ResetBar();
        setClue();
    }

    void setCorrect(List<Tile> corrects)
    {
        correctTiles.Clear();
        correctTiles = corrects;
    }

    void setStartingTiles(List<Tile> starters)
    {
        //TODO: Not set on the first go
        //Debug.Log("START TILES SET!");
        possibleNext = starters;
    }

    void moveCharacter((float, float) position)
    {
        playerCharacter.transform.position = new Vector3(position.Item1, 0.5f, position.Item2);
    }

    void manageTileClick(Tile t)
    {
        //Debug.Log("Clicked");
        if(possibleNext.Contains(t))
        {
            //Remove all next highlights
            foreach(Tile next in possibleNext)
            {
                next.highlightMaterial(baseTile);
            }

            t.pressAnimation();

            possibleNext.Clear();
            if (t.correct)
            {
                addLetterToTopWord(t, correctTile.color);

                //If it's in the back row, you win!
                t.stepMaterial(correctTile);
                if (t.isBackRow)
                {
                    onWin();
                }
                else
                {
                    foreach (Adjacency adj in t.adjacencies)
                    {
                        if (!adj.tile.stepped)
                        {
                            possibleNext.Add(adj.tile);
                            adj.tile.highlightMaterial(highlightTile);
                        }
                    }
                }
            } else
            {
                addLetterToTopWord(t, incorrectTile.color);
                t.stepMaterial(incorrectTile);
                onLose();
            }
        }
        else
        {
            //TODO some sort of warning
        }
    }

    //TODO do more with color, etc.
    void addLetterToTopWord(Tile t, Color32 color)
    {
        topBar.AddLetterToProgress(t.letter, color);
    }

    void setClue()
    {
        definition.text = currDef;
    }

    void onWin()
    {
        topBar.SetAnswer(this.correctTiles, true);
        topBar.kickOffRotation();
    }

    void onLose()
    {
        topBar.SetAnswer(this.correctTiles, false);
        topBar.kickOffRotation();
    }


}
