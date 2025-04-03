using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LetterGen
{
    //CHANGE these to whatever you like
    private static float spaceTileFactor = 0.000f;
    private static float hiddenTileFactor = 0.001f;

    /// <summary>
    /// Return any letter with no restrictions.
    /// </summary>
    public static char getTotallyRandomLetter()
    {
        int randVal = Random.Range(0, 28);
        return letterBase[randVal];
    }

    /// <summary>
    /// Return any letter, with the frequency of letters based on the "frequency to letter" table
    /// </summary>
    public static char getProportionallyRandomLetter()
    {
        float sumAll = 100f + spaceTileFactor + hiddenTileFactor;
        float curr = 0f;
        float randVal = Random.Range(0, sumAll);

        foreach(float freq in frequencyToLetter.Keys)
        {
            curr += freq;
            if(curr >= randVal)
            {
                return frequencyToLetter[freq];
            }
        }
        return ' ';
    }

    /// <summary>
    /// Return a random letter which agrees with the chosen word
    /// </summary>
    public static char getCooperativeRandomLetter(Coordinate pos, string word)
    {
        //Just keep looking for a letter until you find one
        while (true)
        {
            char letterChosen = getTotallyRandomLetter();
            Tile curr = TilemapGen.tileMap[(pos.r, pos.s)];

            //first, does this letter appear in the given word?
            if(word.Contains(letterChosen.ToString()))
            {
                //find every occurrence of the letter
                //if there's a connection to the prior and next letter in the word, assume it's bad news, just pick another letter
                for (int i = word.IndexOf(letterChosen); i > -1; i = word.IndexOf(letterChosen, i + 1))
                {
                    bool formerMatches = false;
                    bool latterMatches = false;
                    foreach (Adjacency adj in curr.adjacencies)
                    {
                        if (i == 0) formerMatches = true;
                        else if (i > 0 && word[i - 1] == adj.tile.letter) formerMatches = true;
                        if (i == word.Length - 1) latterMatches = true;
                        else if (i < word.Length - 1 && word[i + 1] == adj.tile.letter) latterMatches = true;
                    }
                    if (!(formerMatches && latterMatches))
                    {
                        return letterChosen;
                    }
                }
            }
            //if the letter never appears in the word then we're totally fine.
            else
            {
                return letterChosen;
            }
        }

    }

    static char[] letterBase = { ' ', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
                                        'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '?'};

    //see https://www3.nd.edu/~busiforc/handouts/cryptography/letterfrequencies.html
    static Dictionary<float, char> frequencyToLetter = new Dictionary<float,char>()
    {
        { 11.1607f, 'E' },
        { 8.4966f, 'A' },
        { 7.5809f, 'R' },
        { 7.5488f, 'I' },
        { 7.1635f, 'O' },
        { 6.9509f, 'T' },
        { 6.6544f, 'N' },
        { 5.7351f, 'S' },
        { 5.4893f, 'L' },
        { 4.5388f, 'C' },
        { 3.6308f, 'U' },
        { 3.3844f, 'D' },
        { 3.1671f, 'P' },
        { 3.0129f, 'M' },
        { 3.0034f, 'H' },
        { 2.4705f, 'G' },
        { 2.0720f, 'B' },
        { 1.8121f, 'F' },
        { 1.7779f, 'Y' },
        { 1.2899f, 'W' },
        { 1.1016f, 'K' },
        { 1.0074f, 'V' },
        { 0.2902f, 'X' },
        { 0.2722f, 'Z' },
        { 0.1965f, 'J' },
        { 0.1962f, 'Q' },
        { spaceTileFactor, ' ' },
        { hiddenTileFactor, '?' }
    };

    static Dictionary<char, float> letterToFrequency = new Dictionary<char, float>()
    {
        { 'E', 11.1607f },
        { 'A', 8.4966f },
        { 'R', 7.5809f },
        { 'I', 7.5488f },
        { 'O', 7.1635f },
        { 'T', 6.9509f },
        { 'N', 6.6544f },
        { 'S', 5.7351f },
        { 'L', 5.4893f },
        { 'C', 4.5388f },
        { 'U', 3.6308f },
        { 'D', 3.3844f },
        { 'P', 3.1671f },
        { 'M', 3.0129f },
        { 'H', 3.0034f },
        { 'G', 2.4705f },
        { 'B', 2.0720f },
        { 'F', 1.8121f },
        { 'Y', 1.7779f },
        { 'W', 1.2899f },
        { 'K', 1.1016f },
        { 'V', 1.0074f },
        { 'X', 0.2902f },
        { 'Z', 0.2722f },
        { 'J', 0.1965f },
        { 'Q', 0.1962f },
        { ' ', spaceTileFactor },
        { '?', hiddenTileFactor }
    };
}
