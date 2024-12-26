using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TopBarUI : MonoBehaviour
{
    //slide down the postgame animation
    public static Action readyForPostgameAnimation;

    public GameObject baseTileVis;
    private List<GameObject> currProgressVis;
    private List<GameObject> answerVis;

    //rotation
    private float totalRotationTime = 1f;
    private float aggregateRotationTime = 0f;
    private bool rotating = false;

    private Color correct = new Color(65f / 255f, 133f / 255f, 65f / 255f);
    private Color golden = new Color(150f / 255f, 153f / 255f, 63f / 255f);
    private Color red = new Color(153f / 255f, 72f / 255f, 63f / 255f);
    private Color gray = new Color(100f / 255f, 100f / 255f, 100f / 255f);

    // Start is called before the first frame update
    void Start()
    {
        readyForPostgameAnimation += () => { };

        baseTileVis = this.transform.GetChild(0).GetChild(0).gameObject;
        currProgressVis = new List<GameObject>();
        answerVis = new List<GameObject>();
    }

    private void Update()
    {
        if (rotating)
        {
            aggregateRotationTime += Time.deltaTime;
            if (aggregateRotationTime >= totalRotationTime)
            {
                rotating = false;
            }

            // Set rotation
            this.transform.rotation = Quaternion.Lerp(
                Quaternion.Euler(0, 0, 0),
                Quaternion.Euler(-90, 0, 0),
                aggregateRotationTime / totalRotationTime
            );
        }
        
    }

    public void AddLetterToProgress(char ch, Color32 color)
    {
        GameObject next = Instantiate(baseTileVis);
        next.SetActive(true);
        TextMeshProUGUI comp = next.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        comp.text = ch.ToString();
        next.GetComponent<Image>().color = color;

        next.transform.SetParent(baseTileVis.transform.parent);
        currProgressVis.Add(next);
        Readjust();
    }

    void Readjust()
    {
        for(int i = 0; i < currProgressVis.Count; i++)
        {
            GameObject tile = currProgressVis[i];
            float x = (-30 * (currProgressVis.Count - 1)) + (60 * i);
            tile.GetComponent<RectTransform>().localPosition = new Vector3(x, -30, 0);
        }
    }

    public void ResetBar()
    {
        foreach(GameObject tile in currProgressVis)
        {
            Destroy(tile);
        }
        currProgressVis.Clear();

        foreach (GameObject tile in answerVis)
        {
            Destroy(tile);
        }
        answerVis.Clear();

        aggregateRotationTime = 0f;
        this.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void SetAnswer(List<Tile> corrects, bool won)
    {
        int addCoins = 0;
        int addTotems = 0;

        foreach(Tile til in corrects)
        {
            GameObject next = Instantiate(baseTileVis);
            next.SetActive(true);

            TextMeshProUGUI comp = next.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            comp.text = til.letter.ToString();
            Color col = determineAnswerColor(til, won);
            next.GetComponent<Image>().color = col;

            //prepare to add totems
            if (won)
            {
                addCoins += 1;
                if (col == golden) addTotems += 1;
            }

            next.transform.SetParent(this.transform.GetChild(1));
            next.transform.rotation = Quaternion.Euler(90, 0, 0);
            answerVis.Add(next);
        }

        for (int i = 0; i < answerVis.Count; i++)
        {
            GameObject tile = answerVis[i];
            float x = (-30 * (answerVis.Count - 1)) + (60 * i);
            tile.GetComponent<RectTransform>().localPosition = new Vector3(x, -30, 0);
        }

        GameManagerSc.changeCoins(addCoins, true);
        GameManagerSc.changeTotems(addTotems, true);
    }

    public void kickOffRotation()
    {
        rotating = true;
        readyForPostgameAnimation.Invoke();
    }

    private Color determineAnswerColor(Tile til, bool won)
    {
        if (til.stepped)
        {
            return this.correct;
        }
        else
        {
            if (won)
            {
                //Hey, you found a totem, congratulations
                return golden;
            }
            else
            {
                return gray;
            }
        }
    }
}
