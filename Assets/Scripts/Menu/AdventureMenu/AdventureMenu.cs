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
    // The databases available to choose from
    public DatabaseSet[] databaseSets;

    // Display
    public GameObject databaseDescription;  // Title
    public RankBox rankBox;
    public Image dbImage;
    public Image dbHighRank;
    public TextMeshProUGUI dbName;
    public TextMeshProUGUI dbDesc;
    public GameObject highScoresContainer; // High scores
    public Image[] highScores;
    public GameObject neverWon;

    // TODO challenges...

    public Image goButton;                  // Go



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
        //ChallengeClick.enable += updateChallengeInfo;
    }

    private void OnDisable()
    {
        //ChallengeClick.enable -= updateChallengeInfo;
    }


}