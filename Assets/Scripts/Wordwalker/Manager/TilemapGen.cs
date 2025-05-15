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

    public bool retryGeneration;  // Turn ON if this is a demo or legit build. Turn OFF if you want to debug/troubleshoot

    public static Dictionary<(int, int), Tile> tileMap;

    private GenMethod[] generationMethods;     //default generation methods
    private GenMethod[] generationMethodsAdv;  //extra generations used only when Gen Plus challenge enabled
    private GenMethod currentGenMethod;

    public TileMats tileMaterials;

    /// <summary>
    /// Generate tile map, depending on if retryGeneration is on we will try multiple times on failure
    /// </summary>
    public void regenerateTileMap(WordGen.Word word)
    {
        if(retryGeneration)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    fineTuning(word);
                    break;
                }
                catch
                {
                    Debug.LogWarning("Failed to generate the tilemap- attempt #" + i);
                }
            }
        }
        
        else
        {
            fineTuning(word);
        }
    }

    /// <summary>
    /// Perfect inputs
    /// </summary>
    private void fineTuning(WordGen.Word word)
    {
        // TODO we take action here to figure out the inputs of the level, they get harder as they go...
        // Maybe we can transform one or a few inputs across all the various inputs across methods?
        float difficultyHandicap = 1;
        if (GameManagerSc.selectedChallenges.Contains(MenuScript.Challenge.GEN_PLUS))
        {
            currentGenMethod = generationMethodsAdv[UnityEngine.Random.Range(0, generationMethodsAdv.Length)];
        }
        else
        {
            difficultyHandicap = 2;
            currentGenMethod = generationMethods[UnityEngine.Random.Range(0, generationMethods.Length)];
        }

        float diff = (float)GameManagerSc.getCurrentLevel() / (float)GameManagerSc.getNumLevels() / difficultyHandicap;
        Debug.Log("Using difficulty " + diff);
        tileMap = currentGenMethod.regenerateTileMap(diff, word);
    }

    // Start is called before the first frame update
    void Start()
    {
        // All methods up for grabs with Gen Plus turned on...
        generationMethodsAdv = GetComponents<GenMethod>();
        // You specify which ones you want for default
        generationMethods = new GenMethod[] { GetComponent<Triangle>() };

        currentGenMethod = generationMethods[0];

        Debug.Log("Tilemap gen READY");
        greenlight = true;
    }
}
