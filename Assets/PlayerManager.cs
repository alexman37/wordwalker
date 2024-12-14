using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    Vector3 pos;
    public GameObject cam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            pos = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector2 res = (Input.mousePosition - pos);
            Vector3 transformation = new Vector3(-res.x, 0, -res.y) * 0.1f;
            cam.transform.position = cam.transform.position + transformation;

            pos = Input.mousePosition;
        }

        //Zoom in
        if(Input.mouseScrollDelta.y == 1)
        {
            cam.transform.position = cam.transform.position + new Vector3(0, -1, 0);
        }

        //Zoom out
        if (Input.mouseScrollDelta.y == -1)
        {
            cam.transform.position = cam.transform.position + new Vector3(0, 1, 0);
        }
    }
}
