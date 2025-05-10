using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manages initial generation of a round - creating the word path and all fake tiles surrounding it.
/// Does NOT manage anything to do with user input afterwards- see WalkManager.
/// </summary>
public class TilemapGen : MonoBehaviour
{
    public static bool greenlight = false;

    public static Dictionary<(int, int), Tile> tileMap;

    public GenMethod generationMethod;

    public void regenerateTileMap(WordGen.Word word)
    {
        /*for(int i = 0; i < 10; i++)
        {
            try
            {
                tileMap = generationMethod.regenerateTileMap(word);
                break;
            }
            catch
            {
                Debug.LogWarning("Failed to generate the tilemap- attempt #" + i);
            }
        }*/
        tileMap = generationMethod.regenerateTileMap(word);

    }

    // Start is called before the first frame update
    void Start()
    {
        //TODO anything we have to do to set up the gen method(s)?

        Debug.Log("Tilemap gen READY");
        greenlight = true;
    }
    
}
