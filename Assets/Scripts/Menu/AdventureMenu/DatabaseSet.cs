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

    IEnumerator waitingForResize;

    public static event Action<int, float, bool> usedCollapser;

    // Start is called before the first frame update
    void Start()
    {
        heightOfEntries = itemsList.transform.GetChild(0).GetComponent<RectTransform>().rect.height;
    }

    private void OnEnable()
    {
        usedCollapser += moveElementsBelow;
        SettingsMenu.toggledScreenSize += recalculateEntryHeight;
    }

    private void OnDisable()
    {
        usedCollapser -= moveElementsBelow;
        SettingsMenu.toggledScreenSize -= recalculateEntryHeight;
    }

    // I don't like time waits either but this is one situation where it really feels warranted
    // Cannot figure out how to effectively tell when screen size has changed without it being...sooooo annoying
    void recalculateEntryHeight(ScreenSizeSetting _)
    {
        if (waitingForResize != null) StopCoroutine(waitingForResize);
        waitingForResize = setSizeAfterSomeTime(1.5f);
        StartCoroutine(waitingForResize);
    }

    IEnumerator setSizeAfterSomeTime(float sec)
    {
        yield return new WaitForSeconds(sec);
        heightOfEntries = itemsList.transform.GetChild(0).GetComponent<RectTransform>().rect.height;
    }

    // Actually draws the database object onto the screen
    // If the sprite is expanded, return the additional amount to move down the next database set by
    public float build(int slot, float moveDownby)
    {
        // Add selected tracker for this database if not exists yet. Otherwise, get it from storage.
        if (!GlobalStatMap.AddNewBool("selectedSet_" + dbName, false))
        {
            expanded = DatabaseParser.lastLoadedGlobalStatsMap.boolMap["selectedSet_" + dbName];
        }

        heightOfEntries = itemsList.transform.GetChild(0).GetComponent<RectTransform>().rect.height;

        RectTransform oldRect = itemsList.GetComponent<RectTransform>();
        Vector2 oldPos = oldRect.anchoredPosition;

        for (int i = 0; i < databases.Count; i++)
        {
            DatabasePersistentStats persistent = DatabaseTracker.loadDatabaseTracker(databases[i].databaseId);
            GameObject nextEntry = Instantiate(itemsList.transform.GetChild(0).gameObject);

            nextEntry.transform.SetParent(itemsList.transform);
            nextEntry.GetComponent<RectTransform>().anchoredPosition = new Vector2(oldPos.x, oldPos.y - heightOfEntries * i);

            // Set image, high score and name of DB in entry
            //Debug.Log("First time draw of " + databases[i].databaseId + ": " + databases[i].loadedIcon.name);
            databases[i].inLineImage = nextEntry.transform.GetChild(0).GetComponent<Image>();
            nextEntry.transform.GetChild(0).GetComponent<Image>().sprite = databases[i].loadedIcon;
            nextEntry.transform.GetChild(1).GetComponent<Image>().sprite = rankBox.getRankAsSprite(persistent.highScores.highestRank);
            nextEntry.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = databases[i].displayName;

            nextEntry.SetActive(true);
            nextEntry.GetComponent<DBClick>().databaseData = databases[i];
        }

        // Size of container for this particular set
        itemsList.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.rect.width, oldRect.rect.height + heightOfEntries * (databases.Count - 1) + DB_OFFSET);

        // Size of scroll window - just increase it by the height of this new "top tab" element
        RectTransform broadScroll = transform.parent.GetComponent<RectTransform>();

        float heightOfTopTab = this.GetComponent<RectTransform>().rect.height;
        float estimatedHeight = broadScroll.rect.height + heightOfTopTab + DB_OFFSET;
        broadScroll.sizeDelta = new Vector2(broadScroll.rect.width, estimatedHeight);

        dbNameField.text = dbName;

        // Now we have to move this to the right Y position according to its slot
        RectTransform currentPos = this.GetComponent<RectTransform>();
        currentPos.anchoredPosition = new Vector2(currentPos.anchoredPosition.x, currentPos.anchoredPosition.y - moveDownby - heightOfEntries * slot);
        this.slot = slot;

        // Sometimes it'll be expanded on startup
        // TODO - not moving down others on startup, probably because others don't exist yet...
        if (expanded)
        {
            expandedSprite.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
            itemsList.SetActive(true);

            broadScroll.sizeDelta = new Vector2(broadScroll.rect.width, estimatedHeight + heightOfEntries * databases.Count + DB_OFFSET);

            // Position of future elements modified
            usedCollapser.Invoke(slot, heightOfEntries * (databases.Count) + DB_OFFSET, false);
            return heightOfEntries * databases.Count;
        } else
        {
            return 0;
        }

        
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
            SfxManager.instance.playSFXbyName("click-short", null, 1f);

            StartCoroutine(rotateExpandedSprite(0));
            itemsList.SetActive(true);

            // Size of 'broadScroller' - affects scrolling for all elements.
            RectTransform broadScroll = transform.parent.GetComponent<RectTransform>();

            broadScroll.sizeDelta = new Vector2(broadScroll.rect.width, broadScroll.rect.height + heightOfEntries * databases.Count + DB_OFFSET);

            GlobalStatMap.AddOrModifyBool("selectedSet_" + dbName, true);

            // Position of future elements modified
            usedCollapser.Invoke(slot, heightOfEntries * (databases.Count) + DB_OFFSET, false);
        } else
        {
            SfxManager.instance.playSFXbyName("click-short", null, 1f);

            StartCoroutine(rotateExpandedSprite(90));
            itemsList.SetActive(false);

            // Size of 'broadScroller' - affects scrolling for all elements.
            RectTransform broadScroll = transform.parent.GetComponent<RectTransform>();

            broadScroll.sizeDelta = new Vector2(broadScroll.rect.width, broadScroll.rect.height - heightOfEntries * databases.Count - DB_OFFSET);

            GlobalStatMap.AddOrModifyBool("selectedSet_" + dbName, false);

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
        float timeSec = 0.1f;

        Quaternion old = expandedSprite.rectTransform.rotation;
        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            expandedSprite.rectTransform.rotation = Quaternion.Lerp(old, Quaternion.Euler(0, 0, newDeg), Mathf.Clamp(i / timeSec, 0, 1));
            yield return null;
        }
        expandedSprite.rectTransform.rotation = Quaternion.Euler(0, 0, newDeg);
        yield return null;
    }
}



public class DatabaseItem
{
    public Image inLineImage; // used in actual display

    public string databaseId; // use this to actually load the database from BundledAssets or whatever
    public string group; // what group this database will be a part of
    public string displayName;
    public GameObject actualObject; // only property to be assigned (and unassigned) when expanded/unexpanded
    public string iconPath; // load icon from this path.
    public Sprite loadedIcon;
    public string description;
    public int maxBacktracks;

    public string imageDB;
    public int size;  // how many words are in this list
    public HashSet<WordGen.Word> wordsDiscovered;

    public DatabaseItem(string g, string id, string name, string pic, string desc, int maxBack, int sizeOf, string imagePath)
    {
        databaseId = id;
        group = g;
        displayName = name;
        iconPath = pic;
        description = desc;
        maxBacktracks = maxBack;
        imageDB = imagePath;

        size = sizeOf;
        wordsDiscovered = new HashSet<WordGen.Word>();
    }

    public void RequestRedraw(Sprite icon)
    {
        loadedIcon = icon;
        if(inLineImage != null)
        {
            inLineImage.sprite = loadedIcon;
        }
    }
}

