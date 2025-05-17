using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChallengeEnabler : MonoBehaviour
{
    public bool enabler;
    public ChallengeClick[] allChallenges;

    public static event Action<bool> enableAllChallenges;

    public void onClick()
    {
        if(enabler)
        {
            enableAllChallenges.Invoke(true);
        }

        else
        {
            enableAllChallenges.Invoke(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        enableAllChallenges += (_) => { };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
