using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeZone : MonoBehaviour
{
    //A trigger collider on the plate determines if the ball crossed. Used for batters and for balls/strikes

    public bool isStrike;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            isStrike = true;
        }
    }
    //private void OnTriggerExit(Collider other)
    //{
    //    if(other.gameObject.layer == LayerMask.NameToLayer("Ball"))
    //    {
    //        isStrike = false;
    //    }
    //}

    private void clearStrike() //Clear values when ball deleted
    {
        isStrike = false;
    }

    private void OnEnable()
    {
        //Ballpark.deadBall += clearStrike;
    }
    private void OnDisable()
    {
        //Ballpark.deadBall -= clearStrike;
    }

}

