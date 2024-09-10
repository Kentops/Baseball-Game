using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fielder : MonoBehaviour
{
    private Ballpark currentField;
    private GameObject theBall;
    private BaseBall ballInfo;
    private Rigidbody myRB;
    private FielderThrow myThrow;
    private bool grounded = true;
    private bool touchingOthers = false;
    private Vector3 lookTarget;

    [SerializeField] private Transform rayPosition;

    public Transform ballHeldPos;
    public bool holdingBall = false;
    public int pursueTarget = 0;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        Ballpark.ballHit += getLiveBall;
        Ballpark.deadBall += onDeadBall;
        myRB = GetComponent<Rigidbody>();

        myThrow = GetComponent<FielderThrow>();
        myThrow.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Physics
        if(grounded == false)
        {
            transform.position -= new Vector3(0, 1, 0) * currentField.gravityMultiplier * 9.81f * Time.deltaTime;
        }

        //Raycast
        if(Physics.Raycast(rayPosition.position, transform.forward, out RaycastHit hit, 1f))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                touchingOthers = true;
            }
            else
            {
                touchingOthers = false;
            }
        }
        else
        {
            touchingOthers = false;
        }


        //Rotate fielder
        if (lookTarget != Vector3.zero)
        {
            transform.LookAt(lookTarget);
        }
        else
        {
            Quaternion rotA = transform.rotation;
            Vector3 temp = currentField.fieldCameras[0].transform.position;
            temp.y = transform.position.y;
            transform.LookAt(temp);
            transform.rotation = Quaternion.Slerp(rotA, transform.rotation,Time.deltaTime * 5);
        }


    }

    public void getLiveBall()
    {
        theBall = currentField.currentBall;
        if(theBall != null)
        {
            ballInfo = theBall.GetComponent<BaseBall>();
        }
        StartCoroutine("trackBall");
    }

    private IEnumerator trackBall()
    {
        pursueTarget = 0;
        while(theBall != null)
        {
            Vector3 targetPos;
            if (pursueTarget == 0)
            {
                //Follow ball
                if (currentField.flyBallLanding != Vector3.zero && ballInfo.firstGrounded == false)
                {
                    //Follow flyball target
                    targetPos = currentField.flyBallLanding;
                }
                else
                {
                    //Follow Ball
                    targetPos = theBall.transform.position;
                }
            }
            else if(pursueTarget == -1)
            {
                //Don't move
                targetPos = transform.position;
            }
            else
            {
                //Defense!
                targetPos = currentField.fieldPos[pursueTarget].position;
            }

            if (!holdingBall)
            {
                //Where to look
                if (targetPos == transform.position)
                {
                    lookTarget = theBall.transform.position;
                    lookTarget.y = transform.position.y;
                }
                else
                {
                    targetPos.y = transform.position.y;
                    lookTarget = targetPos;
                }

            }
            //Move
            if (!touchingOthers && !holdingBall) //Prevent running through walls
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            }

            yield return null;

        }
        lookTarget = Vector3.zero;
    }

    private void onDeadBall()
    {
        theBall = null;
        ballInfo = null;
        holdingBall = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == theBall && ballInfo.isHeld == false)
        {
            ballInfo.isHeld = true;
            Rigidbody ballRb = theBall.GetComponent<Rigidbody>();
            ballRb.velocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            theBall.transform.parent = ballHeldPos;
            theBall.transform.position = ballHeldPos.position;
            holdingBall = true;
            Debug.Log(myThrow);
            myThrow.StartCoroutine("HoldingBall"); 
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = false;
        }
    }

    
}
