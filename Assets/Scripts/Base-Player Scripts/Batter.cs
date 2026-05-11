using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Batter : MonoBehaviour
{
    [Header ("Skill Fields")]
    public float power;
    public float placementConsistency; //How close to dead left/center/right 0-1
    public bool isRightHanded;
    public float[] timingZonePercentages;
    public float[] poorContactAngles;
    public float[] goodContactAngles;
    public float[] greatContactAngles;


    //Hitspeed is based on the animation
    [SerializeField] private int windingUp = 0;
    private float shiftAmount;
    private bool canMove = true;

    [SerializeField] private Animator myAnim;
    [SerializeField] private GameObject bat;
    private Ballpark currentField;
    private StrikeZone[] swingCheck;
    private Transform batterTarget;
    private Pitcher opposingPitcher;

    [Header("Input")]
    public InputActionReference ia_directional;
    private Vector2 _directionInput;
    public InputActionReference ia_swing;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
    }

    // Update is called once per frame
    void Update()
    {
        //Read input
        _directionInput = ia_directional.action.ReadValue<Vector2>();

        //Shift
        if(canMove)
        {
            float amount = 0;
            if (_directionInput.x > 0)
            {
                if (shiftAmount + 5 * Time.deltaTime > 2)
                {
                    amount = 2 - shiftAmount;
                }
                else
                {
                    amount = 5 * Time.deltaTime;
                }
            }
            else if (_directionInput.x < 0)
            {
                if (shiftAmount - 5 * Time.deltaTime < -2)
                {
                    amount = -2 - shiftAmount;
                }
                else
                {
                    amount = -5 * Time.deltaTime;
                }
            }
            else if (_directionInput.y > 0)
            {
                amount = -1 * shiftAmount;
            }

            //Apply shift
            if (amount != 0)
            {
                transform.position += Vector3.back * amount;
                batterTarget.position += Vector3.back * amount;
                swingCheck[0].transform.position += Vector3.back * amount; //Shift hittable area.
                shiftAmount += amount;
            }
        }
        

    }

    private void onWindup(InputAction.CallbackContext obj)
    {
        canMove = false;
        myAnim.Play("B-Windup");
        windingUp = 0;
    }

    private void onSwing(InputAction.CallbackContext obj)
    {
        myAnim.SetBool("Swinging", true);
    }


    public void wound() //Called in the animation via event
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
        myAnim.SetBool("Swinging", false); //Set variables for swing
        windingUp = 0;

        if (swingCheck[0].isStrike == true) //Check if ball is in hitting range
        {
            swingCheck[0].isStrike = false;

            //Get the ball
            GameObject ball = Ballpark.i.currentBall;
            Rigidbody ballRB = ball.GetComponent<Rigidbody>();
            ballRB.linearVelocity = Vector3.zero; //Stop the ball while we calculate.

            //Determine direction based on position in hittable area.
            float maxX = swingCheck[0].transform.position.x + (0.5f * swingCheck[0].transform.parent.localScale.x);
            float minX = swingCheck[0].transform.position.x - (0.5f * swingCheck[0].transform.parent.localScale.x);
            
            int hitTiming = 4; //The actual value to determine the timing on a hit. //very early, early, good, late, very late
            float sumOfPrev = 0;

            for(int i = 0; i < 4; i++)
            {
                float barrierValue = minX * (timingZonePercentages[i] + sumOfPrev) + maxX * (1-sumOfPrev-timingZonePercentages[i]);
                if (ball.transform.position.x > barrierValue)
                {
                    hitTiming = i;
                    break; //Don't calculate the rest
                }
                sumOfPrev += timingZonePercentages[i];
            }

            //Assigning the direction of the ball
            Vector3 target = new Vector3(0, 0, 0);
            if(isRightHanded)
            {
                target = Ballpark.i.batterMarks[hitTiming].position;
            }
            else //Mirror for lefties
            {
                target = Ballpark.i.batterMarks[4 - hitTiming].position;
            }
            //Add some randomization
            target.x = target.x * (1 + Random.Range(placementConsistency - 1, 1 - placementConsistency));
            target.z = target.z * (1 + Random.Range(placementConsistency - 1, 1 - placementConsistency));


            //Determine quality of contact
            int contactLevel = 1; //1-3, poor,good,great
            float hitPower;
            if (swingCheck[2].isStrike == true)
            {
                contactLevel = 3; //Great
                hitPower = power * Random.Range(1,1.5f); //Power = power stat * 4th root of 1-100 -1.25 (yields 1-1.8 power modifier)
            }
            else if (swingCheck[1].isStrike == true)
            {
                contactLevel = 2; //Good
                hitPower = power * Random.Range(0.9f,1.3f); //0 - 1.8 power modifier
                if(hitPower < 90) { hitPower = 90; }
            }
            else
            {
                hitPower = Random.Range(90, power); //poor
            }

            //Determine Launch angle
            //Here launch angle will be the percentage of the unit vector for the direction
            float randomAngle;
            if(contactLevel == 1)
            {
                randomAngle = Random.Range(poorContactAngles[0], poorContactAngles[1]);
            }
            else if(contactLevel == 2)
            {
                randomAngle = Random.Range(goodContactAngles[0], goodContactAngles[1]);
            }
            else
            {
                randomAngle = Random.Range(greatContactAngles[0], greatContactAngles[1]);
            }

            //Charge up's impact on angle
            if (windingUp == 2 && contactLevel > 1) //If wound up with at least good contact, receive 15% bonus in power
            {
                hitPower *= 1.15f;
                randomAngle *= 1.15f;
            }

            //Pitcher impact on angle and speed
            randomAngle += opposingPitcher.angleInfluence;
            if(opposingPitcher.isFastball)
            {
                hitPower += opposingPitcher.fastballSpeed / 10f;
            }
            else
            {
                hitPower += opposingPitcher.curveballSpeed / 10f;
            }


            //Create a normalized direction vector
            randomAngle /= 90f;
            target = target - transform.position;
            target.y = 0;
            target = target.normalized;
            target.x *= 1 - randomAngle; 
            target.z *= 1 - randomAngle;
            target.y = randomAngle;

            Debug.Log($"Hit power: {hitPower}, Angle: {target.y * 90}, Contact: {contactLevel}, Timing: {hitTiming}");

            //Ball stuff
            //Vector3 direction = target - transform.position;
            ballRB.linearVelocity = target.normalized * hitPower; //normalized is unit vector
            ball.GetComponent<BaseBall>().isHeld = 0;
            ball.GetComponent<BaseBall>().gravityValue = currentField.gravityMultiplier * 9.81f;



            //Let the game know it's a hit
            Ballpark.ballHit();
        }
        //End of swing, ball is irrelevant here
        canMove = true;

    }

    private void OnEnable()
    {
        //Disable colliders (used for physical collisions and touching the ball/base)
        foreach(Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
        GetComponent<NavMeshAgent>().enabled = false; //Disabled as a batter so we aren't bumped at all.
        bat.SetActive(true);

        ia_swing.action.started += onWindup;
        ia_swing.action.canceled += onSwing;

        //Go to position
        if(isRightHanded)
        {
            transform.position = Ballpark.i.fieldPos[14].position;
        }
        else
        {
            transform.position = Ballpark.i.fieldPos[15].position;
        }

        swingCheck = new StrikeZone[3];
        for(int i = 0; i<3; i++)
        {
            swingCheck[i] = Ballpark.i.batterSwingCheck[i];
        }
        batterTarget = Ballpark.i.pitchPoints[2];
        opposingPitcher = TeamControl.i.homeTeam[0].GetComponent<Pitcher>();

        if(isRightHanded == opposingPitcher.isRightHanded)
        {
            //Thinner strike zone
            swingCheck[0].transform.localScale = new Vector3(1, 1, 0.8f);
        }
    }

    private void OnDisable()
    {
        bat.SetActive(false);

        ia_swing.action.started -= onWindup;
        ia_swing.action.canceled -= onSwing;

        shiftAmount = 0;
        batterTarget.position = Ballpark.i.pitchPoints[0].position; //Default Position;
        swingCheck[0].transform.localPosition = Vector3.zero; //Default position;
        swingCheck[0].transform.localScale = new Vector3(1, 1, 1);
    }



}
