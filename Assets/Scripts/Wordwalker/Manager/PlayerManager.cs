using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera and user input manager.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public static bool greenlight = false;

    private float maxXBound = 1000;
    private float minXBound = -1000;
    private float minZBound = 1000;
    private float maxZBound = -1000;
    private float maxZoom = -30;
    private const float minZoom = 12; // I don't see this changing

    Vector3 pos;
    float sumDistance = 0;
    public GameObject cam;
    bool freeCamera = true;
    bool inViewMode = false;

    public Vector3 startingCamPos;
    public Vector3 walterWhitePos;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Player Manager READY");
        greenlight = true;
    }

    private void OnEnable()
    {
        ModeToolUI.inViewMode += inViewer;
        ModeToolUI.inStepperMode += exitViewer;
        ModeToolUI.inMarkerMode += exitViewer;
    }

    private void OnDisable()
    {
        ModeToolUI.inViewMode -= inViewer;
        ModeToolUI.inStepperMode -= exitViewer;
        ModeToolUI.inMarkerMode -= exitViewer;
    }

    // Update is called once per frame
    void Update()
    {
        if(freeCamera)
        {
            // MOUSE CONTROLS
            if(!(Application.platform == RuntimePlatform.IPhonePlayer) && !(Application.platform == RuntimePlatform.Android))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    pos = Input.mousePosition;
                }

                if (Input.GetMouseButton(0))
                {
                    Vector2 res = (Input.mousePosition - pos);
                    Vector3 transformation = new Vector3(res.y, 0, -res.x) * 0.1f;
                    cam.transform.position = boundCameraPosition(cam.transform.position + transformation);

                    pos = Input.mousePosition;
                }

                //Zoom in
                if (Input.mouseScrollDelta.y == 1)
                {
                    cam.transform.position = boundZoomView(cam.transform.position + new Vector3(0, -1, 0));
                }

                //Zoom out
                if (Input.mouseScrollDelta.y == -1)
                {
                    cam.transform.position = boundZoomView(cam.transform.position + new Vector3(0, 1, 0));
                }
            }


            // TOUCH CONTROLS
            else {
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

                    // TODO - If one of the touch phases ends here, track the other one, and do something
                    // To indicate to below clause that this is the new position that should be tracked
                    // We can't only track the position of touch[0] any longer
                }

                else if (Input.touchCount == 1)
                {
                    Touch touch = Input.touches[0];

                    if (touch.phase == TouchPhase.Moved)
                    {
                        // let's not overcomplicate it
                        Vector2 res = touch.deltaPosition;
                        Vector3 transformation = new Vector3(res.y, 0, -res.x) * 0.0037f;
                        cam.transform.position = boundCameraPosition(cam.transform.position + transformation);
                    }
                }
            }
        }
    }

    public void LerpCameraTo(Vector3 position, float time)
    {
        // In stepper mode we lerp the camera around to follow the player- but not in viewer
        if(!inViewMode)
        {
            position.y = cam.transform.position.y;
            StartCoroutine(lerpCameraCoroutine(cam.transform.position, position, time));
        }
    }

    public void XerpCameraTo(Vector3 position, float time)
    {
        StartCoroutine(xerpCameraCoroutine(cam.transform.position, position, time));
    }

    public void setToStartingPosition()
    {
        teleportCameraTo(startingCamPos);
    }

    public void walterWhitePan()
    {
        XerpCameraTo(walterWhitePos, 1);
    }

    IEnumerator xerpCameraCoroutine(Vector3 start, Vector3 end, float time)
    {
        float steps = 30;

        for (float i = 0; i <= steps; i++)
        {
            cam.transform.position = UIUtils.XerpStandard(start, end, i / steps);
            yield return new WaitForSeconds(1 / steps * time);
        }

        yield return null;
    }

    IEnumerator lerpCameraCoroutine(Vector3 start, Vector3 end, float time)
    {
        float steps = 30;

        for (float i = 0; i <= steps; i++)
        {
            cam.transform.position = Vector3.Lerp(start, end, i / steps);
            yield return new WaitForSeconds(1 / steps * time);
        }

        yield return null;
    }

    public void setFreeCamera(bool freeCam)
    {
        freeCamera = freeCam;
    }

    public void teleportCameraTo(Vector3 position)
    {
        cam.transform.position = position;
    }

    Vector3 boundCameraPosition(Vector3 proposedNew)
    {
        Vector3 trueNew = proposedNew;
        if (proposedNew.x > maxXBound)
        {
            trueNew.x = maxXBound;
        }
        else if (proposedNew.x < minXBound)
        {
            trueNew.x = minXBound;
        }
        if (proposedNew.z > maxZBound)
        {
            trueNew.z = maxZBound;
        }
        else if (proposedNew.z < minZBound)
        {
            trueNew.z = minZBound;
        }

        return trueNew;
    }

    Vector3 boundZoomView(Vector3 proposedNew)
    {
        Vector3 trueNew = proposedNew;
        if (proposedNew.y > maxZoom)
        {
            trueNew.y = maxZoom;
        }
        else if (proposedNew.y < minZoom)
        {
            trueNew.y = minZoom;
        }

        return trueNew;
    }

    public void setBounds(float minXBounds, float maxXBounds, float minZBounds, float maxZBounds, int numRows)
    {
        this.minXBound = minXBounds;
        this.maxXBound = maxXBounds;
        this.minZBound = minZBounds;
        this.maxZBound = maxZBounds;
        this.maxZoom = 3 * numRows + 8;
        walterWhitePos = new Vector3((maxXBounds + minXBounds) / 2, maxZoom, (maxZBounds + minZBounds) / 2);
    }



    /// Exclusively used by actions controlling mode changes
    private void inViewer() { inViewMode = true; }
    private void exitViewer() { inViewMode = false; }
}
