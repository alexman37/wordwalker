using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialPopup : WidgetPopup
{
    public Sprite[] pageImgs;
    private string[] headings = new string[] { "BASICS", "CONTROLS", "THE SIDEBAR", "MODE SELECT", "TOTEMS", "ITEMS", "SCORE & RANK", "CHALLENGES" };
    public Image content;
    int currPage = 0;
    const int totalNumPages = 8;
    public Image backButton;
    public Image nextButton;
    public TextMeshProUGUI title;
    public TextMeshProUGUI pageNumSub;

    public Sprite mobilePage2;

    // Start is called before the first frame update
    void Start()
    {
        this.Setup();


    }

    public void goToNextPage()
    {
        if(currPage < totalNumPages - 1)
        {
            currPage = currPage + 1;
            if(currPage == totalNumPages - 1)
            {
                // darken next button if nowhere else to go
                nextButton.color = new Color(0.5f, 0.5f, 0.5f, 1);
            }
            backButton.color = new Color(1, 1, 1, 1);

            changePage(currPage);
        }
    }

    public void goBackPage()
    {
        if (currPage > 0)
        {
            currPage = currPage - 1;
            if (currPage == 0)
            {
                // darken next button if nowhere else to go
                backButton.color = new Color(0.5f, 0.5f, 0.5f, 1);
            }
            nextButton.color = new Color(1, 1, 1, 1);

            changePage(currPage);
        }
    }

    public void changePage(int page)
    {
        content.sprite = pageImgs[page];

        // The "controls" page will vary depending on what platform you're playing on
        if(page == 1)
        {
            switch(Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.Android:
                    content.sprite = mobilePage2;
                    break;
                default: break;
            }
        }

        currPage = page;

        title.text = headings[page];
        pageNumSub.text = (page + 1) + " of " + totalNumPages;
    }
}
