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
    private int throwTarget;

    [SerializeField] private Transform rayPosition;

    public Transform ballHeldPos;
    public bool holdingBall = false;
    public int pursueTarget = 0;
    public float speed;
    public float throwingSpeed;

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
            transform.position -= new Vector3(0, 1, 0) * currentField.gravityMultiplier * 9.81f * Time.deltaTime;
        }

        //Raycast
        if(Physics.Raycast(rayPosition.position, transform.forward, out RaycastHit hit, 2f))
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
        pursueTarget = -1;
        while(theBall != null)
        {
            if (!holdingBall)
            {
                Vector3 targetPos;
                if (pursueTarget == -1)
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
                else if (pursueTarget == -2)
                {
                    //Don't move
                    targetPos = transform.position;
                }
                else
                {
                    //Defense!
                    targetPos = currentField.fieldPos[pursueTarget].position;
                }

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

                //Move
                if (!touchingOthers && !holdingBall) //Prevent running through walls
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                }
            }
            

            yield return null;

        }
        lookTarget = Vector3.zero;
    }

    public IEnumerator HoldingBall()
    {
        while (holdingBall == true)
        {
            //theBall.transform.position = ballHeldPos.position;
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                throwTarget = 2;
                //Don't throw to yourself
                if(currentField.baseDefenders[throwTarget] != this)
                {
                    StartCoroutine("throwBall");
                    StopCoroutine("HoldingBall");
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                throwTarget = 3;
                if (currentField.baseDefenders[throwTarget] != this)
                {
                    StartCoroutine("throwBall");
                    StopCoroutine("HoldingBall");
                }
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                throwTarget = 0;
                if (currentField.baseDefenders[throwTarget] != this)
                {
                    StartCoroutine("throwBall");
                    StopCoroutine("HoldingBall");
                }
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                throwTarget = 1;
                if (currentField.baseDefenders[throwTarget] != this)
                {
                    StartCoroutine("throwBall");
                    StopCoroutine("HoldingBall");
                }
            }
            yield return null;
        }
    }

    private IEnumerator throwBall()
    {
        //Rotate
        Vector3 temp = currentField.baseDefenders[throwTarget].transform.position;
        temp.y = transform.position.y;
        lookTarget = temp;
        yield return new WaitForSeconds(0.5f);

        Rigidbody ballRB = currentField.currentBall.GetComponent<Rigidbody>();
        Vector3 targetPos = currentField.baseDefenders[throwTarget].transform.position - theBall.transform.position;
        float airTime = 1 / throwingSpeed;//(2 * Mathf.Abs(throwingSpeed)) / (9.81f * currentField.gravityMultiplier);

        //Prepare ball
        theBall.transform.parent = null;
        ballInfo.isHeld = 1;
        ballInfo.useGravity = true;
        ballRB.velocity = new Vector3(targetPos.x / airTime, currentField.gravityMultiplier * 9.81f / 2, targetPos.z / airTime);

        yield return new WaitForSeconds(2); //Delay before moving again
        holdingBall = false; 
    }

    private void onDeadBall()
    {
        theBall = null;
        ballInfo = null;
        holdingBall = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == theBall && ballInfo.isHeld != 2)
        {
            ballInfo.hold();
            theBall.transform.position = ballHeldPos.position;
            theBall.transform.parent = ballHeldPos;
            holdingBall = true;
            StartCoroutine("HoldingBall"); 
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
