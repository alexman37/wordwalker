using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FunStatUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static (string, string) getFunStat()
    {
        WWGameState state = GameManagerSc.state;

        int statType = Random.Range(1, 101);
        (string n, string v) funStatResult;

        // Nonsense stats
        if(statType >= 99)
        {
            funStatResult = getRandomNonsense(state);
        }

        // "Legitimate" stats
        else
        {
            funStatResult = getRandomLegit(state);
        }

        return funStatResult;
    }

    private static (string, string) getRandomNonsense(WWGameState state)
    {
        int pick = Random.Range(1, 7);
        switch (pick) {
            case 1:
                return ("Who's Awesome", state.getRank() > 3 ? "You are!" : "Not you");
            case 2:
                return ("Czechoslovakia", "is gone");
            case 3:
                return ("Sko", "Birds");
            case 4:
                return ("Times you blinked", Random.Range((int)100, (int)300).ToString());
            case 5:
                return ("Your lucky number", Random.Range((int)0, (int)100).ToString());
            case 6:
            default:
                return ("Days without winning", "0");
        }
    }

    private static (string, string) getRandomLegit(WWGameState state)
    {
        int pick = Random.Range(1, 6);
        switch (pick)
        {
            case 1:
                if (Random.Range((int)0, (int)2) == 0)
                {
                    return ("Tiles stepped on", state.funStuff.tilesStepped.ToString());
                }
                else
                {
                    return ("Distance walked", (state.funStuff.tilesStepped + 6 * state.getNumLevels()) + "m");
                }
            case 2:
                if (Random.Range((int)0, (int)5) < 4)
                {
                    return ("Totems earned", state.funStuff.totemsFound.ToString());
                }
                else
                {
                    return ("Happy misspells", state.funStuff.totemsFound.ToString());
                }
            case 3:
                if (Random.Range((int)0, (int)5) < 4)
                {
                    return ("Items used", state.funStuff.itemsUsed.ToString());
                }
                else
                {
                    return ("Powerup proclivity", state.funStuff.itemsUsed.ToString());
                }
            case 4:
                return ("Longest word", state.funStuff.longestWord.ToString());
            case 5:
            default:
                return ("Toughest word", state.funStuff.toughestWord.word.ToString());
        }
    }
}
