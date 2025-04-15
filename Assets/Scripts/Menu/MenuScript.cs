using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MenuScript : MonoBehaviour
{
    public static event Action<int, string> newGame;

    public int numLevels = 5;
    public string database = "crossword";

    public GameObject titleCard;
    public GameObject playButtons;

    // Start is called before the first frame update
    void Start()
    {
        newGame += filler;
    }

    public void startNewGame()
    {
        Debug.Log("Setting up new game");
        GameManagerSc.setParametersOnStart(numLevels, database);
        SceneManager.LoadScene(1);
    }




    private void filler(int a, string b) { }
}
