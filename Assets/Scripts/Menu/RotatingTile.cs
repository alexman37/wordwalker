using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RotatingTile : MonoBehaviour
{
    private static float rotateZ = 0;
    private static float timeElapsed = 0;
    private static float timeToTake = 12f;
    private RectTransform rt;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        timeElapsed = timeElapsed % (Mathf.PI * 2f * 12f);
        rotateZ = Mathf.Sin(timeElapsed / timeToTake) * 10f;
        rt.rotation = Quaternion.Euler(new Vector3(0, 0, rotateZ));
    }
}
