using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName;
    public int state; //0 = batting, 1 is pitching, 2 fielding, 3 baserunning

    private Batter myBatter;
    private Pitcher myPitcher;
    private Fielder myFielder;
    private Runner myRunner;

    // Start is called before the first frame update
    void Start()
    {
        myBatter = GetComponent<Batter>();
        myPitcher = GetComponent<Pitcher>();
        myFielder = GetComponent<Fielder>();
        myRunner = GetComponent<Runner>();
        Ballpark.ballHit += ballHitResponse;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeState(int newState)
    {
        if(newState == 0)
        {
            myPitcher.enabled = false;
            myFielder.enabled = false;
            myRunner.enabled = false;

            myBatter.enabled = true;
            state = 0;
        }
        else if(newState == 1)
        {
            myBatter.enabled = false;
            myFielder.enabled = false;
            myRunner.enabled = false;

            myPitcher.enabled = true;
            state = 1;
        }
        else if(newState == 2)
        {
            myBatter.enabled = false;
            myRunner.enabled = false;
            myPitcher.enabled = false;

            myFielder.enabled = true;
            state = 2;
        }
        else
        {
            myBatter.enabled = false;
            myPitcher.enabled = false;
            myFielder.enabled = false;

            myRunner.enabled = true;
            state = 3;
        }
    }

    public void ballHitResponse()
    {
        StartCoroutine("delay");
    }

    IEnumerator delay()
    {
        if (state == 1)
        {
            changeState(2);
            yield return new WaitForSeconds(0.1f);
            myFielder.getLiveBall();
        }
    }
}
