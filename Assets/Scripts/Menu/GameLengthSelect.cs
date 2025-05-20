using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameLengthSelect : MonoBehaviour
{
    public int numLevels;
    public string lengthName;

    public static event Action<int, string> lengthSelected;

    // Start is called before the first frame update
    void Start()
    {
        lengthSelected += (_, __) => { };
    }

    public void selectLength()
    {
        Debug.Log("selected?");
        lengthSelected.Invoke(numLevels, lengthName);
    }
}
