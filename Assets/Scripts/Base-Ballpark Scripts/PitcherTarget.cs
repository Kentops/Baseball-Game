using System.Collections;
using UnityEngine;

public class PitcherTarget : MonoBehaviour
{
    //This is the point where a pitcher throws to or a batter swings at.

    Transform defaultPitchPoint;

    private void setPosition()
    {
        //Returns to default position after resets
        if (defaultPitchPoint != null)
        {
            transform.position = defaultPitchPoint.position;
        }
        else
        {
            StartCoroutine(waitForLoad());
        }
    }

    private void OnEnable()
    {
        setPosition();
        Ballpark.resetField += setPosition;
    }

    private void OnDisable()
    {
        Ballpark.resetField -= setPosition;
    }

    private IEnumerator waitForLoad()
    {
        yield return new WaitForSeconds(0.3f);
        defaultPitchPoint = Ballpark.i.pitchPoints[0].transform;
        transform.position = defaultPitchPoint.position;
    }

    //Maybe add a function to match height with batter
}
