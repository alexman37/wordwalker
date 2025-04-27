using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MenuScript : MonoBehaviour
{
    public static event Action<int, string> newGame;

    public int numLevels = 5;
    public string database;

    public GameObject titleCard;
    public GameObject playButtons;
    public GameObject adventureMenu;

    private float mult = 1;

    // Start is called before the first frame update
    void Start()
    {
        newGame += filler;
    }


    public void toggleAdventureMenu(bool onOrOff)
    {
        adventureMenu.gameObject.SetActive(onOrOff);
    }






    private void updateDatabase(string database)
    {
        this.database = database;
    }

    private void updateMult(float newMult)
    {
        mult = newMult;
    }

    public void startNewGame()
    {
        Debug.Log("Setting up new game");
        GameManagerSc.setParametersOnStart(numLevels, database);
        SceneManager.LoadScene(1);
    }

    private void OnEnable()
    {
        DBClick.dbSelected += updateDatabase;
    }

    private void OnDisable()
    {
        DBClick.dbSelected -= updateDatabase;
    }




    private void filler(int a, string b) { }
}
