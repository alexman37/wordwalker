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

    /*public static Material getCurrentBase(bool stepped, bool correct, )
    {
        Material 
        if (stepped)
        {
            if (correct) changeTo = correctTile;
            else changeTo = incorrectTile;
        }
        else if (specType != SpecialTile.NONE)
        {
            if (specType == SpecialTile.RANDOM) changeTo = spec_random;
            else if (specType == SpecialTile.SPLIT) changeTo = spec_split;
            else if (specType == SpecialTile.FAKE) changeTo = spec_fake;
            else changeTo = spec_blank;
        }
        else
        {
            changeTo = baseTile;
        }
    }*/
}
