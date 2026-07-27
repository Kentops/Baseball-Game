using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pitcher : MonoBehaviour
{
    [Header("Skill Fields")]
    public float curveballSpeed;
    public float fastballSpeed;
    public float accuracy;
    public float speedConsistency; //0-1
    public float pitchMovement;
    public float angleInfluence; //Added to the angle of batted ball
    public bool isRightHanded;
    public bool isFastball; //For batters to know

    [Header("Technical Elements")]
    [SerializeField] private Transform releasePoint;
    [SerializeField] private GameObject ball;
    private GameObject liveBall;
    private Animator myAnim;
    private int pitchWindup = 0;
    private float shiftAmount = 0f;
    private Ballpark currentField;
    private PitcherTarget pitcherTarget;
    private bool hasPitched = false; //Determines if ball has been released yet
    private bool canMove = true;


    [Header("Input")]
    [SerializeField] private InputActionReference ia_directional;
    [SerializeField] private InputActionReference ia_pitch;
    private Vector2 _directionInput;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();

        pitcherTarget = Ballpark.i.pitchPoints[1].transform.GetComponent<PitcherTarget>();

        myAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Read input
        _directionInput = ia_directional.action.ReadValue<Vector2>();


        //Apply shift
        if (hasPitched == false)
        {
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
                else if (_directionInput.y > 0)//Reset position
                {
                    amount = -1 * shiftAmount;
                }

                if (amount != 0)
                {
                    transform.position += Vector3.back * amount;
                    pitcherTarget.transform.position += Vector3.back * amount;
                    shiftAmount += amount;
                }
            }
        }
        else if (liveBall != null) //Ball is in the air
        {
            //Wiggle the ball
            if(isFastball)
            {
                //Lose movement with speed
                liveBall.transform.position += Vector3.back * (pitchMovement/2) * _directionInput.x * Time.deltaTime;
            }
            else
            {
                liveBall.transform.position += Vector3.back * pitchMovement * _directionInput.x * Time.deltaTime;
            }
        }

       
    }

    private void onWind (InputAction.CallbackContext obj)
    {
        if(canMove == false) { return; } //Can't pitch while pitching
        canMove = false;

        if (liveBall != null)
        {
            Destroy(liveBall);
            liveBall = null;
            currentField.removeTheBall();
        }

        //Only call if you have subscribers, otherwise we crash
        if(Ballpark.pitcherWinds != null)
        {
            Ballpark.pitcherWinds();
        }

        myAnim.Play("P-Windup");
        liveBall = Instantiate(ball, releasePoint.position, Quaternion.identity);
        liveBall.transform.parent = releasePoint; //Ball moves with release point
        pitchWindup = 0;

    }

    private void onPitch(InputAction.CallbackContext obj)
    {
        if(hasPitched == true || canMove == true) { return; }//Can't picth while pitching or while there's no ball

        hasPitched = true; //Once we start the anim, we're going
        myAnim.SetBool("Pitching", true);

        //Clear strike zone to invalidate any swing before pitch
        Ballpark.i.strikeZone.isStrike = false;
    }

    private void pitch()
    {
        myAnim.SetBool("Pitching", false);

        //Set Pitch Dir
        Vector3 targetPos = pitcherTarget.transform.position;
        targetPos.z *= 1 + Random.Range(accuracy - 1, 1 - accuracy);
        Vector3 pitchDirection = targetPos - liveBall.transform.position; //Relative position vector

        //Apply Speed to the ball;
        float ballSpeed;
        if(isFastball)
        {
            ballSpeed = fastballSpeed * Random.Range(speedConsistency, 2 - speedConsistency);
        }
        else
        {
            //Curveball
            ballSpeed = curveballSpeed * Random.Range(speedConsistency, 2 - speedConsistency);
        }

        liveBall.GetComponent<Rigidbody>().linearVelocity = pitchDirection.normalized * ballSpeed; //Make it normalized later
        currentField.currentBall = liveBall;
        liveBall.transform.parent = null; //Ball is independent
        pitchWindup = 0;
    }

    public void windingUp()
    {
        if(pitchWindup == 1)
        {
            pitchWindup = 2;
            isFastball = false;
        }
        else
        {
            pitchWindup = 1;
            isFastball = true;
        }
    }

    private IEnumerator pitchCooldown() //Cooldown between pitches for the game not to freak out
    {
        yield return new WaitForSeconds(0.1f);
        canMove = true;
        hasPitched = false;
    }

    private void onDeadBall()
    {

        liveBall = null;

        //Rotate
        Vector3 temp = Ballpark.i.fieldCameras[0].transform.position;
        temp.y = transform.position.y;
        transform.LookAt(temp);

        //Allow another pitch
        isFastball = false;
        StartCoroutine(pitchCooldown());
    }

    private void OnEnable()
    {
        Ballpark.deadBall += onDeadBall;
        ia_pitch.action.started += onWind;
        ia_pitch.action.canceled += onPitch;

        onDeadBall(); //Sets up rotation
    }

    private void OnDisable()
    {
        Ballpark.deadBall -= onDeadBall;
        ia_pitch.action.started -= onWind;
        ia_pitch.action.canceled -= onPitch;

        shiftAmount = 0;
    }
}
