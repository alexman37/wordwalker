using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Utilities;

public class Startup : MonoBehaviour
{
    private List<GameObject> thisAndAllChildren;

    // Start is called before the first frame update
    void Start()
    {
        // Gonna try putting this here and see what happens...?
        AotHelper.EnsureList<int>();
        AotHelper.EnsureType<HashSet<WordGen.Word>>();
        AotHelper.EnsureType<HighScoresList>();

        thisAndAllChildren = new List<GameObject>();
        thisAndAllChildren.Add(this.gameObject);

        //TODO we may need to make this recursive
        for(int i = 0; i < transform.childCount; i++)
        {
            thisAndAllChildren.Add(this.transform.GetChild(i).gameObject);
        }


        // When we're done everything, remove the startup "barrier"
        StartCoroutine(fadeOut());
    }

    IEnumerator fadeOut()
    {
        float timeSec = 1f;
        
        for (float i = 0; i <= timeSec; i += Time.deltaTime)
        {
            foreach (GameObject obj in thisAndAllChildren)
            {
                Image possibleImg = obj.GetComponent<Image>();
                TextMeshProUGUI possibleText = obj.GetComponent<TextMeshProUGUI>();

                if(possibleImg != null)
                {
                    Color col = possibleImg.color;
                    possibleImg.color = new Color(col.r, col.g, col.b, 1 - Mathf.Clamp(i / timeSec, 0, 1));
                }

                if(possibleText != null)
                {
                    Color col = possibleText.color;
                    possibleText.color = new Color(col.r, col.g, col.b, 1 - Mathf.Clamp(i / timeSec, 0, 1));
                }
                
            }
            yield return null;
        }
        
        this.gameObject.SetActive(false);
        yield return null;
    }
}
