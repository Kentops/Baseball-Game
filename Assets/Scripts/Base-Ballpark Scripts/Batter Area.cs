using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterArea : MonoBehaviour
{
    //A trigger collider on the plate determines if the ball crossed. Used for batters and for balls/strikes

    public bool isStrike;
    [SerializeField] private bool isOuterZone; //Check if is outermost zone

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            isStrike = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            isStrike = false;
        }
    }

    private void clearStrike() //Clear values when ball deleted
    {
        isStrike = false;
    }

    private void onReset()
    {
        transform.localPosition = Vector3.zero; //Default position;
        if(isOuterZone)
        {
            //Reset to default size (instead of modified zone via handedness)
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnEnable()
    {
        Ballpark.deadBall += clearStrike;
        Ballpark.resetField += onReset;
    }
    private void OnDisable()
    {
        Ballpark.deadBall -= clearStrike;
        Ballpark.deadBall -= onReset;
    }

}

