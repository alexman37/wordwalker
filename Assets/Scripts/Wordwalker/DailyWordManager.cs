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

    DateTime todaysDate = DateTime.Now.Add(TimeSpan.FromDays(2)); // TODO remove when we are finished testing

    // let's not screw around with asset bundle loading this time...just do it locally
    [SerializeField] private TextAsset dailyWordList;

    private string todaysWordLine = "";

    // This means we won't reset the daily word until you leave the main menu- which is fine
    void Awake()
    {
        StatMap globalStats = GlobalStatMap.loadGlobalStatMap();
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
            Debug.Log("Resetting the Daily word...");
            // Define for first time if needed
            if (!globalStats.intMap.ContainsKey("dailyWordStreak")) globalStats.intMap["dailyWordStreak"] = 0;
            // Only show star if there is a streak to speak of
            if (globalStats.intMap["dailyWordStreak"] > 0)
            {
                dailyWordStreakStar.SetActive(true);
                dailyWordStreakStarText.text = globalStats.intMap["dailyWordStreak"].ToString();
            } else
            {
                dailyWordStreakStar.SetActive(false);
            }

            // If you failed to play the last daily word, reset the streak. So in other words check for:
            // 1. Is last known daily word == yesterday?
            // 2. Did you beat it?
            // If both of these are not true, either the streak will be 0, or the 'dailyWordPlayedToday' flag won't be set.
            // In either case, skill issue, reset the streak
            if (!globalStats.flags.Contains("dailyWordPlayedToday") ||
                !globalStats.textMap.ContainsKey("lastKnownDailyWord") ||
                !SameDay(DateTime.Parse(globalStats.textMap["lastKnownDailyWord"]).Add(TimeSpan.FromDays(1)), todaysDate))
            {
                GlobalStatMap.AddOrModifyInt("dailyWordStreak", 0);
                dailyWordStreakStar.SetActive(false);
                dailyWordStreakStarText.text = "0";
            }

            GlobalStatMap.AddOrModifyText("lastKnownDailyWord", today);
            GlobalStatMap.RemoveFlag("dailyWordPlayedToday");

            enableButton();
        }
    }

    private bool SameDay(DateTime d1, DateTime d2)
    {
        return d1.Day == d2.Day;
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
        menuScript.startDailyWordGame(wordAndDef[0], wordAndDef[1], seedifyTodaysDate(todaysDate.ToString("d")), getDayOfWeek());
    }

    private int getDayOfWeek()
    {
        // Monday = 0...Sunday = 6
        return Math.Abs((int)(todaysDate.DayOfWeek) + 6) % 7;
    }

    // Return DDMMYYYY as an integer with a 1 in front
    private int seedifyTodaysDate(string date)
    {
        return int.Parse("1" + date.Replace("/", ""));
    }
}
