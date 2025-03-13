using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{
    public Transform pitchPoint;
    public Transform releasePoint;
    public GameObject ball;

    [SerializeField] private float pitchSpeed;
    [SerializeField] private float accuracy;
    [SerializeField] private bool rightHanded;

    private GameObject liveBall;
    private Animator myAnim;
    private int pitchWindup = 0;
    private Ballpark currentField;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        if (rightHanded)
        {
            pitchPoint = currentField.pitchPoints[0];
        }
        else
        {
            pitchPoint = currentField.pitchPoints[1];
        }
        myAnim = GetComponent<Animator>();

        Ballpark.deadBall += onDeadBall;
        onDeadBall(); //Sets up rotation
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.P) && pitchWindup == 0)
        {
            if(liveBall != null)
            {
                Destroy(liveBall);
                liveBall = null;
                currentField.removeTheBall();
            }
            myAnim.Play("Windup");
            liveBall = Instantiate(ball, releasePoint.position, Quaternion.identity);
            liveBall.transform.parent = releasePoint; //Ball moves with release point
            pitchWindup = 1;
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            myAnim.SetBool("Pitching", true);
        }
    }

    private void pitch()
    {
        myAnim.SetBool("Pitching", false);
        //Random Position
        Vector3 targetPos = pitchPoint.position;
        targetPos.z *= 1 + Random.Range(accuracy - 1, 1 - accuracy);
        Vector3 pitchDirection = targetPos - transform.position; //Relative position vector

        //Random speed
        float ballSpeed = pitchSpeed * Random.Range(0.85f, 1.15f);
        if(pitchWindup == 1)
        {
            ballSpeed *= 1.25f;
        }

        liveBall.GetComponent<Rigidbody>().velocity = pitchDirection * ballSpeed; //Make it normalized later
        currentField.currentBall = liveBall;
        liveBall.transform.parent = null; //Ball is independent
        pitchWindup = 0;
    }

    public void windingUp()
    {
        if(pitchWindup == 1)
        {
            pitchWindup = 2;
        }
        else
        {
            pitchWindup = 1;
        }
    }

    private void onDeadBall()
    {
        //Rotate
        Vector3 temp = currentField.fieldCameras[0].transform.position;
        temp.y = transform.position.y;
        transform.LookAt(temp);
    }
}
