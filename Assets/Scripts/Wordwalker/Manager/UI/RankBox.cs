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
    private int currentRank = 1; // D-

    public Sprite neverBeaten; //only used in showing scores - not included in sprite cycle

    public static int[] scoreThresholds = new int[] { 
        -10000,
        0,
        100,
        150,
        200,
        250,
        300,
        350,
        400,
        500,
        600,
        700,
        800,
        900,
        1000
    };

    /// <summary>
    /// Figure out which new rank you have with this score
    /// </summary>
    public void determineNewRank(int newScore)
    {
        // Above a certain level it's just always the highest score
        // Otherwise, use the highest available score
        int newRank = getRank(newScore);

        StartCoroutine(rotateRank(newRank));
    }

    public static int getRank(int score)
    {
        int newRank = 0;
        // Above a certain level it's just always the highest score
        // Otherwise, use the highest available score
        for (int i = 0; i < scoreThresholds.Length; i++)
        {
            if (i == scoreThresholds.Length - 1)
            {
                newRank = scoreThresholds.Length - 1;
                break;
            }
            if (scoreThresholds[i] > score)
            {
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
        float steps = 30;
        float timeSec = 0.5f;

        for(float i = 0; i <= steps; i++)
        {
            theBox.transform.rotation = Quaternion.Euler(targetAngle * (i / steps), 0, 0);
            yield return new WaitForSeconds(1 / steps * timeSec);
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
        float steps = 30;
        float timeSec = 0.5f;

        for (float i = 0; i <= steps; i++)
        {
            theBox.transform.rotation = Quaternion.Euler(90 * (i / steps), 0, 0);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }

        current.sprite = lower.sprite;

        theBox.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentRank = -1;
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

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(rotateRank(currentRank + 1));
            currentRank++;
        }
    }
}
