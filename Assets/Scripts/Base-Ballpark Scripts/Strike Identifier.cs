using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeIdentifier : MonoBehaviour
{
    //Calls balls and strikes behind the batter

    public StrikeZone strikeCheck;
    public bool wasStrike; //False means ball

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if (strikeCheck.isStrike == true)
            {
                wasStrike = true;
                Debug.Log("Strike!");
                //Report it
                ScoreKeeper.i.callStrike();
            }
            else
            {
                wasStrike = false;
                Debug.Log("Ball!");
                ScoreKeeper.i.callBall();
            }
            strikeCheck.isStrike = false;
            Ballpark.deadBall();

            //Report it below
            ScoreKeeper.i.checkForEvent();
        }

    }
}
