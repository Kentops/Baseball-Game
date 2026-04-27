using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pitcher : MonoBehaviour
{
    public Transform releasePoint;
    public GameObject ball;

    [SerializeField] private float pitchSpeed;
    [SerializeField] private float accuracy;
    [SerializeField] private float pitchMovement;

    private GameObject liveBall;
    private Animator myAnim;
    private int pitchWindup = 0;
    private float shiftAmount = 0f;
    private Ballpark currentField;
    private Transform defaultPitchPoint;
    private Transform pitcherTarget;
    private bool hasPitched = false;


    [Header("Input")]
    [SerializeField] private InputActionReference ia_directional;
    [SerializeField] private InputActionReference ia_pitch;
    private Vector2 _directionInput;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();

        defaultPitchPoint = Ballpark.i.pitchPoints[0].transform;
        pitcherTarget = Ballpark.i.pitchPoints[1];

        myAnim = GetComponent<Animator>();
        Ballpark.deadBall += onDeadBall;
        onDeadBall(); //Sets up rotation
    }

    // Update is called once per frame
    void Update()
    {
        //Read input
        _directionInput = ia_directional.action.ReadValue<Vector2>();


        //Apply shift
        if(hasPitched == false)
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

            if (amount != 0)
            {
                transform.position += Vector3.back * amount;
                pitcherTarget.position += Vector3.back * amount;
                shiftAmount += amount;
            }
        }

        else //Ball is in the air
        {
            //Wiggle the ball
            liveBall.transform.position += Vector3.back * pitchMovement * _directionInput.x * Time.deltaTime;
        }

       
    }

    private void onWind (InputAction.CallbackContext obj)
    {
        if (liveBall != null)
        {
            Destroy(liveBall);
            liveBall = null;
            currentField.removeTheBall();
        }
        myAnim.Play("P-Windup");
        liveBall = Instantiate(ball, releasePoint.position, Quaternion.identity);
        liveBall.transform.parent = releasePoint; //Ball moves with release point
        pitchWindup = 1;
    }

    private void onPitch(InputAction.CallbackContext obj)
    {
        hasPitched = true; //Once we start the anim, we're going
        myAnim.SetBool("Pitching", true);
    }

    private void pitch()
    {
        myAnim.SetBool("Pitching", false);
        //Random Position
        Vector3 targetPos = pitcherTarget.position;
        targetPos.z *= 1 + Random.Range(accuracy - 1, 1 - accuracy);
        Vector3 pitchDirection = targetPos - liveBall.transform.position; //Relative position vector

        //Random speed
        float ballSpeed = pitchSpeed * Random.Range(0.85f, 1.15f);
        if(pitchWindup == 1)
        {
            ballSpeed *= 1.25f;
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
        }
        else
        {
            pitchWindup = 1;
        }
    }

    private void onDeadBall()
    {
        //Rotate
        Vector3 temp = currentField.fieldCameras[0].transform.position;
        temp.y = transform.position.y;
        transform.LookAt(temp);

        //Allow another pitch
        hasPitched = false;
    }

    private void OnEnable()
    {
        Ballpark.deadBall += onDeadBall;
        ia_pitch.action.started += onWind;
        ia_pitch.action.canceled += onPitch;
    }

    private void OnDisable()
    {
        Ballpark.deadBall -= onDeadBall;
        ia_pitch.action.started -= onWind;
        ia_pitch.action.canceled -= onPitch;

        shiftAmount = 0;
        if(pitcherTarget.gameObject.activeSelf)
        {
            pitcherTarget.position = defaultPitchPoint.position;
        }
    }
}
