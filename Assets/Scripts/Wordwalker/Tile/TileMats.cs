using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMats : MonoBehaviour
{
    public Material correctTile;
    public Material incorrectTile;
    public Material highlightTile;
    public Material baseTile;

    public Material spec_random;
    public Material spec_split;
    public Material spec_blank;
    public Material spec_fake;

    public Material highlightTileRandom;
    public Material highlightTileSplit;
    public Material highlightTileFake;
    public Material highlightTileBlank;

    public Texture[] highlightTextures; //BASE
    public Texture[] highlightTexturesRandom;
    public Texture[] highlightTexturesFake;
    public Texture[] highlightTexturesSplit;
    public Texture[] highlightTexturesBlank;
    private bool activelyChanging;
    private Coroutine currChanger = null;
    private int currIndex = 0;

    public Material getCurrentBase(bool marked, bool stepped, bool correct, Tile.SpecialTile specType)
    {
        if (marked) return incorrectTile;
        else if (stepped)
        {
            if (correct) return correctTile;
            else return incorrectTile;
        }
        else if (specType != Tile.SpecialTile.NONE)
        {
            if (specType == Tile.SpecialTile.RANDOM) return spec_random;
            else if (specType == Tile.SpecialTile.SPLIT) return spec_split;
            else if (specType == Tile.SpecialTile.FAKE) return spec_fake;
            else return spec_blank;
        }
        else
        {
            return baseTile;
        }
    }

    public Material getCurrentHighlight(bool marked, bool stepped, bool correct, Tile.SpecialTile specType)
    {
        if (marked) return incorrectTile;
        else if (stepped)
        {
            if (correct) return highlightTileBlank; // TODO Have a separate one for correct and blank...maybe?
            else return incorrectTile;
        }
        else if (specType != Tile.SpecialTile.NONE)
        {
            if (specType == Tile.SpecialTile.RANDOM) return highlightTileRandom;
            else if (specType == Tile.SpecialTile.SPLIT) return highlightTileSplit;
            else if (specType == Tile.SpecialTile.FAKE) return highlightTileFake;
            else return highlightTileBlank;
        }
        else
        {
            return highlightTile;
        }
    }

    public void startHighlightAnimation()
    {
        Debug.Log("thinking about starting it");
        if (currChanger == null)
        {
            Debug.Log("starting");
            activelyChanging = true;
            currChanger = StartCoroutine(changeHighlightSprites(0.3f));
        }
    }

    public void stopHighlightAnimation()
    {
        activelyChanging = false;
        StopCoroutine(currChanger);
        currChanger = null;
    }

    IEnumerator changeHighlightSprites(float every)
    {
        while(activelyChanging)
        {
            highlightTile.mainTexture = highlightTextures[currIndex];
            highlightTileRandom.mainTexture = highlightTexturesRandom[currIndex];
            highlightTileFake.mainTexture = highlightTexturesFake[currIndex];
            highlightTileSplit.mainTexture = highlightTexturesSplit[currIndex];
            highlightTileBlank.mainTexture = highlightTexturesBlank[currIndex];

            currIndex = (currIndex + 1) % highlightTextures.Length;
            yield return new WaitForSeconds(every);
        }
    }

    private void OnEnable()
    {
        startHighlightAnimation();
    }

    private void OnDisable()
    {
        stopHighlightAnimation();
    }
}
