using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Batter : PlayerCore //Extends Player Cores
{
    public float power;
    public float groundBallPercent; //0-1
    public float averageLaunchAngle; //height above batter for direction
    public float percentLeft, percentRight; //Chance of hitting to left or right. l + r < 1
    public float placementConsistency; //How close to dead left/center/right 0-1

    //Hitspeed is based on the animation

    [SerializeField] private Animator myAnim;
    [SerializeField] private StrikeZone swingCheck;

    // Start is called before the first frame update
    void Start()
    {
        //Will be overridden, remove later
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            swing();
        } 
    }

    public void swing()
    {
        myAnim.Play("Swing");
    }

    public void swingClimax()
    {
        if(swingCheck.isStrike == true) //Check if ball is in hitting range
        {
            swingCheck.isStrike = false;

            //Get the ball
            GameObject ball = StrikeZone.theBall;
            StrikeZone.theBall = null;
            if(ball != null) //Required in case the ball is deleted by this time
            {
                Rigidbody ballRB = ball.GetComponent<Rigidbody>();

                //Power = power stat * random - part of ball's initial speed
                float hitPower = power * Random.Range(0.7f, 1.3f) - (0.005f * ballRB.velocity.magnitude);
                Debug.Log(0.005f * ballRB.velocity.magnitude);
                if(hitPower < 0) { hitPower = 0.01f; }

                //Deciding where the ball goes
                Vector3 target = new Vector3(0, 0, 0);
                float chance = Random.Range(0f, 1f);
                if (chance < groundBallPercent)
                {
                    //Grounder
                    target.y = Random.Range(transform.position.y - 100, transform.position.y);
                }
                else
                {
                    //Flyball
                    target.y = Random.Range(transform.position.y, averageLaunchAngle * 2);
                }

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

                Vector3 direction = target - transform.position;
                ball.GetComponent<Rigidbody>().velocity = direction.normalized * hitPower; //normalized is unit vector

                //Let the game know it's a hit
                Ballpark.ballHit();
            }
            
        }
    }

    
}
