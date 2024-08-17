using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fielder : MonoBehaviour
{
    private Ballpark currentField;
    private GameObject theBall;
    private BaseBall ballInfo;
    private Rigidbody myRB;
    private bool grounded = true;
    private bool touchingOthers = false;
    private Vector3 lookTarget;

    [SerializeField] private Transform ballHeldPos;
    [SerializeField] private Transform rayPosition;

    public bool holdingBall = false;
    public float speed = 18;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        Ballpark.ballHit += getLiveBall;
        Ballpark.deadBall += onDeadBall;
        myRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //Physics
        if(grounded == false)
        {
            myRB.velocity -= new Vector3(0, 1, 0) * currentField.gravityMultiplier * 9.81f * Time.deltaTime;
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
        while(theBall != null && ballInfo.isHeld == false)
        {

            Vector3 targetPos;
            if (currentField.flyBallLanding != Vector3.zero && ballInfo.firstGrounded == false)
            {
                targetPos = currentField.flyBallLanding;
            }
            else
            {
                targetPos = theBall.transform.position;
            }
            targetPos.y = transform.position.y;
            lookTarget = targetPos;

            if (!touchingOthers) //Prevent running through walls
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
