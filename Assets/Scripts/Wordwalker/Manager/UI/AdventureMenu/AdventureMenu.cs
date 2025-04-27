using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the menu for Adventure mode
/// Everything to do with loading databases is handled with DatabaseParser
/// </summary>
public class AdventureMenu : MonoBehaviour
{
    public DatabaseSet[] databaseSets;

    public GameObject databaseDescription;
    public RankBox rankBox;

    public GameObject highScoresContainer;
    public Image[] highScores;
    public GameObject neverWon;

    public Image dbImage;
    public Image dbHighRank;
    public TextMeshProUGUI dbName;
    public TextMeshProUGUI dbDesc;

    public GameObject challengeInfo;
    public Image challengePic;
    public TextMeshProUGUI challengeTitle;
    public TextMeshProUGUI challengeDesc;

    public Image goButton;
    public Image mult;

    // In terms of calculating how much additional challenges bring
    private int numChallenges = 0;
    private float[] multSequence = new float[] { 1, 1.5f, 2, 2.5f, 3, 4, 5};
    public Color[] goButtonColors;
    public Color[] multColors;



    public void displayDatabase(DatabaseItem item)
    {
        //Update title stuff
        dbImage.sprite = item.image;
        dbHighRank.sprite = rankBox.getRankAsSprite(item.highestRank);
        dbName.text = item.displayName;
        dbDesc.text = item.description;

        // If you've never beaten this database display "NEVER WON"
        if(item.highScores.highScores == null)
        {
            neverWon.SetActive(true);
            highScoresContainer.SetActive(false);
            for (int i = 0; i < highScores.Length; i++)
            {
                highScores[i].gameObject.SetActive(false);
            }
        } 
        // If you have beaten it show the high scores
        else
        {
            neverWon.SetActive(false);
            highScoresContainer.SetActive(true);
            for (int i = 0; i < highScores.Length; i++)
            {
                if(i < item.highScores.highScores.Length && item.highScores.highScores[i] != null)
                {
                    highScores[i].gameObject.SetActive(true);
                    highScores[i].sprite = rankBox.getRankAsSprite(item.highScores.highScores[i].rank);
                    highScores[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.highScores.highScores[i].value.ToString();
                    highScores[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = item.highScores.highScores[i].dateAchieved;
                }
                else
                {
                    highScores[i].gameObject.SetActive(false);
                }
            }
        }
        
    }

    public void updateChallengeInfo(bool enabled, Sprite spr, string name, string desc)
    {
        challengeInfo.SetActive(enabled);
        challengePic.sprite = spr;
        challengeTitle.text = name;
        challengeDesc.text = desc;

        if (enabled) numChallenges++;
        else numChallenges--;

        Debug.Log(goButton.color);
        Debug.Log(goButtonColors[numChallenges]);
        goButton.color = goButtonColors[numChallenges];
        mult.color = multColors[numChallenges];
        mult.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "x" + multSequence[numChallenges];
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        ChallengeClick.enable += updateChallengeInfo;
    }

    private void OnDisable()
    {
        ChallengeClick.enable -= updateChallengeInfo;
    }


}