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

    public void displayDatabase(DatabaseItem item)
    {
        databaseDescription.transform.GetChild(0).GetComponent<Image>().sprite = item.image;
        databaseDescription.transform.GetChild(1).GetComponent<Image>().sprite = rankBox.getRankAsSprite(item.highestRank);
        databaseDescription.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = item.displayName;
        databaseDescription.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = item.description;
        //databaseDescription.transform.GetChild(4).GetComponent<HighScore>().text = item.description;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}