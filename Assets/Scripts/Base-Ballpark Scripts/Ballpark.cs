using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballpark : MonoBehaviour
{
    public Transform leftMark;
    public Transform centerMark;
    public Transform rightMark;
    public GameObject currentBall;
    public Transform[] pitchPoints;
    public StrikeZone[] batterSwingCheck;
    public Camera[] fieldCameras; //0 - Catcher, 1 - Ball
    public GameObject ballTarget;
    [SerializeField] private GameObject fieldPositionHolder;
    public float gravityMultiplier;

    public Vector3 flyBallLanding;
    public Transform[] fieldPos; //In conventional baseball order but rf is 0.
    public Fielder[] baseDefenders;

    private Transform ballCam;
    private bool ballCamCanMove = true;
    private bool landingPadPlaced = false;

    public static Ballpark i;
    public delegate void FieldEvent();
    public static FieldEvent ballHit;
    public static FieldEvent fairBall;
    public static FieldEvent deadBall; //Ball removed from play
    public static FieldEvent resetField; //Elements of scene reset

    // Start is called before the first frame update
    void Start()
    {
        if (i == null) { i = this; } else { Destroy(gameObject);  } //Singleton

        ballCam = fieldCameras[1].transform;
        fieldPos = new Transform[fieldPositionHolder.transform.childCount];
        for(int i = 1; i < fieldPositionHolder.transform.childCount; i++)
        {
            fieldPos[i] = fieldPositionHolder.transform.GetChild(i);
        }
        baseDefenders = new Fielder[4];
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
            if (currentBall == null) //Change for each stadium
            {
                ballCamCanMove = false;
                ballTarget.GetComponent<MeshRenderer>().enabled = false;
                flyBallLanding = Vector3.zero;
            }

            if(!landingPadPlaced && cbPos.y > transform.position.y + 25)
            {
                ballTarget.GetComponent<MeshRenderer>().enabled = true;
                landingPadPlaced = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            //Delete ball and reset game;
            deadBall();
            resetField();
        }
        if (flyBallLanding != Vector3.zero && currentBall.GetComponent<BaseBall>().isHeld == 2)
        {
            ballTarget.GetComponent<MeshRenderer>().enabled = false;
            flyBallLanding = Vector3.zero;
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
        Destroy(currentBall);
        currentBall = null;
        flyBallLanding = Vector3.zero;
        baseDefenders = new Fielder[4];
    }

    private void setBallLanding()
    {
        //Sets the visual for where the ball will land
        Rigidbody ballRB = currentBall.GetComponent<Rigidbody>();
        float airTime = (2 * Mathf.Abs(ballRB.linearVelocity.y)) / (9.81f * gravityMultiplier);
        float xPos = ballRB.linearVelocity.x * airTime;
        float zPos = ballRB.linearVelocity.z * airTime;
        Vector3 targetPos = new Vector3(xPos, ballTarget.transform.position.y, zPos);
        ballTarget.transform.position = targetPos + new Vector3(currentBall.transform.position.x, 0, currentBall.transform.position.z);
        flyBallLanding = ballTarget.transform.position;
    }

    private void OnFairBall()
    {
        ballTarget.GetComponent<MeshRenderer>().enabled = false;
        landingPadPlaced = false;
        flyBallLanding = Vector3.zero;
    }

    private void OnEnable()
    {
        ballHit += swapToBallCam;
        ballHit += setBallLanding;
        fairBall += OnFairBall;
        deadBall += removeTheBall;
        resetField += swapToCatcherCam;
    }

    private void OnDisable()
    {
        ballHit -= swapToBallCam;
        ballHit -= setBallLanding;
        fairBall -= OnFairBall;
        deadBall -= removeTheBall;
        resetField -= swapToCatcherCam;
    }

}
