using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FielderThrow : MonoBehaviour
{
    private Ballpark currentField;
    private Fielder myFielder;
    private int target;

    public float throwingSpeed;
    public float maxAirTime;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        myFielder = GetComponent<Fielder>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void throwBall()
    {
        Rigidbody ballRB = currentField.currentBall.GetComponent<Rigidbody>();
        Vector3 targetPos = transform.position - currentField.fieldPos[target].position;
        float airTime = 3;//(2 * Mathf.Abs(throwingSpeed)) / (9.81f * currentField.gravityMultiplier);
        float xPos = ballRB.velocity.x * airTime;
        float zPos = ballRB.velocity.z * airTime;

        ballRB.velocity = new Vector3(targetPos.x / airTime, 12, targetPos.z / airTime);
    }

    public IEnumerable HoldingBall()
    {
        Debug.Log("huh?");
        while (myFielder.holdingBall == true)
        {
            Debug.Log("test");
            currentField.currentBall.transform.position = myFielder.ballHeldPos.position;
            if (Input.GetKeyUp(KeyCode.W))
            {
                target = 11;
                myFielder.holdingBall = false;
                //currentField.currentBall.GetComponent<BaseBall>().isHeld = false;
                throwBall();
            }
            yield return null;
        }
    }
}
