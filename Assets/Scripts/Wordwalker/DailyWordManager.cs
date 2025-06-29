using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class DailyWordManager : MonoBehaviour
{
    [SerializeField] private GameObject dailyWordButton;
    [SerializeField] private TextMeshProUGUI dailyWordText;
    [SerializeField] private TextMeshProUGUI dailyWordDate;
    [SerializeField] private Sprite dailyWordDefaultSpr;
    [SerializeField] private Sprite dailyWordWonSpr;
    [SerializeField] private Sprite dailyWordLostSpr;
    [SerializeField] private GameObject dailyWordStreakStar;
    [SerializeField] private TextMeshProUGUI dailyWordStreakStarText;

    public MenuScript menuScript;

    // The daily word list starts counting from here.
    // So, on this day you'd do word 0. After that, word 1...etc.
    // When you reach the end of the word list, you cycle back around.
    const string DAILY_START_DATE = "06/29/2025";
    DateTime dateOrigin = DateTime.Parse(DAILY_START_DATE);

    // let's not screw around with asset bundle loading this time...just do it locally
    [SerializeField] private TextAsset dailyWordList;

    private string todaysWordLine = "";

    // This means we won't reset the daily word until you leave the main menu- which is fine
    void Awake()
    {
        StatMap globalStats = GlobalStatMap.loadGlobalStatMap();
        DateTime todaysDate = DateTime.Now.Add(TimeSpan.FromDays(0)); // TODO remove when we are finished testing
        string today = todaysDate.ToString("d");

        dailyWordDate.text = today;

        // Get the word itself - it goes on a cycle
        int difference = todaysDate.Subtract(dateOrigin).Days;
        string[] temp = dailyWordList.text.Split('\n');
        int len = temp.Length;
        todaysWordLine = temp[difference % len];

        // You can only play the daily word once per day
        if (globalStats.textMap.ContainsKey("lastKnownDailyWord") &&
            globalStats.textMap["lastKnownDailyWord"] == today)
        {
            Debug.Log("DAILY WORD FROM STORAGE");
            if (globalStats.flags.Contains("dailyWordPlayedToday"))
            {
                string[] wordAndDef = todaysWordLine.Split('|');

                // gray out the daily word. do not allow user to play it again
                if (globalStats.intMap["dailyWordStreak"] == 0)
                {
                    disableButton(false);
                    dailyWordDate.text = "\'" + wordAndDef[0] + "\'";
                    dailyWordStreakStar.SetActive(false);
                    dailyWordStreakStarText.text = "0";
                }
                else
                {
                    disableButton(true);
                    dailyWordDate.text = wordAndDef[0];
                    dailyWordStreakStar.SetActive(true);
                    dailyWordStreakStarText.text = globalStats.intMap["dailyWordStreak"].ToString();
                }
                
            } else
            {
                // keep it open
                dailyWordStreakStar.SetActive(true);
                dailyWordStreakStarText.text = globalStats.intMap["dailyWordStreak"].ToString();
            }
        } 

        // First time setup needed
        else
        {
            Debug.Log("FIRST TIME SETUP");
            dailyWordStreakStar.SetActive(true);
            dailyWordStreakStarText.text = globalStats.intMap["dailyWordStreak"].ToString();

            string previousPlay = globalStats.textMap["lastKnownDailyWord"];
            GlobalStatMap.AddOrModifyText("lastKnownDailyWord", today);

            // If you failed to play the last daily word, reset the streak. So in other words check for:
            // 1. Is last known daily word == yesterday?
            // 2. Did you beat it? (dailyWordPlayedToday == true; the streak will be set to 0 regardless if you lost.)
            if (!globalStats.flags.Contains("dailyWordPlayedToday") ||
                !globalStats.textMap.ContainsKey("lastKnownDailyWord") ||
                !DateTime.Parse(globalStats.textMap["lastKnownDailyWord"]).Add(TimeSpan.FromDays(1)).Equals(todaysDate))
            {
                GlobalStatMap.AddOrModifyInt("dailyWordStreak", 0);
                dailyWordStreakStar.SetActive(false);
                dailyWordStreakStarText.text = "0";
            }
            GlobalStatMap.RemoveFlag("dailyWordPlayedToday");

            enableButton();
        }
    }

    private void disableButton(bool won)
    {
        dailyWordButton.GetComponent<Button>().enabled = false;
        dailyWordButton.GetComponent<Image>().sprite = won ? dailyWordWonSpr : dailyWordLostSpr;
    }

    private void enableButton()
    {
        dailyWordButton.GetComponent<Button>().enabled = true;
        dailyWordButton.GetComponent<Image>().sprite = dailyWordDefaultSpr;
    }


    public void playDailyWord()
    {
        string[] wordAndDef = todaysWordLine.Split('|');
        menuScript.startDailyWordGame(wordAndDef[0], wordAndDef[1]);
    }
}
