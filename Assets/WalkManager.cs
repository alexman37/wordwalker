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

    public TextMeshProUGUI topWord;
    public TextMeshProUGUI definition;
    public TextMeshProUGUI postgame;

    public Image playerCharacter;

    private List<Tile> possibleNext;

    private string currWord;
    private string currDef;


    // Start is called before the first frame update
    void Start()
    {
        possibleNext = new List<Tile>();
        Tile.tileClicked += manageTileClick;
        TilemapGen.finishedGeneration += setStartingTiles;
        TilemapGen.regenerate += reset;
        postgame.transform.parent.gameObject.SetActive(false);
    }

    void reset(string w, string d)
    {
        possibleNext.Clear();
        currWord = w;
        currDef = d;
        topWord.text = "";
        postgame.text = "";
        postgame.transform.parent.gameObject.SetActive(false);
        setClue();
    }

    void setStartingTiles(List<Tile> starters)
    {
        //TODO: Not set on the first go
        //Debug.Log("START TILES SET!");
        possibleNext = starters;
    }

    void pushTileDown(Tile t)
    {
        //each frame, push it down by just a little bit.
        
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
            addLetterToTopWord(t);
            if (t.correct)
            {
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
    void addLetterToTopWord(Tile t)
    {
        topWord.text = topWord.text + t.letter.ToString();
    }

    void setClue()
    {
        definition.text = currDef;
    }

    void onWin()
    {
        postgame.transform.parent.gameObject.SetActive(true);
        postgame.text = "You won! The word was " + currWord;
        postgame.textInfo.characterInfo[0].color = new Color32(1,0,0,1);
    }

    void onLose()
    {
        postgame.transform.parent.gameObject.SetActive(true);
        postgame.text = "You lose...the word was " + currWord;
        postgame.textInfo.characterInfo[0].color = new Color32(1, 0, 0, 1);
        postgame.UpdateFontAsset();
    }


}
