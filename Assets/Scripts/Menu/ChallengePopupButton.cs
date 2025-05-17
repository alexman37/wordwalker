using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengePopupButton : MonoBehaviour
{
    public ChallengePopup challengePopup;

    public Image img;
    public Sprite[] backgrounds;

    public void showPopup()
    {
        challengePopup.openPopup();
    }

    private void changeBackground(int i)
    {
        img.sprite = backgrounds[i];
    }

    private void OnEnable()
    {
        ChallengePopup.closedPopup += changeBackground;
    }

    private void OnDisable()
    {
        ChallengePopup.closedPopup -= changeBackground;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
