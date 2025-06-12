using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DatabaseSet : MonoBehaviour
{
    private List<DatabaseItem> databases = new List<DatabaseItem>();
    public string dbName;
    public bool expanded;

    private const int DB_OFFSET = 5;

    public Image expandedSprite;
    public GameObject itemsList;
    public RankBox rankBox;
    public TextMeshProUGUI dbNameField;

    private float heightOfEntries;
    private int slot;

    public static event Action<int, float, bool> usedCollapser;

    // Start is called before the first frame update
    void Start()
    {
        heightOfEntries = itemsList.transform.GetChild(0).GetComponent<RectTransform>().rect.height;
    }

    private void OnEnable()
    {
        usedCollapser += moveElementsBelow;
    }

    private void OnDisable()
    {
        usedCollapser -= moveElementsBelow;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void build(int slot)
    {
        heightOfEntries = itemsList.transform.GetChild(0).GetComponent<RectTransform>().rect.height;

        RectTransform oldRect = itemsList.GetComponent<RectTransform>();
        Vector2 oldPos = oldRect.anchoredPosition;

        for (int i = 0; i < databases.Count; i++)
        {
            Debug.Log(databases[i].displayName);
            GameObject nextEntry = Instantiate(itemsList.transform.GetChild(0).gameObject);

            nextEntry.transform.SetParent(itemsList.transform);
            nextEntry.GetComponent<RectTransform>().anchoredPosition = new Vector2(oldPos.x, oldPos.y - heightOfEntries * i);

            // Set image, high score and name of DB in entry
            nextEntry.transform.GetChild(0).GetComponent<Image>().sprite = databases[i].image;
            nextEntry.transform.GetChild(1).GetComponent<Image>().sprite = rankBox.getRankAsSprite(databases[i].highestRank);
            nextEntry.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = databases[i].displayName;

            nextEntry.SetActive(true);
            nextEntry.GetComponent<DBClick>().databaseData = databases[i];
        }

        // Size of container for this particular set
        itemsList.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.rect.width, oldRect.rect.height + heightOfEntries * (databases.Count - 1));

        // Size of scroll window - just increase it by the height of this new "top tab" element
        RectTransform broadScroll = transform.parent.GetComponent<RectTransform>();

        float heightOfTopTab = this.GetComponent<RectTransform>().rect.height;
        broadScroll.sizeDelta = new Vector2(broadScroll.rect.width, broadScroll.rect.height + heightOfTopTab);

        dbNameField.text = dbName;

        // Now we have to move this to the right Y position according to its slot
        RectTransform currentPos = this.GetComponent<RectTransform>();
        currentPos.anchoredPosition = new Vector2(currentPos.anchoredPosition.x, currentPos.anchoredPosition.y - heightOfEntries * slot);
        this.slot = slot;

        // Sometimes it'll be expanded on startup
        // TODO - not moving down others on startup, probably because others don't exist yet...
        /*if (expanded)
        {
            expandedSprite.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
            itemsList.SetActive(true);

            broadScroll.sizeDelta = new Vector2(broadScroll.rect.width, broadScroll.rect.height + heightOfEntries * databases.Count);

            // Position of future elements modified
            usedCollapser.Invoke(slot, heightOfEntries * (databases.Count) + DB_OFFSET, false);
        }*/
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

            // Size of 'broadScroller' - affects scrolling for all elements.
            RectTransform broadScroll = transform.parent.GetComponent<RectTransform>();

            broadScroll.sizeDelta = new Vector2(broadScroll.rect.width, broadScroll.rect.height + heightOfEntries * databases.Count);

            // Position of future elements modified
            usedCollapser.Invoke(slot, heightOfEntries * (databases.Count) + DB_OFFSET, false);
        } else
        {
            StartCoroutine(rotateExpandedSprite(90));
            itemsList.SetActive(false);

            // Size of 'broadScroller' - affects scrolling for all elements.
            RectTransform broadScroll = transform.parent.GetComponent<RectTransform>();

            broadScroll.sizeDelta = new Vector2(broadScroll.rect.width, broadScroll.rect.height - heightOfEntries * databases.Count);

            // Position of future elements modified
            usedCollapser.Invoke(slot, heightOfEntries * (databases.Count) + DB_OFFSET, true);
        }
    }

    private void moveElementsBelow(int slot, float amount, bool up)
    {
        if(slot < this.slot)
        {
            RectTransform currentPos = this.GetComponent<RectTransform>();
            currentPos.anchoredPosition = new Vector2(currentPos.anchoredPosition.x, currentPos.anchoredPosition.y + amount * (up ? 1 : -1));
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

    // Used on initial load
    public DatabaseItem getFirst()
    {
        return databases[0];
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

    public bool imageDB;
    public int size;  // how many words are in this list
    public HashSet<WordGen.Word> wordsDiscovered;

    public DatabaseItem(string id, string name, Sprite pic, string desc, HighScore[] scores, int sizeOf, bool imageDB)
    {
        databaseId = id;
        displayName = name;
        image = pic;
        description = desc;

        highestRank = (scores != null && scores[0] != null) ? scores[0].rank : -1;
        highScores = new HighScoresList(scores);

        size = sizeOf;
        wordsDiscovered = new HashSet<WordGen.Word>();
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
        sortHighScores();
    }

    // Sort scores in ascending order of value
    public void sortHighScores()
    {
        HighScore[] sorted = new HighScore[5];

        bool putInPlace = false;
        HighScore temp = null;

        if(highScores != null)
        {
            for (int i = 0; i < highScores.Length; i++)
            {
                if(highScores[i] != null)
                {
                    putInPlace = false;
                    for (int j = 0; j < 5; j++)
                    {
                        if (sorted[j] == null || sorted[j].value <= highScores[i].value)
                        {
                            temp = sorted[j];
                            sorted[j] = highScores[i];
                            putInPlace = true;
                        }
                        else if (putInPlace)
                        {
                            HighScore whatever = sorted[j];
                            sorted[j] = temp;
                            temp = whatever;
                        }
                    }
                }
            }
        } /*else
        {
            highScores = new HighScore[5];
        }*/
    }

    // Add (or attempt to add) a new high score...if it doesn't make the list then return false
    public bool addNewHighScore(HighScore hs)
    {
        if(highScores == null)
        {
            highScores = new HighScore[5];
            highScores[0] = hs;
            return true;
        }


        for (int i = 0; i < highScores.Length; i++)
        {
            // If there's still not 5 high scores, add this and sort immediately- easy peasy
            if (highScores[i] == null)
            {
                highScores[i] = hs;
                sortHighScores();
                return true;
            }
            // If the score numerically beats anything on the list, immediately replace the lowest score then sort
            else if (hs.value > highScores[i].value)
            {
                highScores[4] = hs;
                sortHighScores();
                return true;
            }
        }

        return false;
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

