using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class DBClick : MonoBehaviour
{
    private AdventureMenu adventureMenu;
    public DatabaseItem databaseData;
    public static event Action<string> dbSelected;

    private Image backgroundCol;
    private bool selected;

    public void showData()
    {
        //Debug.Log("you clicked on " + databaseData.displayName);
        dbSelected.Invoke(databaseData.databaseId);
        adventureMenu.displayDatabase(databaseData);
    }

    // Start is called before the first frame update
    void Start()
    {
        dbSelected += (_) => { };
        adventureMenu = FindObjectOfType<AdventureMenu>();
        backgroundCol = this.GetComponent<Image>();
    }

    private void OnEnable()
    {
        dbSelected += StopSelect;
    }

    private void OnDisable()
    {
        dbSelected -= StopSelect;
    }

    // Currently unused...
    public void OnHighlight()
    {
        if (!selected)
            backgroundCol.color = new Color(40f / 255f, 40f / 255f, 40f / 255f);
    }

    public void OnStopHighlight()
    {
        if(!selected)
            backgroundCol.color = new Color(0, 0, 0);
    }

    private void OnSelectMe()
    {
        selected = true;
        backgroundCol.color = new Color(135f / 255f, 146f / 255f, 171f / 255f);
    }

    private void StopSelect(string dbName)
    {
        if(databaseData.databaseId == dbName)
        {
            OnSelectMe();
        } else
        {
            // Don't waste time recoloring every DB
            if (selected)
            {
                backgroundCol.color = new Color(0, 0, 0);
                selected = false;
            }
        }
        
    }
}
