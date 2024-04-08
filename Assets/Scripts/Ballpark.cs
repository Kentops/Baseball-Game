using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballpark : MonoBehaviour
{
    public Transform leftMark;
    public Transform centerMark;
    public Transform rightMark;
    public GameObject currentBall;
    public Camera[] fieldCameras; //0 - Catcher, 1 - Ball

    private Transform ballCam;

    public delegate void FieldEvent();
    public static FieldEvent ballHit;
    public static FieldEvent deadBall;

    // Start is called before the first frame update
    void Start()
    {
        ballCam = fieldCameras[1].transform;
        ballHit += swapToBallCam;
        deadBall += swapToCatcherCam;
    }

    // Update is called once per frame
    void Update()
    {
        //Move Ball Camera
        if(currentBall != null)
        {
            //The plus and minus are to center it. The height is for the players
            ballCam.position = new Vector3(currentBall.transform.position.x - 34,
                ballCam.position.y, currentBall.transform.position.z + 6);
        }
    }

    public void swapToBallCam()
    {
        fieldCameras[1].targetDisplay = 0; //Change first so no black screen
        fieldCameras[0].targetDisplay = 1; //Displayes are indexed, display-1 = 0
    }

    public void swapToCatcherCam()
    {
        fieldCameras[0].targetDisplay = 0;
        fieldCameras[1].targetDisplay = 1;
    }

    public void removeTheBall() //Makes sure the field knows what's up
    {
        if (fieldCameras[0].targetDisplay != 0)
        {
            swapToCatcherCam();
        }
        Destroy(currentBall);
        currentBall = null;
    }
}
