using System.Collections;
using UnityEngine;

public class PlayEventManager : MonoBehaviour
{
    //Controls events such as foul balls, strikeouts, and balls
    public static PlayEventManager i;

    private Coroutine activePlayCountdown;

    public void resetField(float seconds = 0) //resets the field with a nice fade.
    {
        stopPlayCountdown(); //stop countdown for next play
        StartCoroutine(fieldReset(seconds));
    }

    public void onStrikeout()
    {
        StartCoroutine(strikeoutReset());
    }

    private void lockPitcherHitter() //Prevents pitches and swings
    {
        if(TeamControl.i.currentPitcher.GetComponent<Pitcher>().enabled)
        {
            TeamControl.i.currentPitcher.GetComponent<Pitcher>().lockUp();
        }
        if(TeamControl.i.currentHitter != null && TeamControl.i.currentHitter.GetComponent<Batter>().enabled)
        {
            TeamControl.i.currentHitter.GetComponent<Batter>().lockUp();
        }
    }
    private void unlockPitcherHitter()
    {
        TeamControl.i.currentPitcher.GetComponent<Pitcher>().unlock();
        TeamControl.i.currentPitcher.GetComponent<Batter>().unlock();
    }
    private void onFoulBall()
    {
        stopPlayCountdown();
        StartCoroutine(foulDelay());
    }
    private void startPlayCountdown()
    {
        if(activePlayCountdown == null)
        {
            activePlayCountdown = StartCoroutine(nextPlayCountdown());
        }
    }
    private void stopPlayCountdown()
    {
        if(activePlayCountdown != null)
        {
            StopCoroutine(activePlayCountdown);
        }
        activePlayCountdown = null;
    }

    private void OnEnable()
    {
        if(i == null)
        {
            i = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Ballpark.foulBall += onFoulBall;
        Ballpark.ballHit += startPlayCountdown;
        Ballpark.resetField += stopPlayCountdown;
    }
    private void OnDisable()
    {
        Ballpark.foulBall -= onFoulBall;
        Ballpark.ballHit -= startPlayCountdown;
        Ballpark.resetField -= stopPlayCountdown;
    }

    private IEnumerator foulDelay()
    {
        yield return new WaitForSeconds(2);
        Ballpark.deadBall();
        StartCoroutine(fieldReset());
    }

    private IEnumerator fieldReset(float s = 0)
    {
        lockPitcherHitter();
        yield return new WaitForSeconds(s); //Delay before fading out;
        FadeManager.i.fadeOut(1);
        yield return new WaitForSeconds(1);

        TeamControl.i.sceneReady = false;
        Ballpark.deadBall();
        Ballpark.resetField();
        while (TeamControl.i.sceneReady == false) //Wait until scene is ready for us
        {
            yield return null;
        }

        FadeManager.i.fadeIn(1);
        yield return new WaitForSeconds(1);
        unlockPitcherHitter();
    }

    private IEnumerator strikeoutReset() //Prevent pitching and swinging, delete batter, reset field
    {
        lockPitcherHitter();
        yield return new WaitForSeconds(2);
        FadeManager.i.fadeOut(1);
        yield return new WaitForSeconds(1);

        Destroy(TeamControl.i.currentHitter.gameObject);
        TeamControl.i.currentHitter = null;

        //Reset field
        TeamControl.i.sceneReady = false;
        Ballpark.resetField();
        while (TeamControl.i.sceneReady == false) //Wait until scene is ready for us
        {
            yield return null;
        }

        FadeManager.i.fadeIn(1);
        yield return new WaitForSeconds(1);
        unlockPitcherHitter();
    }

    private IEnumerator nextPlayCountdown() //Determines when the current play should end
    {
        //End play if ball is held and all runners are on base for two full seconds
        while(true) //Just keep going until stopped;
        {
            if(Ballpark.i.currentBall.GetComponent<BaseBall>().isHeld != 0
                && TeamControl.i.allOnBase() == true)
            {
                yield return new WaitForSeconds(2);
                if (Ballpark.i.currentBall.GetComponent<BaseBall>().isHeld != 0
                && TeamControl.i.allOnBase() == true)
                {
                    //Stop runners from advancing somehow
                    resetField();
                }
            }

            //Didn't work, wait some time before next check
            yield return new WaitForSeconds(0.25f);
        }
    }
}
