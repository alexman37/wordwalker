using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DatabaseSet : MonoBehaviour
{
    private List<DatabaseItem> databases = new List<DatabaseItem>();
    public bool expanded;
    public Image expandedSprite;
    public GameObject itemsList;
    public RankBox rankBox;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void build()
    {
        RectTransform oldRect = itemsList.GetComponent<RectTransform>();
        Vector2 oldPos = oldRect.anchoredPosition;
        for (int i = 0; i < databases.Count; i++)
        {
            Debug.Log(databases[i].displayName);
            GameObject nextEntry = Instantiate(itemsList.transform.GetChild(0).gameObject);

            nextEntry.transform.SetParent(itemsList.transform);
            nextEntry.GetComponent<RectTransform>().anchoredPosition = new Vector2(oldPos.x, oldPos.y - nextEntry.GetComponent<RectTransform>().rect.height * i);

            // Set image, high score and name of DB in entry
            nextEntry.transform.GetChild(0).GetComponent<Image>().sprite = databases[i].image;
            nextEntry.transform.GetChild(1).GetComponent<Image>().sprite = rankBox.getRankAsSprite(databases[i].highestRank);
            nextEntry.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = databases[i].displayName;

            nextEntry.SetActive(true);
            nextEntry.GetComponent<DBClick>().databaseData = databases[i];
        }
        itemsList.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.rect.width, oldRect.rect.height + 60 * (databases.Count - 1));
    }

    public void AddDatabase(DatabaseItem database)
    {
        databases.Add(database);
    }

    public void displayAllInSet()
    {
        for(int i = 0; i < databases.Count; i++)
        {
            databases[i].actualObject.SetActive(true);
        }
    }

    public void expandPressed()
    {
        expanded = !expanded;
        if (expanded)
        {
            StartCoroutine(rotateExpandedSprite(0));
            itemsList.SetActive(true);
        } else
        {
            StartCoroutine(rotateExpandedSprite(90));
            itemsList.SetActive(false);
        }
    }

    IEnumerator rotateExpandedSprite(float newDeg)
    {
        float steps = 5;
        float timeSec = 0.1f;

        Quaternion old = expandedSprite.rectTransform.rotation;
        for (float i = 0; i <= steps; i++)
        {
            expandedSprite.rectTransform.rotation = Quaternion.Lerp(old, Quaternion.Euler(0, 0, newDeg), i / steps);
            yield return new WaitForSeconds(1 / steps * timeSec);
        }
        expandedSprite.rectTransform.rotation = Quaternion.Euler(0, 0, newDeg);
        yield return null;
    }
}



public class DatabaseItem
{
    public string databaseId; // use this to actually load the database from BundledAssets or whatever
    public string displayName;
    public GameObject actualObject; // only property to be assigned (and unassigned) when expanded/unexpanded
    public Sprite image;
    public string description;
    public int highestRank; // these should be stored...somewhere...else
    public HighScoresList highScores;

    public DatabaseItem(string id, string name, Sprite pic, string desc, HighScore[] scores)
    {
        databaseId = id;
        displayName = name;
        image = pic;
        description = desc;

        highestRank = (scores != null && scores[0] != null) ? scores[0].rank : -1;
        highScores = new HighScoresList(scores);
    }
}

// List of high scores - add a new one (maybe), display them, etc...
public class HighScoresList
{
    public HighScore[] highScores;

    public HighScoresList()
    {
        highScores = new HighScore[5];
    }

    public HighScoresList(HighScore[] highScores)
    {
        this.highScores = highScores;
    }
}

public class HighScore
{
    public int value;
    public int rank;
    public string dateAchieved;

    public HighScore(int value, int rank, string dateAchieved)
    {
        this.value = value;
        this.rank = rank;
        this.dateAchieved = dateAchieved;
    }
}

