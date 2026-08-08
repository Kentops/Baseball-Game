using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TeamControl : MonoBehaviour
{
    //Script that controls the teams. Where players are positioned and when. Stacks the pins.

    [SerializeField] private GameObject[] homeFieldPrefab; //Determines field position (P,C,1B...)
    [SerializeField] private GameObject[] homeBatPrefab; //Determines batting order (0,1,2...)

    [SerializeField] private GameObject[] awayFieldPrefab;
    [SerializeField] private GameObject[] awayBatPrefab;

    [SerializeField] private int homeLineupPos = 1;
    [SerializeField] private int awayLineupPos = 1;
    public bool sceneReady = true; //Set to false elsewhere, lets others know when to stop a loading screen

    public Player[] defense = new Player[9];
    public Player[] safeRunners = new Player[3]; //0 is first, etc.

    public Player [] previousRunners = new Player[3]; //Runners at start of play
    public int[] previousRunnersId = new int[3]; //Runners at start of play (based on lineup index)

    public List<Runner> runnersInPlay;
    public Player currentHitter;
    public Player currentPitcher;

    private bool lastPlayFoul = false;

    public static TeamControl i;

    // Start is called before the first frame update
    void Start()
    {
        if (i == null) //Singleton
        {
            i = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        defense = new Player[9];

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartCoroutine(spawnPlayers());
        }
    }

    #region Triggered Events
    public void onResetField()
    {
        //Sets offense and defense to their default position. Loads batter and pitcher.
        StartCoroutine(resetFieldRoutine());
    }

    private void spawnBatter()
    {
        GameObject bat = Instantiate(awayBatPrefab[awayLineupPos-1]);
        currentHitter = bat.GetComponent<Player>();
        currentHitter.posInLineup = awayLineupPos; //Tell player where in lineup they are
    }

    private void onFairBall() //Ball is in play, next batter will be someone new
    {
        currentHitter = null;
        progressLineup(false); //Next batter will be new
    }

    private void onFoul()
    {
        lastPlayFoul = true;
    }
    #endregion

    public void progressLineup(bool home)
    {
        if(home)
        {
            homeLineupPos = (homeLineupPos % 9) + 1;
        }
        else
        {
            awayLineupPos = (awayLineupPos % 9) + 1;
        }
    }

    public void walkBatter() //Moves players on bases to walk the batter. Scene must be reset to apply.
    {
        lastPlayFoul = true; //So previousRunnersId is used to decide runners
        int held = currentHitter.posInLineup;

        for (int i = 0; i < 4; i++)
        {
            if(i == 3) //Bases were loaded before walk
            {
                ScoreKeeper.i.onScore();
            }
            else if (previousRunnersId[i] != 0) //Base occupied, keep going with next base
            {
                int temp = previousRunnersId[i];
                previousRunnersId[i] = held;
                held = temp;
            }
            else //Base is free
            {
                previousRunnersId[i] = held;
                break; //end loop
            }
        }
    }

    public bool allOnBase() //returns true if every runner is on base
    {
        foreach(Runner r in runnersInPlay)
        {
            if (r.onBase == false)
            {
                return false;
            }
        }
        //All on base
        return true;
    }

    private IEnumerator spawnPlayers()
    {
        //Creates fielders from prefab
        for (int i = 0; i < 9; i++)
        {
            GameObject temp = Instantiate(homeFieldPrefab[i]);
            temp.transform.parent = transform;
            defense[i] = temp.GetComponent<Player>();

        }
        spawnBatter();

        yield return new WaitForSeconds(0.1f); //Delay
        onResetField();
    }

    private IEnumerator resetFieldRoutine()
    {
        Debug.Log("Field reset");
        //Defense
        for (int i = 0; i < 9; i++)
        {
            defense[i].gameObject.transform.position = Ballpark.i.fieldPos[i + 1].position;
            if (i == 0)
            {
                defense[i].changeState(1); //Make pitcher
                while (defense[0].GetComponent<Pitcher>().enabled == false)
                {
                    yield return null;
                }
                currentPitcher = defense[i];
                //defense[i].GetComponent<Pitcher>().canMove = false; //Eventually we need to prevent pitcher from pitching whenever they feel.
            }
            else
            {
                defense[i].changeState(2);
            }
        }

        //Deal with runners
        int[] safeCopy = {0,0,0}; //List of who's safe based on prefab
        for(int i = 0; i < safeRunners.Length; i++)
        {
            if(lastPlayFoul)
            {
                if (previousRunnersId[i] != 0)
                {
                    safeCopy[i] = previousRunnersId[i];
                    safeRunners[i] = null;
                }
            }
            else //Last play was fair
            {
                if (safeRunners[i] != null)
                {
                    safeCopy[i] = safeRunners[i].posInLineup;
                    safeRunners[i] = null;
                }
            }
        }

        int runnerCount = runnersInPlay.Count; //We want the number of elements in the list at the start, not after removals.
        for (int i = 0; i < runnerCount; i++) //Kill active runners
        {
            Runner temp = runnersInPlay[0];
            runnersInPlay.RemoveAt(0);
            if(currentHitter != null && temp.gameObject == currentHitter.gameObject)//Prevents missing/duplicate hitter
            {
                currentHitter = null;
            }
            Destroy(temp.gameObject);
        }
        BaseBugManager.i.removeAllRunnerBugs();
        Debug.Log("Creation");
        for(int i = 2; i >= 0; i--) //Spawn new runners on basepath (Farthest first)
        {
            if (safeCopy[i] != 0)
            {
                Player temp = Instantiate(awayBatPrefab[safeCopy[i]-1]).GetComponent<Player>();    
                temp.transform.position = Ballpark.i.basePos[i + 1].position;
                temp.changeState(3);
                temp.posInLineup = safeCopy[i];
                safeRunners[i] = temp;
                temp.GetComponent<Runner>().baseStarted = i + 1;

            }
        }
        previousRunners = (Player[])safeRunners.Clone(); //Creates an unlinked copy
        previousRunnersId = (int[])safeCopy.Clone(); 
        lastPlayFoul = false;

        //Get batter
        if (currentHitter == null) //No active batter, wait until we have one
        {
            spawnBatter();
            while (currentHitter == null)
            {
                yield return null;
            }
        }
        currentHitter.changeState(0);
        sceneReady = true;
    }

    private void OnEnable()
    {
        Ballpark.resetField += onResetField;
        Ballpark.fairBall += onFairBall;
        Ballpark.foulBall += onFoul;
    }

    private void OnDisable()
    {
        Ballpark.resetField -= onResetField;
        Ballpark.fairBall -= onFairBall;
        Ballpark.foulBall -= onFoul;
    }


}
