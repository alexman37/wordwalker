using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DBClick : MonoBehaviour
{
    private AdventureMenu adventureMenu;
    public DatabaseItem databaseData;
    public static event Action<string> dbSelected;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
