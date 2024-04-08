using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBall : MonoBehaviour
{
    public bool isLive = true;
    public bool grounded = false; //Touching the ground
    public bool firstGrounded;
    public float gravity;

    private Rigidbody myRb;

    // Start is called before the first frame update
    void Start()
    {
        myRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(grounded == true && !firstGrounded)
        {
            firstGrounded = true;
        }
        //Physics
        if(grounded == false)
        {
            //needs to be a vector
            myRb.velocity -= new Vector3 (0,1,0) * gravity * Time.deltaTime; //Time.deltaTime works in the update function
        }
        else
        {
            float tick = 0.5f * Time.deltaTime;
            myRb.velocity -= new Vector3(myRb.velocity.x * tick, 0, myRb.velocity.z * tick);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
            myRb.velocity = new Vector3(myRb.velocity.x, 0, myRb.velocity.z); //Gravity behaves
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
