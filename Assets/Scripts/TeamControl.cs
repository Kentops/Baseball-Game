using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamControl : MonoBehaviour
{
    [SerializeField] private GameObject[] homeTeamPrefab;
    [SerializeField] private GameObject[] awayTeamPrefab;

    public Player[] homeTeam;
    public Player[] awayTeam;

    private Ballpark currentField;
    private GameObject theBall;
    private BaseBall ballInfo;

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        Ballpark.deadBall += resetFielders;
        Ballpark.ballHit += assignFielderTargets;
        homeTeam = new Player[9];
        awayTeam = new Player[9];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartCoroutine("spawnPlayers");
        }
    }

    public void resetFielders()
    {
        for(int i = 0; i <9; i++)
        {
            homeTeam[i].gameObject.transform.position = currentField.fieldPos[i].position;
            if (i == 1)
            {
                homeTeam[i].changeState(1);
            }
            else
            {
                homeTeam[i].changeState(2);
            }
        }
    }

    private void assignFielderTargets()
    {
        StartCoroutine("fielderTargets");
        theBall = currentField.currentBall;
        ballInfo = theBall.GetComponent<BaseBall>();
    }

    private IEnumerator spawnPlayers()
    {
        //Creates fielders from prefab
        for (int i = 0; i < 9; i++)
        {
            GameObject temp = Instantiate(homeTeamPrefab[i]);
            temp.transform.parent = transform;
            homeTeam[i] = temp.GetComponent<Player>();

        }
        yield return new WaitForSeconds(0.1f); //Delay
        resetFielders();
    }

    private IEnumerator fielderTargets()
    {
        yield return new WaitForSeconds(1);
        while (theBall != null)
        {
            //Find closest fielder
            Vector3 targetPos;
            int closest = 0;
            if (currentField.flyBallLanding == Vector3.zero)
            {
                //Grounder
                targetPos = currentField.currentBall.transform.position;
            }
            else
            {
                //Flyball
                targetPos = currentField.flyBallLanding;
            }
            for (int i = 0; i < 9; i++)
            {
                if ((homeTeam[i].transform.position - targetPos).sqrMagnitude < (homeTeam[closest].transform.position - targetPos).sqrMagnitude)
                {
                    closest = i;
                }
            }
            //Closest determined
            for (int i = 0; i < 9; i++)
            {
                if (i == closest && ballInfo.isHeld == 0)
                {
                    homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = -1;
                }
                else
                {
                    //Defense!
                    if (i >= 2 && i <= 5)
                    {
                        homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = i + 7;
                        currentField.baseDefenders[i - 2] = homeTeam[i].transform.GetComponent<Fielder>();
                    }
                    else if (i == 6 || i == 1)
                    {
                        //Shortstop and pitcher
                        homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = i;
                    }
                    else
                    {
                        //Outfielders chase ground balls until held
                        if (ballInfo.isHeld == 0 && currentField.flyBallLanding == Vector3.zero)
                        {
                            homeTeam[i].GetComponent<Fielder>().pursueTarget = -1;
                        }
                        else
                        {
                            homeTeam[i].GetComponent<Fielder>().pursueTarget = i;
                        }

                    }

                }
            }
            //Special cases
            if (closest <= 5 && homeTeam[closest].transform.position != currentField.fieldPos[closest + 7].position)
                //|| currentField.baseDefenders[closest-2] != homeTeam[closest] && ballInfo.isHeld == 1) //Make sure no one is already defending the base NEEDS WORK
            {
                if (closest == 2 || closest == 3)
                {
                    //Pitcher covers home and first
                    homeTeam[1].transform.GetComponent<Fielder>().pursueTarget = closest + 7;
                    currentField.baseDefenders[closest - 2] = homeTeam[1].transform.GetComponent<Fielder>();
                }
                else if (closest == 4 || closest == 5)
                {
                    //Short stop covers second and third
                    homeTeam[6].transform.GetComponent<Fielder>().pursueTarget = closest + 7;
                    currentField.baseDefenders[closest - 2] = homeTeam[6].transform.GetComponent<Fielder>();
                }
            }
           

            yield return new WaitForSeconds(0.5f);
        }

    }
}
