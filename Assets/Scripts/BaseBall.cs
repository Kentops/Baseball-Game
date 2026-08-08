using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBall : MonoBehaviour
{
    //public bool isLive = false;
    public bool grounded = false; //Touching the ground
    public bool firstGrounded = false;
    public int isHeld = 2; //0 is false, 1 is being thrown so it can be picked up, 2 is in someone's hand
    public bool useGravity = false;
    public float gravityValue = 0; //Gravity starts when hit;

    public GameObject currentBatter; //Used for who hit the ball

    private Rigidbody myRb;
    private Collider myCol;
    bool passedFairMarker = false;
    private bool isFoul = false;
    private bool hasBeenTouched; //True if ball is ever touched
    private Coroutine activeGroundCheck;

    // Start is called before the first frame update
    void Start()
    {
        myRb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isHeld == 0 || useGravity == true)
        {
            if (grounded == true && !firstGrounded)
            {
                firstGrounded = true;
                if (!isFoul) { Ballpark.fairBall(); }
            }
            //Physics
            if (grounded == false)
            {
                //needs to be a vector
                myRb.linearVelocity -= Vector3.up * gravityValue * Time.deltaTime; //Time.deltaTime works in the update function
            }
            else
            {
                //Friction
                float tick = 0.5f * Time.deltaTime;
                myRb.linearVelocity -= new Vector3(myRb.linearVelocity.x * tick, 0, myRb.linearVelocity.z * tick);
            }
        }
        
    }

    public void hold()
    {
        isHeld = 2;
        hasBeenTouched = true;
        useGravity = false;
        if(activeGroundCheck != null)
        {
            StopCoroutine(activeGroundCheck); //We know ball is not on the ground
            activeGroundCheck = null;
        }

        myRb.linearVelocity = Vector3.zero;
        myRb.angularVelocity = Vector3.zero;
        myCol.isTrigger = true;

        if(firstGrounded == false) //Check if we caught the ball
        {
            currentBatter.GetComponent<Runner>().onOut();
            Ballpark.flyOut();

            firstGrounded = true;
            Ballpark.fairBall(); //Ball is in play now
        }

    }

    public void onThrow()
    {
        //Prepare to be thrown
        myCol.isTrigger = false;
        isHeld = 1;
        transform.parent = null;
        useGravity = true;
    }

    private void clearFairMark() //Resets mark after passed when pitching
    {
        passedFairMarker = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(isFoul)
        {
            return; //We are foul, we don't care about anything.
        }

        if (collision.gameObject.tag.Equals("Foul Territory") && isFoul == false && hasBeenTouched == false) //isFoul is for no repeats
        {
            if (firstGrounded == false || passedFairMarker == false) //Ball is foul if it lands foul or goes foul before fair marker (bases)
            {
                Ballpark.foulBall();
                isFoul = true;
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
            startGroundCheck();
        }
        
        

    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Fair Marker"))
        {
            if(isHeld == 0) //Doesn't get marked when pither throws it. Becomes 0 when hit
            {
                passedFairMarker = true;
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Foul Fly") && isFoul == false) //Ball has left the park foul
        {
            if (firstGrounded == true)
            {
                //Ground rule double or something
                //Make another case if ball was touched. That's a free base for all
            }
            else
            {
                Ballpark.foulBall();
                isFoul = true;
            }
        }
        //Be grabbed by the fielder, so long as they are not a pitcher
        else if (other.gameObject.tag == "Player" && isHeld != 2 && other.gameObject.GetComponent<Fielder>().enabled == true)
        {
            if(isFoul) { return; }
            hold();
            Fielder myFielder = other.gameObject.GetComponent<Fielder>();
            transform.parent = myFielder.ballHeldPos;
            transform.position = transform.parent.position;
            myFielder.holdingBall = true;
            myFielder.StartCoroutine("HoldingBall");

        }
    }

    private void startGroundCheck() //Prevents duplicate checks and ensures specific check is stopped when necessary
    {
        if(activeGroundCheck == null)
        {
            activeGroundCheck = StartCoroutine(checkGround());
        }
    }

    private IEnumerator checkGround()
    {
        //After being on the ground for two seconds, defenders will start to move again. Only allow one check at a time.
        yield return new WaitForSeconds(2);
        if (transform.parent == null)
        {
            isHeld = 0;
        }
        activeGroundCheck = null; //Allow more checks
    }

    private void OnEnable()
    {
        Ballpark.ballHit += clearFairMark;
    }
    private void OnDisable()
    {
        Ballpark.ballHit -= clearFairMark;
    }

}
