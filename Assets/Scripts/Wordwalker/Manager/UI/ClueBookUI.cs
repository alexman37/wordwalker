using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO all of this.
/// Functionally, this should be very similar to how the scroll works- just with different animations
/// </summary>
public class ClueBookUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

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
