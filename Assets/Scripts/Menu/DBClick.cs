using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBClick : MonoBehaviour
{
    private AdventureMenu adventureMenu;
    public DatabaseItem databaseData;

    public void showData()
    {
        //Debug.Log("you clicked on " + databaseData.displayName);
        adventureMenu.displayDatabase(databaseData);
    }

    // Start is called before the first frame update
    void Start()
    {
        adventureMenu = FindObjectOfType<AdventureMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
