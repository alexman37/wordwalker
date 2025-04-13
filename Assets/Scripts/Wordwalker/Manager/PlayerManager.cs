using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static bool greenlight = false;

    private float maxXBound = 1000;
    private float minXBound = -1000;
    private float minZBound = 1000;
    private float maxZBound = -1000;

    Vector3 pos;
    float sumDistance = 0;
    public GameObject cam;

    // Start is called before the first frame update
    void Start()
    {
        greenlight = true;
    }

    // Update is called once per frame
    void Update()
    {
        // MOUSE CONTROLS
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 res = (Input.mousePosition - pos);
            Vector3 transformation = new Vector3(res.y, 0, -res.x) * 0.1f;
            cam.transform.position = updateCameraPosition(cam.transform.position + transformation);

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


        // TOUCH CONTROLS
        if (Input.touchCount == 2)
        {
            Touch first = Input.touches[0];
            Touch second = Input.touches[1];

            if (second.phase == TouchPhase.Began)
            {
                sumDistance = Vector2.Distance(first.position, second.position);
            }

            if (first.phase == TouchPhase.Moved || second.phase == TouchPhase.Moved)
            {
                float deltaDistance = sumDistance - Vector2.Distance(first.position, second.position);

                // If they're getting closer, zoom out
                if (deltaDistance < 0)
                {
                    cam.transform.position = cam.transform.position - new Vector3(0, 0.4f, 0);
                }


                // If they're getting further, zoom in
                if (deltaDistance > 0)
                {
                    cam.transform.position = cam.transform.position + new Vector3(0, 0.4f, 0);
                }

                sumDistance = Vector2.Distance(first.position, second.position);
            }
        }

        else if (Input.touchCount == 1)
        {
            Touch touch = Input.touches[0];

            if (touch.phase == TouchPhase.Moved)
            {
                // let's not overcomplicate it
                Vector2 res = touch.deltaPosition;
                Vector3 transformation = new Vector3(res.y, 0, -res.x) * 0.005f;
                cam.transform.position = updateCameraPosition(cam.transform.position + transformation);

                pos = Input.mousePosition;
            }
        }
    }

    Vector3 updateCameraPosition(Vector3 proposedNew)
    {
        Vector3 trueNew = proposedNew;
        if(proposedNew.x > maxXBound)
        {
            trueNew.x = maxXBound;
        } else if(proposedNew.x < minXBound)
        {
            trueNew.x = minXBound;
        } if(proposedNew.z > maxZBound)
        {
            trueNew.z = maxZBound;
        } else if(proposedNew.z < minZBound)
        {
            trueNew.z = minZBound;
        }

        return trueNew;
    }

    public void setBounds(float minXBounds, float maxXBounds, float minZBounds, float maxZBounds)
    {
        this.minXBound = minXBounds;
        this.maxXBound = maxXBounds;
        this.minZBound = minZBounds;
        this.maxZBound = maxZBounds;
    }
}
