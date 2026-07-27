using Unity.VisualScripting;
using UnityEngine;

public class Base : MonoBehaviour
{
    public int id; //1 is first, 4 is home
    public Runner runnerOn;
    public Fielder defenderOn;

    private bool canForce = true;//Prevent duplicates
    private bool fairBallCalled; //Allows scoring

    private void Update()
    {
        //Check if our defender has the ball for a force out
        if (canForce && defenderOn != null && defenderOn.holdingBall && runnerOn == null)
        {
            if(TeamControl.i.runnersInPlay.Count >= id) //There are enough runners for a possible force out on this base
            {
                if(id == 1)
                {
                    if (TeamControl.i.runnersInPlay[TeamControl.i.runnersInPlay.Count - 1].GetComponent<Runner>().lastBaseTouched == 0)
                    {
                        TeamControl.i.runnersInPlay[TeamControl.i.runnersInPlay.Count - 1].onOut();
                        canForce = false;
                    }
                }

                else if (TeamControl.i.previousRunners[id - 2] != null //If forced runner has not touched our base
                    && TeamControl.i.previousRunners[id - 2].GetComponent<Runner>().lastBaseTouched == id - 1)
                {
                    TeamControl.i.runnersInPlay[TeamControl.i.runnersInPlay.Count - id].onOut();
                    canForce = false;
                }
            }

            //Check for tag-up
            if (id != 4 && TeamControl.i.previousRunners[id-1] != null
                && TeamControl.i.previousRunners[id-1].GetComponent<Runner>().flyRetreat) //They haven't retreated back yet
            {
                Debug.Log("TagUp");
                TeamControl.i.previousRunners[id - 1].GetComponent<Runner>().onOut();
                canForce = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other) //Will use to determine safe/out
    {
        if(other.tag.Equals("Player") && other.GetComponent<Runner>().enabled)
        {
            //Runner has entered base, check if they may stay or we kick them out

            //See if defender is on with ball
            if (defenderOn != null && defenderOn.holdingBall)
            {
                other.GetComponent<Runner>().onOut();
                return;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player") && other.GetComponent<Runner>().enabled)
        {
            //Runner has left base
            if(runnerOn != null) //In case it was deleted
            {
                runnerOn.onBase = false;
            }
            runnerOn = null;
            if(id < 4)
            {
                TeamControl.i.safeRunners[id - 1] = null;
            }

            //Nothing for home base
        }
        else if(other.tag.Equals("Player") && other.GetComponent<Fielder>().enabled) //Defender leaves
        {
            defenderOn = null;
        }
    }

    //Check to add a runner or fielder
    private void OnTriggerStay(Collider other) //Occurs for each collider each frame
    {
        //A runner is hovering
        if(other.tag.Equals("Player") && other.GetComponent<Runner>().enabled) //Don't add runner to home
        {
            if(id != 4)
            {
                //Add runner to base if able
                if (runnerOn == null)
                {
                    TeamControl.i.safeRunners[id - 1] = other.GetComponent<Player>();
                    runnerOn = other.GetComponent<Runner>();
                    runnerOn.onReachBase(id);
                }
            }
            else //Wait until we are eligible to score
            {
                Runner theRunner = other.GetComponent<Runner>();
                if(fairBallCalled && theRunner.flyRetreat == false)
                {
                    theRunner.onScore();
                }
            }
        }

        //A defender is hovering
        else if(other.tag.Equals("Player") && other.GetComponent<Fielder>().enabled)
        {
            //Add fielder to base if able, give priority to the one with the ball
            Fielder newcomer = other.GetComponent<Fielder>();
            if (defenderOn == false || newcomer.holdingBall == true)
            {
                defenderOn = newcomer;
            }
        }
        
    }

    private void clearBase()
    {
        runnerOn = null;
        defenderOn = null;
        canForce = true;
        fairBallCalled = false;
    }

    private void onFair()
    {
        fairBallCalled = true;
    }

    private void OnEnable()
    {
        Ballpark.resetField += clearBase;
        Ballpark.fairBall += onFair;
    }

    private void OnDisable()
    {
        Ballpark.resetField -= clearBase;
        Ballpark.fairBall += onFair;
    }
}

