using Unity.VisualScripting;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static ScoreKeeper i;
    public int balls = 0;
    public int strikes = 0;
    public int outs = 0;
    public bool canChange;

    public int runsHome;
    public int runsAway;

    [SerializeField] private GameObject display;
    [SerializeField] private GameObject sceneCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Implement Singleton
        if(i==null)
        {
            i = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void checkForEvent()
    {
        //Checks the count to see if something should happen

        if(strikes > 2) //Strikeout
        {
            TeamControl.i.progressLineup(false); //Move lineup
            cleanCount();
            PlayEventManager.i.onStrikeout();
            callOut();
        }
        else if(balls > 3)
        {
            TeamControl.i.progressLineup(false);
            cleanCount();
            TeamControl.i.walkBatter();
            Instantiate(display, sceneCanvas.transform).GetComponent<PopupText>().displayText("WALK", 2);
            PlayEventManager.i.onStrikeout(); //Same from code perspective at this point
        }
    }

    public void callStrike()
    {
        strikes += 1;
        Instantiate(display, sceneCanvas.transform).GetComponent<PopupText>().displayText("STRIKE", 1); //Display message
        checkForEvent();

        //Pitcher ought to be given a cooldown after this
    }
    public void callBall()
    {
        balls += 1;
        Instantiate(display, sceneCanvas.transform).GetComponent<PopupText>().displayText("BALL", 1); //Display message
        checkForEvent();
    }
    public void callOut()
    {
        outs += 1;
        Instantiate(display, sceneCanvas.transform).GetComponent<PopupText>().displayText("OUT", 2); //Display message
        checkForEvent();
    }

    public void onScore()
    {
        runsAway += 1;
        Instantiate(display, sceneCanvas.transform).GetComponent<PopupText>().displayText($"{runsAway} - {runsHome}", 2);
    }

    private void onFoul()
    {
        if(strikes < 2)
        {
            strikes += 1;
        }
        canChange = false;
        Instantiate(display, sceneCanvas.transform).GetComponent<PopupText>().displayText("FOUL", 2); //Display message
    }

    private void onReset()
    {
        canChange = true;
    }

    private void cleanCount()
    {
        strikes = 0;
        balls = 0;
    }

    private void OnEnable()
    {
        Ballpark.foulBall += onFoul;
        Ballpark.resetField += onReset;
        Ballpark.fairBall += cleanCount;
    }
    private void OnDisable()
    {
        Ballpark.foulBall -= onFoul;
        Ballpark.resetField -= onReset;
        Ballpark.fairBall -= cleanCount;
    }
}
