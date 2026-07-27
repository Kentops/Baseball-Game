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

    public int homeLineupPos = 1;
    public int awayLineupPos = 1;

    public Player[] defense = new Player[9];
    public Player[] safeRunners = new Player[3]; //0 is first, etc.
    public Player[] previousRunners = new Player[3]; //Runners at start of play 
    public List<Runner> runnersInPlay;
    public Player currentHitter;

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
        awayLineupPos = (awayLineupPos % 9) + 1; //Next batter will be new
    }

    private void onFoul()
    {
        lastPlayFoul = true;
    }
    #endregion

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
                if (previousRunners[i] != null)
                {
                    safeCopy[i] = previousRunners[i].posInLineup;
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
        previousRunners = (Player[])safeRunners.Clone(); //Previous runners becomes a clone
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
