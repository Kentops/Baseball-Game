using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName;
    public int state; //0 = batting, 1 is pitching, 2 fielding, 3 baserunning
    public int posInLineup; //Used to access prefab on offense

    private Batter myBatter;
    private Pitcher myPitcher;
    private Fielder myFielder;
    private Runner myRunner;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeState(int newState)
    {
        myBatter.enabled = false;
        myPitcher.enabled = false;
        myFielder.enabled = false;
        myRunner.enabled = false;

        if (newState == 0)
        {
            myBatter.enabled = true;
            state = 0;
        }
        else if(newState == 1)
        {
            myPitcher.enabled = true;
            state = 1;
        }
        else if(newState == 2)
        {
            myFielder.enabled = true;
            state = 2;
        }
        else
        {
            myRunner.enabled = true;
            state = 3;
        }
    }

    public void ballHitResponse()
    {
        StartCoroutine(ballHitDelay());
    }

    IEnumerator ballHitDelay()
    {
        //Pitcher -> Fielder
        if (state == 1)
        {
            changeState(2);
            yield return new WaitForSeconds(0.1f); //Delay to make sure state is changes I guess
            myFielder.getLiveBall();
        }

        //Batter -> Runner
        else if(state ==0)
        {
            changeState(3);
            myRunner.startRunning();
        }
    }

    private void OnEnable()
    {
        Ballpark.ballHit += ballHitResponse;

        myBatter = GetComponent<Batter>();
        myPitcher = GetComponent<Pitcher>();
        myFielder = GetComponent<Fielder>();
        myRunner = GetComponent<Runner>();
    }
    private void OnDisable()
    {
        Ballpark.ballHit -= ballHitResponse;
    }
}
