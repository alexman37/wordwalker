using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// TODO all of this.
/// Functionally, this should be very similar to how the scroll works- just with different animations
/// </summary>
public class ClueBookUI : MonoBehaviour
{
    public Image clueBookPicture;
    public Image imageInWaiting; // we will put this image on the "next" page of the book.
    public TextMeshProUGUI caption;
    public Animator clueBoxAnimator;      // Animation component

    public float maxWidth;
    public float maxHeight;

    private string currImageAssetBundlePath;

    [SerializeField] private AudioClip pageTurnClip;

    // Start is called before the first frame update
    void Start()
    {
        // Whenever scaling is done, determine max width and max height we'll be working with.
        RectTransform rectTransform = clueBookPicture.GetComponent<RectTransform>();
        ScalingUIComponent scalingComp = GetComponent<ScalingUIComponent>();
        GetComponent<ScalingUIComponent>().completedScaling += () =>
        {
            maxWidth = rectTransform.rect.width;
            maxHeight = rectTransform.rect.height;
        };
        if (scalingComp.DONE)
        {
            maxWidth = rectTransform.rect.width;
            maxHeight = rectTransform.rect.height;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        // When the character plays the animation to open their scroll, start same animation here
        // Win or lose, move the scroll out of view afterwards
        AnimationManager.openedScroll += turnClueBookPage;
        //GameManagerSc.levelWon += moveScrollOnFinish;
        //GameManagerSc.gameOver += moveScrollOnFinish;
    }

    private void OnDisable()
    {
        // When the character plays the animation to open their scroll, start same animation here
        // Win or lose, move the scroll out of view afterwards
        AnimationManager.openedScroll -= turnClueBookPage;
        //GameManagerSc.levelWon -= moveScrollOnFinish;
        //GameManagerSc.gameOver -= moveScrollOnFinish;
    }

    public void setImageAssetBundlePath(string path)
    {
        currImageAssetBundlePath = path;
    }

    public void setPage(string imageName)
    {
        StartCoroutine(WordGen.LoadImageAsset("imagedbs/" + currImageAssetBundlePath, imageName, imageInWaiting, (maxWidth, maxHeight)));
    }

    private void turnClueBookPage()
    {
        SfxManager.instance.playSFX(pageTurnClip, null, 1f);
        clueBoxAnimator.SetTrigger("gotoNextPage");
        clueBookPicture.rectTransform.sizeDelta = imageInWaiting.rectTransform.sizeDelta;
        clueBookPicture.sprite = imageInWaiting.sprite;
    }

    // TODO not sure what this was used for
    /*Color colS = scrollClue.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
    clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(colS.r, colS.g, colS.b, 0);
    Color col = clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
    clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);*/

    //clueBoxAnimator.SetTrigger("gotoNextPage");

    //Reset position:
    //Color col = clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
    //clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);
}
