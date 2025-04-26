using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DatabaseSet : MonoBehaviour
{
    private List<DatabaseItem> databases = new List<DatabaseItem>();
    public bool expanded;
    public Image expandedSprite;
    public GameObject itemsList;

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
        Debug.Log("Done");
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

