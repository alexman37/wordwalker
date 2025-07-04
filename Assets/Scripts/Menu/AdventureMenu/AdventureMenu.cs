using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the menu for Adventure mode
/// Everything to do with loading databases is handled with DatabaseParser
/// </summary>
public class AdventureMenu : WidgetPopup
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
    public TextMeshProUGUI wordsDiscovered;
    public TextMeshProUGUI winRate;
    public GameObject highScoresContainer; // High scores
    public Image[] highScores;
    public GameObject neverWon;

    public Sprite[] challengeStarDisplays;

    // TODO challenges...

    public Image goButton;                  // Go



    private void Start()
    {
        this.Setup();
    }

    public void displayDatabase(DatabaseItem item)
    {
        // Get persistent data
        DatabasePersistentStats dbStats = DatabaseTracker.loadDatabaseTracker(item.databaseId);

        //Update title stuff
        dbImage.sprite = item.loadedIcon; // TODO icon load
        dbHighRank.sprite = rankBox.getRankAsSprite(dbStats.highScores.highestRank);
        dbName.text = item.displayName;
        dbDesc.text = item.description;

        // If you've never beaten this database display "NEVER WON"
        if(dbStats.highScores.highScores == null || dbStats.highScores.highestRank < 0)
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
            //dbStats.highScores.sortHighScores();
            for (int i = 0; i < highScores.Length; i++)
            {
                if(i < dbStats.highScores.highScores.Length && dbStats.highScores.highScores[i] != null)
                {
                    highScores[i].gameObject.SetActive(true);
                    highScores[i].sprite = rankBox.getRankAsSprite(dbStats.highScores.highScores[i].rank);
                    highScores[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dbStats.highScores.highScores[i].value.ToString();
                    highScores[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = dbStats.highScores.highScores[i].dateAchieved;
                    highScores[i].transform.GetChild(2).GetComponent<Image>().sprite = challengeStarDisplays[dbStats.highScores.highScores[i].numStars];
                }
                else
                {
                    highScores[i].gameObject.SetActive(false);
                }
            }
        }

        wordsDiscovered.text = $"Words Discovered\n{dbStats.wordsDiscovered} / {item.size}";
        winRate.text = $"Win Rate\n{dbStats.wins} / {dbStats.attempts}";
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