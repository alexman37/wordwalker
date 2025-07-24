using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankBox : MonoBehaviour
{
    private GameObject theBox;
    private Image current;
    private Image upper;
    private Image lower;
    public Sprite[] spriteCycle;
    public Sprite deathSprite;
    public Sprite goldStarSprite;
    private int currentRank = -1;

    public Sprite neverBeaten; //only used in showing scores - not included in sprite cycle

    public static int[] scoreThresholds = new int[14];

    /// <summary>
    /// Setup (for this new game) how many mistakes correspond to each rank
    /// </summary>
    public void setupRankingSystem(float numLevels)
    {
        // By default (short, 10 levels) each mistake costs you one rank.
        // We set ranks up where the lowest rank (0) is the worst score and the 
        for (int i = scoreThresholds.Length - 1; i >= 0; i--)
        {
            // rank 0 = 14 mistakes, rank 1 = 13 mistakes, etc... rank 14 = 0 mistakes
            scoreThresholds[scoreThresholds.Length - 1 - i] = (int)(i * ((float)numLevels / 14f));
        }
    }

    /// <summary>
    /// Figure out which new rank you have with this many mistakes
    /// Use the "illusion of success" - assuming the user will make 1 mistake per level from here on out, what would their rank be?
    /// This makes it appear it's increasing, rather than starting from the top and just going down.
    /// </summary>
    public int determineNewRank(int numMistakes, int levelsToGo)
    {
        // Perfect play - the user will have no levels left to play and no mistakes, thus, they get rank "0" or the highest rank.
        int newRank = getRank(levelsToGo + numMistakes);

        StartCoroutine(rotateRank(newRank));
        return newRank;
    }

    // When restarting the game
    public void defaultRank()
    {
        current.sprite = neverBeaten;

        theBox.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentRank = -1;
    }

    public static int getFinalRank(int numMistakes)
    {
        return getRank(numMistakes);
    }

    public static int getRank(int expectedMistakes)
    {
        int newRank = 0;
        // You must qualify at or above a certain threshold to get that rank
        // ex: A+ = 2 mistakes or less and you get 1 mistake...
        // You qualify for A+, but not for S, which needs 0 mistakes.
        for (int i = 1; i < scoreThresholds.Length; i++)
        {
            // highest possible rank achieved. good job
            if (i == scoreThresholds.Length - 1)
            {
                newRank = scoreThresholds.Length - 1;
                break;
            }
            // you don't qualify for this rank so settle for the previous one
            else if (scoreThresholds[i] < expectedMistakes)
            {
                Debug.Log("Found rank: " + scoreThresholds[i] + " was less than " + expectedMistakes + " expected (rank " + i + ")");
                newRank = i - 1;
                break;
            }
        }

        return newRank;
    }

    public Sprite getRankAsSprite(int rank)
    {
        if (rank < 0) return neverBeaten;
        return spriteCycle[rank];
    }

    IEnumerator rotateRank(int toNewRank)
    {
        if (toNewRank == currentRank) yield break;

        // Set the next sprite
        bool up = toNewRank > currentRank;
        if (up) upper.sprite = spriteCycle[toNewRank];
        else lower.sprite = spriteCycle[toNewRank];

        // Begin rotation animation - either up or down
        float targetAngle = 90.01f * (up ? -1 : 1);
        float timeSec = 0.5f;

        for(float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            theBox.transform.rotation = Quaternion.Euler(targetAngle * Mathf.Clamp(i / timeSec, 0, 1), 0, 0);
            yield return null;
        }

        if (up) current.sprite = upper.sprite;
        else current.sprite = lower.sprite;

        theBox.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentRank = toNewRank;
    }

    /// <summary>
    /// On death, rotate the rankbox to an appropriate sprite
    /// </summary>
    public void onDeath(GameManagerSc.LossReason _)
    {
        StartCoroutine(rotateToDeath());
    }

    IEnumerator rotateToDeath()
    {
        lower.sprite = deathSprite;

        // Begin rotation animation - either up or down
        float timeSec = 0.5f;

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            theBox.transform.rotation = Quaternion.Euler(90 * Mathf.Clamp(i / timeSec, 0, 1), 0, 0);
            yield return null;
        }

        current.sprite = lower.sprite;

        theBox.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentRank = -1;
    }

    /// <summary>
    /// On Gold star, do the same idea
    /// </summary>
    public void awardGoldStar()
    {
        StartCoroutine(rotateToGoldStar());
    }

    IEnumerator rotateToGoldStar()
    {
        upper.sprite = goldStarSprite;

        // Begin rotation animation - either up or down
        float timeSec = 0.5f;

        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            theBox.transform.rotation = Quaternion.Euler(-90 * Mathf.Clamp(i / timeSec, 0, 1), 0, 0);
            yield return null;
        }

        current.sprite = upper.sprite;

        theBox.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentRank = 14;
    }

    // Start is called before the first frame update
    void Start()
    {
        theBox = this.gameObject;
        current = theBox.transform.GetChild(0).GetComponent<Image>();
        upper = theBox.transform.GetChild(1).GetComponent<Image>();
        lower = theBox.transform.GetChild(2).GetComponent<Image>();
    }

    private void OnEnable()
    {
        GameManagerSc.gameOver += onDeath;
    }

    private void OnDisable()
    {
        GameManagerSc.gameOver -= onDeath;
    }
}
