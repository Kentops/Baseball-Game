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
    public GameObject ballTarget;
    public float gravityMultiplier;

    private Transform ballCam;
    private bool ballCamCanMove = true;
    private bool landingPadPlaced = false;

    public delegate void FieldEvent();
    public static FieldEvent ballHit;
    public static FieldEvent fairBall;
    public static FieldEvent deadBall;

    // Start is called before the first frame update
    void Start()
    {
        ballCam = fieldCameras[1].transform;
        ballHit += swapToBallCam;
        ballHit += setBallLanding;
        fairBall += OnFairBall;
        deadBall += swapToCatcherCam;
    }

    // Update is called once per frame
    void Update()
    {
        //Move Ball Camera
        if(currentBall != null && ballCamCanMove)
        {
            //The plus and minus are to center it. The height is for the players
            ballCam.position = new Vector3(currentBall.transform.position.x - 34, currentBall.transform.position.y + 55, currentBall.transform.position.z + 6);
          

            //Restrict Ball Camera
            Vector3 cbPos = currentBall.transform.position;
            if (currentBall != null && (cbPos.x > 230 || cbPos.z > 215 || cbPos.z < -250)) //Change for each stadium
            {
                ballCamCanMove = false;
                ballTarget.GetComponent<MeshRenderer>().enabled = false;
            }

            if(!landingPadPlaced && cbPos.y > 75)
            {
                ballTarget.GetComponent<MeshRenderer>().enabled = true;
                landingPadPlaced = true;
            }
        }
        

    }

    public void swapToBallCam()
    {
        ballCamCanMove = true;
        landingPadPlaced = false;

        fieldCameras[1].targetDisplay = 0; //Change first so no black screen
        fieldCameras[0].targetDisplay = 1; //Displayes are indexed, display-1 = 0
    }

    public void swapToCatcherCam()
    {
        ballTarget.GetComponent<MeshRenderer>().enabled = false;
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

    private void setBallLanding()
    {
        //Sets the visual for where the ball will land
        Rigidbody ballRB = currentBall.GetComponent<Rigidbody>();
        float airTime = (2 * Mathf.Abs(ballRB.velocity.y)) / (9.81f * gravityMultiplier);
        airTime *= 1 - (0.0005f * ballRB.velocity.magnitude);
        float xPos = ballRB.velocity.x * airTime;
        float zPos = ballRB.velocity.z * airTime;
        Vector3 targetPos = new Vector3(xPos, ballTarget.transform.position.y, zPos);
        ballTarget.transform.position = targetPos + new Vector3(currentBall.transform.position.x, 0, currentBall.transform.position.z);
    }

    private void OnFairBall()
    {
        ballTarget.GetComponent<MeshRenderer>().enabled = false;
        landingPadPlaced = false;
    }
}
