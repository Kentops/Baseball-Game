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

    private Rigidbody myRb;
    private Collider myCol;
    private bool passedFairMarker = false;
    private bool isFoul = false;

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
                Ballpark.fairBall();
            }
            //Physics
            if (grounded == false)
            {
                //needs to be a vector
                myRb.linearVelocity -= new Vector3(0f, 1, 0f) * gravityValue * Time.deltaTime; //Time.deltaTime works in the update function
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
        useGravity = false;
        StopCoroutine("checkHeld"); //No need to check
        myRb.linearVelocity = Vector3.zero;
        myRb.angularVelocity = Vector3.zero;
        myCol.isTrigger = true;
    }

    public void onThrow()
    {
        //Prepare to be thrown
        myCol.isTrigger = false;
        isHeld = 1;
        transform.parent = null;
        useGravity = true;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(isFoul)
        {
            return; //We are foul, we don't care about anything.
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
            StartCoroutine("checkHeld");
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Foul Territory"))
        {
            if(firstGrounded == false || passedFairMarker == false) //Ball is foul if it lands foul or goes foul before fair marker (bases)
            {
                Ballpark.foulBall();
                isFoul = true;
            }
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
        else if (other.gameObject.layer == LayerMask.NameToLayer("Foul Fly")) //Ball has left the park foul
        {
            if (firstGrounded == true)
            {
                //Ground rule double or something
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
            hold();
            Fielder myFielder = other.gameObject.GetComponent<Fielder>();
            transform.parent = myFielder.ballHeldPos;
            transform.position = transform.parent.position;
            myFielder.holdingBall = true;
            myFielder.StartCoroutine("HoldingBall");

        }
    }

    private IEnumerator checkHeld()
    {
        //After being on the ground for two seconds, defenders will start to move again
        yield return new WaitForSeconds(2);
        if(transform.parent == null)
        {
            Debug.Log("Hi there problem");
            isHeld = 0;
        }
    }

}
