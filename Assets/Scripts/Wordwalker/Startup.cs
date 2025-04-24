using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Startup : MonoBehaviour
{
    private List<GameObject> thisAndAllChildren;

    // Start is called before the first frame update
    void Start()
    {
        thisAndAllChildren = new List<GameObject>();
        thisAndAllChildren.Add(this.gameObject);

        //TODO we may need to make this recursive
        for(int i = 0; i < transform.childCount; i++)
        {
            thisAndAllChildren.Add(this.transform.GetChild(i).gameObject);
        }

        initializeDatabaseList();


        // When we're done everything, remove the startup "barrier"
        StartCoroutine(fadeOut());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Create all databases
    /// According to some file, bc we need to store scores and which custom ones we're actually using
    /// </summary>
    void initializeDatabaseList()
    {
        
    }

    IEnumerator fadeOut()
    {
        float steps = 5;
        float timeSec = 1f;
        
        for (float i = 0; i <= steps; i++)
        {
            foreach (GameObject obj in thisAndAllChildren)
            {
                Image possibleImg = obj.GetComponent<Image>();
                TextMeshProUGUI possibleText = obj.GetComponent<TextMeshProUGUI>();

                if(possibleImg != null)
                {
                    Color col = possibleImg.color;
                    possibleImg.color = new Color(col.r, col.g, col.b, 1 - i / steps);
                }

                if(possibleText != null)
                {
                    Color col = possibleText.color;
                    possibleText.color = new Color(col.r, col.g, col.b, 1 - i / steps);
                }
                
            }
            yield return new WaitForSeconds(1 / steps * timeSec);
        }
        
        this.gameObject.SetActive(false);
        yield return null;
    }
}
