using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Runner : MonoBehaviour
{
    public bool onBase = false;
    public int baseStarted; //0 is none. Given at spawn by team control
    public int lastBaseTouched; //So our runners stay in base lines when retreating
    public bool reachedBase = false;
    public int targetBase; //Used for baseBug
    public GameObject baseBugIcon;

    //Public behavior
    public bool flyRetreat; //Back to initial base
    public bool isAdvancing;
    public bool isRetreating;
    public bool isStalled;

    //Private behavior, these are flags that tell the runner to do something when true;
    private bool stall; //Stop moving
    private bool advance;
    private bool retreat; //Back to last base
    private bool canInteract = false; //Determines if player can command runners. Enabled after ball hit.

    [Header("Skill Fields")]
    public float speed;

    [Header("Input")]
    [SerializeField] private InputActionReference ia_directional;
    [SerializeField] private InputActionReference ia_retreat;
    [SerializeField] private InputActionReference ia_advance;
    private Vector2 _dirInput;
    private int _advanceInput;


    private NavMeshAgent myNav;
    private bool isOut;


    public void startRunning()
    {
        reachedBase = false;
        StartCoroutine(runCoroutine());
    }

    public void onReachBase(int baseNum)
    {
        clearStatus();

        onBase = true;
        reachedBase = true;
        lastBaseTouched = baseNum;
    }


    public void onOut() //We're out :(
    {
        if(isOut) { return; }

        isOut = true;
        ScoreKeeper.i.callOut(); //Confirm the out
        //Remove from runners list and graphics list
        int temp = TeamControl.i.runnersInPlay.IndexOf(this);
        BaseBugManager.i.removeRunnerBug(temp);
        TeamControl.i.runnersInPlay.Remove(this);


        //Delete once we reach dugout
        StartCoroutine(leaveCoroutine());
    }

    public void onScore()
    {
        ScoreKeeper.i.onScore();
        int temp = TeamControl.i.runnersInPlay.IndexOf(this);
        BaseBugManager.i.removeRunnerBug(temp);
        TeamControl.i.runnersInPlay.Remove(this);
        isOut = true; //Want no one calling us out
        StartCoroutine(leaveCoroutine());
    }

    public void onFlyOut() //Someone else's fly out
    {
        if(isOut) { return; }
        flyRetreat = true;
        retreat = true;
    }

    public void onFoulBall()
    {
        StopAllCoroutines();
        myNav.destination = transform.position;
        canInteract = false;
    }
    private void onPlayEnd() //When screen starts to fade
    {
        canInteract = false; //Prevents runners from moving when play is over
    }

    /// <summary>
    /// Clears public behaviors such as isAdvancing
    /// </summary>
    private void clearStatus()
    {
        isAdvancing = false;
        isRetreating = false;
        isStalled = false;
    }

    #region Input response

    private void Update()
    {
        _dirInput = ia_directional.action.ReadValue<Vector2>();
    }

    private void onAdvanceInput(InputAction.CallbackContext obj) //If advance pressed twice while runner on base, go another base;
    {
        if (checkDirectional() == false) { return; } //See if this input is for us

        if(isRetreating && !flyRetreat) //If retreating, stop.
        {
            retreat = false;
            stall = true;
            return;
        }
        if (isStalled)  //If stalled, resume movement towards the next base.
        { 
            stall = false; 
            myNav.isStopped = false;
            advance = true;
            return; 
        }
        else if (isAdvancing || !onBase) { return; } //If advancing or not on base (advancing), return;

        _advanceInput++;

        if (_advanceInput == 2)
        {
            advance = true;
            _advanceInput = 0;
        }
        
    }
    private void onRetreatInput(InputAction.CallbackContext obj)
    {
        if (checkDirectional() == false) { return; } //See if this input is for us

        if (flyRetreat || isRetreating) { return; } //We're already leaving
        else if (!isStalled)
        {
            //Stop moving
            stall = true;
        }
        else if (isStalled)
        {
            //Start retreating
            retreat = true;
        }
    }

    /// <summary>
    /// Returns true if directional input is empty or is selecting the runner
    /// </summary>
    private bool checkDirectional()
    {
        if(canInteract == false) { return false; }
        //Don't affect runner coming from home
        if (lastBaseTouched == 0) { return false; }

        else if (_dirInput == Vector2.zero) { return true; } //No input, affect everyone
        else if (_dirInput == Vector2.right && lastBaseTouched == 1) { return true; } //first base
        else if (_dirInput == Vector2.up && lastBaseTouched == 2) { return true; } //second base
        else if (_dirInput == Vector2.left && lastBaseTouched == 3) { return true; } //third base
        else { return false; }

    }

    //Stall cant be allowed at start otherwise batting (pressing r) stalls runners
    private IEnumerator stallDelay()
    {
        yield return new WaitForSeconds(0.25f);
        canInteract = true;
    }
    #endregion

    private void OnEnable()
    {
        Ballpark.ballHit += startRunning;
        Ballpark.flyOut += onFlyOut;
        Ballpark.foulBall += onFoulBall;
        Ballpark.playEnd += onPlayEnd;
        ia_retreat.action.started += onRetreatInput;
        ia_advance.action.started += onAdvanceInput;

        GetComponent<NavMeshAgent>().enabled = true; //Give us collision again
        myNav = GetComponent<NavMeshAgent>();
        myNav = GetComponent<NavMeshAgent>();
        myNav.speed = speed * 0.75f;

        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = true;
        }

        //Tell team control that we are a runner in play (Added to the end)
        TeamControl.i.runnersInPlay.Add(this);
        BaseBugManager.i.createRunnerBug(0, baseBugIcon);
        reachedBase = false;

        //Set target base and tell navmesh don't move
        myNav.isStopped = true;
    }

    private void OnDisable()
    {
        Ballpark.ballHit -= startRunning;
        Ballpark.flyOut -= onFlyOut;
        Ballpark.foulBall -= onFoulBall;
        Ballpark.playEnd -= onPlayEnd;
        ia_retreat.action.started -= onRetreatInput;
        ia_advance.action.started -= onAdvanceInput;

        StopAllCoroutines();
        myNav.speed = speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        //OnTriggerEnter is active even when script is disabled!!! Since When???
        if(this.enabled == false) { return; }

        if(other.tag.Equals("Player") && other.GetComponent<Fielder>().enabled
            && other.GetComponent<Fielder>().holdingBall && onBase == false)
        {
            //Tagged out
            onOut();
        }
    }

    private IEnumerator leaveCoroutine() //Leave the field (Out or score)
    {
        myNav.isStopped = false;
        myNav.destination = Ballpark.i.dugouts[0].position;
        myNav.speed = 60;
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }

        while (transform.position != myNav.destination)
        {
            myNav.isStopped = false;
            yield return null;
        }
        //We reached it
        Destroy(gameObject);
    }

    private IEnumerator runCoroutine() //Makes a batter run
    {
        targetBase = baseStarted + 1;
        myNav.destination = Ballpark.i.basePos[targetBase].position;
        StartCoroutine(stallDelay()); //So we don't stall out to start;

        myNav.isStopped = false; //Navmesh moves runner


        //We reached the next base, now what?
        while(!isOut)
        {
            if (flyRetreat) //Fly ball caught
            {
                clearStatus();

                myNav.isStopped = false;

                int prevBase = lastBaseTouched;
                while (prevBase != baseStarted)
                {
                    isRetreating = true;
                    targetBase = prevBase;
                    myNav.destination = Ballpark.i.basePos[prevBase].position;
                    
                    while (transform.position != myNav.destination)
                    {
                        yield return null;
                    }
                    //reached previous base
                    prevBase--;
                }
                //Once more now that we know prevBase is our starting base
                isRetreating = true;
                myNav.destination = Ballpark.i.basePos[prevBase].position;
                targetBase = prevBase;
                while (transform.position != myNav.destination)
                {
                    yield return null;
                }
                
                //Returned to base
                onBase = true;
                flyRetreat = false;
                clearStatus();
            }

            //User intervention
            else if(stall)
            {
                //Stop baserunner in their tracks
                myNav.isStopped = true;
                stall = false; //We stopped, don't need to keep doing it

                //Keep track of behavior
                clearStatus();
                isStalled = true;
            }
            else if (retreat) //Go to previous base
            {
                myNav.isStopped = false;
                myNav.destination = Ballpark.i.basePos[lastBaseTouched].position;
                targetBase = lastBaseTouched;

                //Behavior status
                clearStatus();
                isRetreating = true;
                retreat = false;
            }
            else if(advance) //Advance to the next base
            {
                myNav.isStopped = false;
                targetBase = lastBaseTouched + 1;
                myNav.destination = Ballpark.i.basePos[targetBase].position;

                //Behavior Status
                clearStatus();
                isAdvancing = true;
                advance = false;

            }

                yield return null;
        }
        
        
    }

}
