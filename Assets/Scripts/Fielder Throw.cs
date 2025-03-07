using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FielderThrow : MonoBehaviour
{
    private Ballpark currentField;
    private Fielder myFielder;
    private int target;

    public float throwingSpeed = 1;
    public float maxAirTime = 50;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        myFielder = GetComponent<Fielder>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void throwBall()
    {
        //Rotate
        Vector3 temp = currentField.fieldPos[target].position;
        temp.y = transform.position.y;
        transform.LookAt(temp);

        //Prepare ball
        GameObject ball = currentField.currentBall;
        ball.transform.parent = null;
        ball.GetComponent<BaseBall>().useGravity = true;
        myFielder.holdingBall = false;

        Rigidbody ballRB = currentField.currentBall.GetComponent<Rigidbody>();
        Vector3 targetPos =currentField.fieldPos[target].position - transform.position;
        float airTime = 1/throwingSpeed;//(2 * Mathf.Abs(throwingSpeed)) / (9.81f * currentField.gravityMultiplier);
        //float xPos = ballRB.velocity.x * airTime;
        //float zPos = ballRB.velocity.z * airTime;

        ballRB.velocity = new Vector3(targetPos.x / airTime, currentField.gravityMultiplier * 9.81f / 2, targetPos.z / airTime);
    }

    public IEnumerator HoldingBall()
    {
        myFielder.enabled = false;
        GameObject ball = currentField.currentBall;
        while (myFielder.holdingBall == true)
        {
            //ball.transform.position = myFielder.ballHeldPos.position;
            if (Input.GetKeyUp(KeyCode.W))
            {
                target = 11;

                
                throwBall();
            }
            yield return null;
        }
    }
}
