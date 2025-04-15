using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all UI in the game- or at least, modifying them and kicking off animations
/// </summary>
public class WordwalkerUIScript : MonoBehaviour
{
    public static bool greenlight = false;

    public GameObject critStats;         // Score, Room # and totems
    public GameObject clueBox;           // The book used for image clues
    public Animator clueBoxAnimator;          // Animation component
    public GameObject inventory;         // Inventory menu
    public GameObject postgame;          // Postgame popup
    public GameObject gameOver;          // Game over popup

    //animating - gotta know the right positions
    private Vector3 postgameAnimationStart;
    private Vector3 postgameAnimationDest;

    // Critical stat fields
    private TextMeshProUGUI displayScore;
    private TextMeshProUGUI displayRoom;
    private TextMeshProUGUI displayTotem;
    private int numLevels;

    //TODO: not in final product
    public GameObject debugRegen;

    // Start is called before the first frame update
    void Start()
    {
        postgameAnimationStart = postgame.transform.localPosition;

        displayScore = critStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        displayRoom = critStats.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        displayTotem = critStats.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();

        GameManagerSc.levelWon += BeginPostgameAnimation;
        GameManagerSc.gameOver += BeginGameOverAnimation;

        greenlight = true;
    }

    public void SetLevelAmount(int amnt)
    {
        displayRoom.text = "0 / " + amnt.ToString();
        numLevels = amnt;
    }

    public void SetNewRoom(int nextLvl)
    {
        displayRoom.text = nextLvl.ToString() + " / " + numLevels;
        if(nextLvl == 10)
        {
            displayRoom.transform.localPosition = displayRoom.transform.localPosition + new Vector3(10, 0, 0);
        }
    }

    public void ChangeScore(int newAmnt, int delta, bool adding)
    {
        displayScore.text = newAmnt.ToString();
    }

    public void ChangeTotems(int newAmnt, int delta, bool adding)
    {
        displayTotem.text = newAmnt.ToString();
    }

    private void BeginPostgameAnimation()
    {
        StartCoroutine(postgameAnimation(1.5f));
    }

    public void ResetPostgamePosition()
    {
        postgame.transform.localPosition = postgameAnimationStart;
        gameOver.transform.localPosition = postgameAnimationStart;

        // TODO not sure what this was used for
        /*Color colS = scrollClue.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(colS.r, colS.g, colS.b, 0);
        Color col = clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);*/
    }

    private void BeginGameOverAnimation()
    {
        StartCoroutine(gameOverAnimation(1.5f));
    }

    public void ResetGameOverPosition()
    {
        gameOver.transform.localPosition = postgameAnimationStart;

        Color col = clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        clueBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);
    }

    IEnumerator postgameAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log(postgameAnimationStart);
        Debug.Log(postgameAnimationDest);
        float frameTime = 30;
        float timeSec = 1f;

        for (float i = 0; i <= frameTime; i++)
        {
            postgame.transform.localPosition = UIUtils.XerpStandard(postgameAnimationStart,
                    postgameAnimationDest,
                    i / frameTime);

            yield return new WaitForSeconds(1 / frameTime * timeSec);
        }
    }

    IEnumerator gameOverAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        float frameTime = 30;
        float timeSec = 1f;

        for (float i = 0; i <= frameTime; i++)
        {
            gameOver.transform.localPosition = UIUtils.XerpStandard(postgameAnimationStart,
                    postgameAnimationDest,
                    i / frameTime);
            
            yield return new WaitForSeconds(1 / frameTime * timeSec);
        }
    }
}
