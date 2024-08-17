using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBall : MonoBehaviour
{
    public bool isLive = false;
    public bool grounded = false; //Touching the ground
    public bool firstGrounded = false;
    public bool isHeld = true;
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
        if(isHeld == false)
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
                myRb.velocity -= new Vector3(0, 1, 0) * gravityValue * Time.deltaTime; //Time.deltaTime works in the update function
                                                                                       //myRb.AddForce(Vector3.down * mass * 9.8f * Time.deltaTime);
            }
            else
            {
                float tick = 0.5f * Time.deltaTime;
                myRb.velocity -= new Vector3(myRb.velocity.x * tick, 0, myRb.velocity.z * tick);
            }
        }
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
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
