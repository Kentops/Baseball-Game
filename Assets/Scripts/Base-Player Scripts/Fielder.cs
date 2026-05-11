using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Fielder : MonoBehaviour
{
    private Ballpark currentField;
    private GameObject theBall;
    private BaseBall ballInfo;
    private Rigidbody myRB;
    private NavMeshAgent myNav;
    private bool grounded = true;
    private bool touchingOthers = false;
    private Vector3 lookTarget;
    private int throwTarget;
    private bool canLook = true;

    [SerializeField] private Transform rayPosition;

    public Transform ballHeldPos;
    public bool holdingBall = false;
    public bool canMove = true;
    public bool offPosition = false; //When moved
    public int pursueTarget = 0;


    public float speed;

    public float throwingHeight; // 0.1 Is an absoulute lob, 2 is a straight line
    public float throwingStrength; //130 covers all infield throws, 200 gets a ball from the outfield to the infield, 350 should get everything

    [Header("Input")]
    public InputActionReference ia_directional;
    public InputActionReference ia_prepThrow;
    private Vector2 _inputDirection; //The directional value of input
    private bool _throwPrepped;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        myRB = GetComponent<Rigidbody>();
        myNav = GetComponent<NavMeshAgent>();
        myNav.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        //Physics
        if(grounded == false)
        {
            transform.position -= new Vector3(0, 1, 0) * currentField.gravityMultiplier * 9.81f * Time.deltaTime;
        }

        //Read input
        _inputDirection = ia_directional.action.ReadValue<Vector2>();
        _throwPrepped = ia_prepThrow.action.ReadValue<float>() >= 0.5f; //Return true if key down

        //Rotate fielder
        Vector3 relativePos;
        Quaternion lookRot;



        if(canLook == true) //Don't automatically look when we're doing manual movements
        {
            if (lookTarget != Vector3.zero && lookTarget != transform.position)
            {
                relativePos = lookTarget - transform.position;
                lookRot = Quaternion.LookRotation(relativePos, Vector3.up);
            }
            else
            {
                //Look at catcher
                relativePos = currentField.fieldCameras[0].transform.position - transform.position;
                lookRot = Quaternion.LookRotation(relativePos);

                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5);
            }

            //Apply
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5);
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

    //Controls the movement of the fielder
    private IEnumerator trackBall()
    {
        pursueTarget = -1;
        while(theBall != null)
        {
            if (canMove && !holdingBall)
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
                        //Follow Ball if it has been grounded
                        targetPos = theBall.transform.position;
                    }
                }
                else if(pursueTarget == 0)
                {
                    targetPos = transform.position;
                }
                else
                {
                    //Defense!
                    targetPos = currentField.fieldPos[pursueTarget].position;
                }

                //Rotate
                if (Vector3.Distance(targetPos, transform.position) < 2)
                {
                    //If not pursuing
                    lookTarget = theBall.transform.position;
                    lookTarget.y = transform.position.y;
                }
                else
                {
                    lookTarget = targetPos;
                }

                //Move
                if (!touchingOthers && !holdingBall) //Prevent running through walls
                {
                    //transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                    myNav.destination = targetPos;
                }
            }
            

            yield return null;

        }
        lookTarget = Vector3.zero;
    }

    //Deals with throwing the ball
    public IEnumerator HoldingBall()
    {
        canMove = false;
        lookTarget = Vector3.zero; //Look at home when you get the ball.

        while (holdingBall == true)
        {
            if(_throwPrepped == false) //False here because I realize throwing will be much more common than moving
            {
                //Throw ball if direction inputted
                if(_inputDirection.x == 1) //First Base
                {
                    throwTarget = 1;
                    //Don't throw to yourself
                    if (currentField.baseDefenders[throwTarget] != this)
                    {
                        StartCoroutine("throwBall");
                        StopCoroutine("HoldingBall");
                    }
                }
                else if (_inputDirection.y == 1) //Second Base
                {
                    throwTarget = 2;
                    if (currentField.baseDefenders[throwTarget] != this)
                    {
                        StartCoroutine("throwBall");
                        StopCoroutine("HoldingBall");
                    }
                }
                else if(_inputDirection.x == -1) //Third Base
                {
                    throwTarget = 3;
                    if (currentField.baseDefenders[throwTarget] != this)
                    {
                        StartCoroutine("throwBall");
                        StopCoroutine("HoldingBall");
                    }
                }
                else if(_inputDirection.y == -1) //Home Base
                {
                    throwTarget = 0;
                    if (currentField.baseDefenders[throwTarget] != this)
                    {
                        StartCoroutine("throwBall");
                        StopCoroutine("HoldingBall");
                    }
                }
                
            }
            else //Throw is not prepped, move on input
            {
                Vector3 moveVector = Vector3.zero;

                if(_inputDirection.x > 0.5f) //Move right
                {
                    moveVector += Vector3.back;
                    offPosition = true; //Tell defense to cover your position
                    canLook = false; //Disable automatic rotation;
                }
                else if (_inputDirection.x < -0.5f) //Move Left
                {
                    moveVector += Vector3.forward;
                    offPosition = true;
                    canLook = false;
                }

                if (_inputDirection.y > 0.5f) //Move Up
                {
                    moveVector += Vector3.right;
                    offPosition = true;
                    canLook = false;
                }
                else if(_inputDirection.y < -0.5f)
                {
                    moveVector += Vector3.left;
                    offPosition = true;
                    canLook = false;
                }

                //Apply movement and rotation
                myNav.destination = moveVector.normalized + transform.position;
                lookTarget = moveVector + transform.position;

                //Rotate fielder
                if(lookTarget != transform.position)
                {
                    Vector3 relativePos;
                    Quaternion lookRot;
                    relativePos = lookTarget - transform.position;
                    lookRot = Quaternion.LookRotation(relativePos);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5);
                }
            }
            yield return null;
        }
    }

    private IEnumerator throwBall()
    {
        canMove = false;
        //Rotate
        Vector3 temp = currentField.fieldPos[throwTarget + 10].transform.position;
        temp.y = transform.position.y;
        lookTarget = temp;
        canLook = true; //Enable automatic rotation
        yield return new WaitForSeconds(0.5f);

        //Throw to the base
        Rigidbody ballRB = currentField.currentBall.GetComponent<Rigidbody>();
        Vector3 targetPos = currentField.fieldPos[throwTarget+10].transform.position - theBall.transform.position;
        float airTime = 1 / throwingHeight;//(2 * Mathf.Abs(throwingSpeed)) / (9.81f * currentField.gravityMultiplier);

        //Prepare ball
        theBall.transform.position = ballHeldPos.position;
        ballInfo.onThrow();

        Vector3 defaultThrow = new Vector3(targetPos.x / airTime, currentField.gravityMultiplier * 9.81f * airTime/ 2, targetPos.z / airTime);

        //Is our fielder strong enough for this throw?
        Debug.Log("Required throwing strength: " + defaultThrow.magnitude);
        if(defaultThrow.magnitude < throwingStrength)
        {
            //Fielder is strong enough
            ballRB.linearVelocity = defaultThrow;
        }
        else
        {
            //Fielder is not strong enough, use their power as the magnitude
            ballRB.linearVelocity = defaultThrow.normalized * throwingStrength;
        }
        holdingBall = false;
        offPosition = false; //Reincorporate into defensive scheme
        pursueTarget = 0; //Wait to be reassigned

        yield return new WaitForSeconds(2); //Delay before moving again
        canMove = true;

    }

    private void onDeadBall()
    {
        theBall = null;
        ballInfo = null;
        holdingBall = false;
        canMove = true;
        myNav.ResetPath();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
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

    //Colliders only really nned to be active when fielding
    private void OnEnable()
    {
        Ballpark.deadBall += onDeadBall;
        Ballpark.ballHit += getLiveBall;
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = true;
        }
        GetComponent<NavMeshAgent>().enabled = true;
    }

    private void OnDisable()
    {
        Ballpark.deadBall -= onDeadBall;
        Ballpark.ballHit -= getLiveBall;
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
    }


}
