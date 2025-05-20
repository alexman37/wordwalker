using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLengthMod : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI lengthText;
    IEnumerator movingCoroutineIn;
    IEnumerator movingCoroutineOut;

    // Start is called before the first frame update
    void Start()
    {
        movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, 0));
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, -Screen.safeArea.height));
    }

    private void OnEnable()
    {
        GameLengthSelect.lengthSelected += changeLengthSelect;
    }

    private void OnDisable()
    {
        GameLengthSelect.lengthSelected -= changeLengthSelect;
    }

    void changeLengthSelect(int numLevels, string lengthName)
    {
        closeLengthMod();
        lengthText.text = "Length: " + lengthName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openLengthMod()
    {
        Debug.Log("open length");
        StopCoroutine(movingCoroutineOut);
        movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, 0));
        StartCoroutine(movingCoroutineIn);
    }

    public void closeLengthMod()
    {
        StopCoroutine(movingCoroutineIn);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, new Vector2(0, -Screen.safeArea.height));
        StartCoroutine(movingCoroutineOut);
    }
}
