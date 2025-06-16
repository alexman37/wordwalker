using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatSnake : GenMethod
{
    public override Dictionary<(int, int), Tile> generateShape(float difficulty, string word, int maxBacks)
    {
        throw new System.NotImplementedException();
    }

    protected override void findAdjacencies(int subInterval)
    {
        throw new System.NotImplementedException();
    }

    protected override List<Tile> generateWordPath(List<Tile> startingCandidates, string word, int backTracksRemaining, int numBlanks)
    {
        throw new System.NotImplementedException();
    }

    protected override void fillInOtherTiles(int subInterval)
    {
        throw new System.NotImplementedException();
    }

    protected override int generateNumBacktracks(int wordLen, float chancePer, int maxAllowed)
    {
        throw new System.NotImplementedException();
    }
}
