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
    public void regenerateTileMap(WordGen.Word word, int maxBacktracks)
    {
        if(retryGeneration)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    fineTuning(word, maxBacktracks);
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
            fineTuning(word, maxBacktracks);
        }
    }

    // Spawn treasure or make the door light or whatever we do for the last level
    public void switchDoorType(bool toBlack)
    {
        currentGenMethod.switchDoorType(toBlack);
    }

    /// <summary>
    /// Perfect inputs
    /// </summary>
    private void fineTuning(WordGen.Word word, int maxBacktracks)
    {
        // We take action here to figure out the inputs of the level, they get harder as they go...
        // Some algorithms are generally harder than others
        float difficultyHandicap = 1;
        if (GameManagerSc.state.selectedChallenges.Contains(MenuScript.Challenge.GEN_PLUS))
        {
            currentGenMethod = generationMethodsAdv[UnityEngine.Random.Range(0, generationMethodsAdv.Length)];
        }
        else
        {
            difficultyHandicap = 2;
            // Without gen plus, you can still get the "tricky" algorithms but only if past the halfway point
            if((float)GameManagerSc.getCurrentLevel() / (float)GameManagerSc.getNumLevels() >= 0.5f)
            {
                currentGenMethod = generationMethodsAdv[UnityEngine.Random.Range(0, generationMethodsAdv.Length)];
            } else
            {
                currentGenMethod = generationMethods[UnityEngine.Random.Range(0, generationMethods.Length)];
            }
        }

        float diff = (float)GameManagerSc.getCurrentLevel() / (float)GameManagerSc.getNumLevels() / difficultyHandicap;
        Debug.Log("Using difficulty " + diff);
        tileMap = currentGenMethod.generateTileMap(diff, word, maxBacktracks);
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

    // TODO remove
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            regenerateTileMap(new WordGen.Word("EEEEEEEEE", ""), 1);
        }
    }
}
