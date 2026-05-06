using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Batter : MonoBehaviour
{
    public float power;
    public float groundBallPercent; //0-1
    public float averageLaunchAngle; //height above batter for direction
    public float percentLeft, percentRight; //Chance of hitting to left or right. l + r < 1
    public float placementConsistency; //How close to dead left/center/right 0-1
    public bool isRightHanded;

    //Hitspeed is based on the animation
    private int windingUp = 0;
    private float shiftAmount;

    [SerializeField] private Animator myAnim;
    [SerializeField] private GameObject bat;
    private Ballpark currentField;
    private StrikeZone[] swingCheck;
    private Transform batterTarget;

    [Header("Input")]
    public InputActionReference ia_directional;
    private Vector2 _directionInput;
    public InputActionReference ia_swing;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        GetComponent<Collider>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Read input
        _directionInput = ia_directional.action.ReadValue<Vector2>();

        //Shift
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

        //Apply shift
        if (amount != 0)
        {
            transform.position += Vector3.back * amount;
            batterTarget.position += Vector3.back * amount;
            swingCheck[0].transform.position += Vector3.back * amount; //Shift hittable area.
            shiftAmount += amount;
        }

    }

    private void onWindup(InputAction.CallbackContext obj)
    {
        myAnim.Play("B-Windup");
        windingUp = 1;
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
                ballRB.linearVelocity = Vector3.zero;
                ballRB.linearVelocity = direction.normalized * hitPower; //normalized is unit vector
                ball.GetComponent<BaseBall>().isHeld = 0;
                ball.GetComponent<BaseBall>().gravityValue = currentField.gravityMultiplier * 9.81f;



                //Let the game know it's a hit
                Ballpark.ballHit();

            }
        }
        //End of swing, ball is irrelevant here

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
    }

    private void OnDisable()
    {
        bat.SetActive(false);

        ia_swing.action.started -= onWindup;
        ia_swing.action.canceled -= onSwing;

        shiftAmount = 0;
        if (batterTarget.gameObject != null)
        {
            batterTarget.position = Ballpark.i.pitchPoints[0].position; //Default Position;
            //swingCheck.transform.position = swingCheck.transform.TransformDirection(Vector3.zero); //Default position;
        }
    }



}
