using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBall : MonoBehaviour
{
    public bool isLive = false;
    public bool grounded = false; //Touching the ground
    public bool firstGrounded = false;
    public int isHeld = 2; //0 is false, 1 is being thrown so it can be picked up, 2 is in someone's hand
    public bool useGravity = false;
    public float gravityValue = 0; //Gravity starts when hit;

    private Rigidbody myRb;

    // Start is called before the first frame update
    void Start()
    {
        myRb = GetComponent<Rigidbody>();
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
                myRb.velocity -= new Vector3(0f, 1, 0f) * gravityValue * Time.deltaTime; //Time.deltaTime works in the update function
            }
            else
            {
                //Friction
                float tick = 0.5f * Time.deltaTime;
                myRb.velocity -= new Vector3(myRb.velocity.x * tick, 0, myRb.velocity.z * tick);
            }
        }
        
    }

    public void hold()
    {
        isHeld = 2;
        useGravity = false;
        StopCoroutine("checkHeld"); //No need to check
        myRb.velocity = Vector3.zero;
        myRb.angularVelocity = Vector3.zero;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;;
            StartCoroutine("checkHeld");
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = false;
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
