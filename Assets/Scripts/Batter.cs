using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Batter : MonoBehaviour
{
    public float power;
    public float groundBallPercent; //0-1
    public float averageLaunchAngle; //height above batter for direction
    public float percentLeft, percentRight; //Chance of hitting to left or right. l + r < 1
    public float placementConsistency; //How close to dead left/center/right 0-1

    //Hitspeed is based on the animation
    private int windingUp = 0;

    [SerializeField] private Animator myAnim;
    [SerializeField] private StrikeZone swingCheck;
    private Ballpark currentField;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        GetComponent<Collider>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.S) & windingUp == 0)
        {
            myAnim.Play("Wind up");
            windingUp = 1;
        } 
        else if (Input.GetKeyUp(KeyCode.S))
        {
            swing();
        }
    }

    public void swing()
    {
        myAnim.Play("Swing");
    }

    public void wound()
    {
        if (windingUp == 1)
        {
            windingUp = 2; //Fully wound up
        }
        else
        {
            windingUp = 1; //Wound up too much
        }
        
    }

    public void swingClimax()
    {
        if (swingCheck.isStrike == true) //Check if ball is in hitting range
        {
            swingCheck.isStrike = false;
            //Get the ball
            GameObject ball = StrikeZone.theBall;
            StrikeZone.theBall = null;
            if (ball != null) //Required in case the ball is deleted by this time
            {
                Rigidbody ballRB = ball.GetComponent<Rigidbody>();

                //Power = power stat * 4th root of 1-100 -1 (yields 0-2.16 power modifier)
                float hitPower = power * (Mathf.Pow(Random.Range(1, 100), 0.25f) - 1.35f);
                if(windingUp == 2)
                {
                    hitPower *= 1.15f;
                }
                if (hitPower < 90) { hitPower = 90f; }


                //Deciding where the ball goes
                Vector3 target = new Vector3(0, 0, 0);
                float chance = Random.Range(0f, 1f);
                if (chance < groundBallPercent)
                {
                    //Grounder
                    target.y = Random.Range(transform.position.y - 50, transform.position.y + 20);
                }
                else
                {
                    //Flyball
                    target.y = Random.Range(transform.position.y, averageLaunchAngle * 2);
                    hitPower /= 1.33f;
                    if (hitPower < power) { hitPower = power; }
                    Debug.Log("Launch angle" + target.y);
                }
                Debug.Log("Hit power" + hitPower);

                //Direction of ball
                chance = Random.Range(0f, 1f);
                if (chance < percentLeft)
                {
                    //Left field hit
                    Vector3 leftField = currentField.leftMark.position;
                    target.x = leftField.x * (1 + Random.Range(placementConsistency - 1, 1 - placementConsistency));
                    target.z = leftField.z * (1 + Random.Range(placementConsistency - 1, 1 - placementConsistency));
                }
                else if (chance < percentLeft + percentRight)
                {
                    //Right field hit
                    Vector3 rightField = currentField.rightMark.position;
                    target.x = rightField.x * (1 + Random.Range(placementConsistency - 1, 1 - placementConsistency));
                    target.z = rightField.z * (1 + Random.Range(placementConsistency - 1, 1 - placementConsistency));
                }
                else
                {
                    //Center field hit
                    Vector3 centerField = currentField.centerMark.position;
                    target.x = centerField.x * (1 + Random.Range(placementConsistency - 1, 1 - placementConsistency));
                    target.z = centerField.z * (1 + Random.Range(placementConsistency - 1, 1 - placementConsistency));
                }

                //Ball stuff
                Vector3 direction = target - transform.position;
                ballRB.velocity = Vector3.zero;
                ballRB.velocity = direction.normalized * hitPower; //normalized is unit vector
                ball.GetComponent<BaseBall>().isHeld = 0;
                ball.GetComponent<BaseBall>().gravityValue = currentField.gravityMultiplier * 9.81f;



                //Let the game know it's a hit
                Ballpark.ballHit();

            }
        }
        //End of swing, ball is irrelevant here
        windingUp = 0;
    }



}
