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
    public bool flyRetreat; //Back to initial base
    public bool retreat; //Back to last base
    public int targetBase; //Used for baseBug
    public GameObject baseBugIcon;

    private bool stall; //Stop moving
    private bool canStall = false; //Delay between ball in play and when you can retreat (So you can't stall before ball is in play);
    private bool advance;

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
        advance = false; //Clear baserunning actions
        retreat = false;

        if(baseNum == lastBaseTouched)
        {
            return;
        }
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
    }

    #region Input response

    private void Update()
    {
        
    }
    private void onAdvanceInput(InputAction.CallbackContext obj) //If advance pressed twice while runner on base, go another base;
    {
        if(retreat && !flyRetreat) //If retreating, stop.
        {
            retreat = false;
            stall = true;
            return;
        }
        if (stall)  //If stalled, resume movement towards the next base.
        { 
            stall = false; 
            myNav.isStopped = false;
            advance = true;
            return; 
        }
        else if (advance || !onBase) { return; } //If advancing or not on base (advancing), return;

        _advanceInput++;

        if (_advanceInput == 2)
        {
            advance = true;
            _advanceInput = 0;
        }
        
    }
    private void onRetreatInput(InputAction.CallbackContext obj)
    {
        if(!canStall || flyRetreat || retreat) { return; } //We're already leaving
        else if(!stall) 
        {
            //Stop moving
            stall = true; 
        } 
        else if(stall)
        {
            //Start retreating
            retreat = true;
            stall = false;
        }
    }
    private IEnumerator stallDelay()
    {
        yield return new WaitForSeconds(0.25f);
        canStall = true;
    }
    #endregion

    private void OnEnable()
    {
        Ballpark.ballHit += startRunning;
        Ballpark.flyOut += onFlyOut;
        Ballpark.foulBall += onFoulBall;
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

        while (transform.position != myNav.destination && !retreat && !flyRetreat && !stall) //Keep going towards next base
        {
            yield return null; 
        }

        //We reached the next base, now what?
        while(true)
        {
            if (flyRetreat) //Fly ball caught
            {
                myNav.isStopped = false;
                stall = false;

                int prevBase = lastBaseTouched;
                while (prevBase != baseStarted)
                {
                    myNav.destination = Ballpark.i.basePos[prevBase].position;
                    targetBase = prevBase;
                    while (transform.position != myNav.destination)
                    {
                        yield return null;
                    }
                    //reached previous base
                    prevBase--;
                }
                //Once more now that we know prevBase is our starting base
                myNav.destination = Ballpark.i.basePos[prevBase].position;
                targetBase = prevBase;
                while (transform.position != myNav.destination)
                {
                    yield return null;
                }
                
                //Returned to base
                onBase = true;
                flyRetreat = false;
                retreat = false;
            }

            //User intervention
            else if(stall)
            {
                //Stop baserunner in their tracks
                myNav.isStopped = true;
            }
            else if (retreat) //Go to previous base
            {
                myNav.isStopped = false;
                myNav.destination = Ballpark.i.basePos[lastBaseTouched].position;
                targetBase = lastBaseTouched;
                //while (transform.position != myNav.destination)
                //{
                //    yield return null;
                //}
                //retreat = false;

            }
            else if(advance) //Advance to the next base
            {
                myNav.isStopped = false;
                targetBase = lastBaseTouched + 1;
                myNav.destination = Ballpark.i.basePos[targetBase].position;

            }

                yield return null;
        }
        
        
    }

}
