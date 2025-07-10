using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// Scene Transitions.
/// SO why not just call it "TransitionManager"?? mistakes were made...
public class TitleHex : MonoBehaviour
{
    private Image img;

    public int xCoord;
    public int yCoord;

    private static (int, int) maxCoords = (6, 4);
    private static float maxDistance = 2f * Mathf.Sqrt(13);

    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
        if (SceneManager.GetActiveScene().buildIndex == 0) // menu
        {
            img.enabled = MenuScript.transitioning;
            if (transform.childCount > 0)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().enabled = MenuScript.transitioning;
            }
        } else if (SceneManager.GetActiveScene().buildIndex == 1) // wordwalker
        {
            img.enabled = GameManagerSc.transitioning;
            if (transform.childCount > 0)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().enabled = GameManagerSc.transitioning;
            }
        }
    }

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void OnEnable()
    {
        MenuScript.transition += gotoWW;
        GameManagerSc.transition += gotoMenu;
    }

    private void OnDisable()
    {
        MenuScript.transition -= gotoWW;
        GameManagerSc.transition -= gotoMenu;
    }

    void gotoMenu(bool into)
    {
        StartCoroutine(rotation(into, 0));
    }

    void gotoWW(bool into)
    {
        StartCoroutine(rotation(into, 1));
    }

    IEnumerator rotation(bool into, int sceneId)
    {
        float totalGradTime = 0.75f;
        float gradDelay = totalGradTime * (Vector2.Distance(Vector2.zero, new Vector2(xCoord, yCoord)) / maxDistance);

        // set initial rotation
        if (into)
        {
            transform.localRotation = Quaternion.Euler(0, 90, 0);
        } else
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        img.enabled = true;
        if(transform.childCount > 0)
        {
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().enabled = true;
        }
        float timeSec = 0.75f;

        for(float i = -gradDelay; i <= timeSec; i += Time.deltaTime)
        {
            if(i > 0)
            {
                float d = i / timeSec;
                this.transform.localRotation = Quaternion.Euler(0, into ? (90 * (1 - d)) : (90 * d), 0);
            }
            yield return null;
        }

        this.transform.localRotation = Quaternion.Euler(0, into ? 0 : 90, 0);

        if(into)
        {
            // awful way of triggering this. but you know what? it's july 9th, 20 or so days from when i want this thing done.
            // screw you and your "code smell". i dont care to dispatch some stupid action, or have a static variable blah blah WHATEVER.
            // just change the scene once when we're done the animation. that's all.
            if(xCoord == maxCoords.Item1 && yCoord == maxCoords.Item2)
            {
                Debug.Log("Changing to scene " + sceneId);
                SceneManager.LoadScene(sceneId);
            }
        }
        else {
            img.enabled = false;
            if (transform.childCount > 0)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().enabled = false;
            }
        }

        //MenuScript.transitioning = false;

        yield return null;
    }
}
