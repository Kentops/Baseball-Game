using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Runner : MonoBehaviour
{
    public bool onBase = false;
    public int baseStarted; //0 is none. Given at spawn by team control
    public int lastBaseTouched; //So our runners stay in base lines when retreating
    public bool reachedBase = false;
    public bool flyRetreat; //Back to initial base

    [Header("Skill Fields")]
    public float speed;

    private NavMeshAgent myNav;
    private bool retreat; //Back to last base
    private bool isOut;


    public void startRunning()
    {
        reachedBase = false;
        StartCoroutine(runCoroutine());
    }

    public void onReachBase(int baseNum)
    {
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
        TeamControl.i.runnersInPlay.Remove(this);

        //Delete once we reach dugout
        StartCoroutine(leaveCoroutine());
    }

    public void onScore()
    {
        ScoreKeeper.i.onScore();
        TeamControl.i.runnersInPlay.Remove(this);
        isOut = true; //Want no one calling us out
        StartCoroutine(leaveCoroutine());
    }

    public void onFlyOut() //Someone else's fly out
    {
        if(isOut) { return; }
        flyRetreat = true;
    }

    public void onFoulBall()
    {
        StopAllCoroutines();
        myNav.destination = transform.position;
    }

    private void OnEnable()
    {
        Ballpark.ballHit += startRunning;
        Ballpark.flyOut += onFlyOut;
        Ballpark.foulBall += onFoulBall;

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
        reachedBase = false;
    }

    private void OnDisable()
    {
        Ballpark.ballHit -= startRunning;
        Ballpark.flyOut -= onFlyOut;
        Ballpark.foulBall -= onFoulBall;

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
        myNav.destination = Ballpark.i.basePos[baseStarted + 1].position;
        while (transform.position != myNav.destination && !retreat && !flyRetreat) //Keep going towards next base
        {
            yield return null;
        }

        //We reached the next base, now what?
        while(true)
        {
            if (flyRetreat) //Fly ball caught
            {
                int prevBase = lastBaseTouched;
                while (prevBase != baseStarted)
                {
                    myNav.destination = Ballpark.i.basePos[prevBase].position;
                    while (transform.position != myNav.destination)
                    {
                        yield return null;
                    }
                    //reached previous base
                    prevBase--;
                }
                //Once more now that we know prevBase is our starting base
                myNav.destination = Ballpark.i.basePos[prevBase].position;
                while (transform.position != myNav.destination)
                {
                    yield return null;
                }
                Debug.Log("Back at base");
                flyRetreat = false;
                retreat = false;
            }

            else if (retreat) //Go to previous base
            {
                myNav.destination = Ballpark.i.basePos[lastBaseTouched].position;
                while (transform.position != myNav.destination)
                {
                    yield return null;
                }

            }

            yield return null;
        }
        
        
    }

}
