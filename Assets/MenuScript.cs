using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MenuScript : MonoBehaviour
{
    public static event Action<int, WordGen.WordDB> newGame;

    public int numLevels = 2;
    public WordGen.WordDB database = WordGen.WordDB.CROSSWORD;

    public GameObject titleCard;
    public GameObject playButtons;

    private Rect safeArea;

    // Start is called before the first frame update
    void Start()
    {
        newGame += filler;

        // Configure UI to the phone's settings
        safeArea = Screen.safeArea;

        //setLocAndScale(titleCard, new Vector2(0.15f, 0.15f));
        //setLocAndScale(playButtons, new Vector2(0.15f, 0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startNewGame()
    {
        Debug.Log("Setting up new game");
        GameManagerSc.setParametersOnStart(numLevels, database);
        SceneManager.LoadScene(1);
    }

    private void setScale(GameObject container, Vector3 fill)
    {
        container.transform.position = new Vector3();
    }

    //offset and fill measured in percent of total screen safe area size
    //TODO anything with scale?
    private void setLocAndScale(GameObject container, Vector2 offset)
    {
        // Assuming you don't use this foolishly
        float deltaX = safeArea.xMax - safeArea.xMin;
        float deltaY = safeArea.yMax - safeArea.yMin;

        container.transform.position = new Vector2(
            safeArea.xMin + (deltaX * offset.x),
            safeArea.yMin + (deltaY * offset.y)
        );

        //container.transform.localScale =
        
    }




    private void filler(int a, WordGen.WordDB b) { }
}
