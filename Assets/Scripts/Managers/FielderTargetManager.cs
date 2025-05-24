using System.Collections;
using UnityEngine;

public class FielderTargetManager : MonoBehaviour
{
    [SerializeField] private TeamControl teams;
    private Ballpark currentField;
    private GameObject theBall;
    private BaseBall ballInfo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        Ballpark.ballHit += assignFielderTargets;
    }

    private void assignFielderTargets()
    {
        StartCoroutine("fielderTargets");
        theBall = currentField.currentBall;
        ballInfo = theBall.GetComponent<BaseBall>();
    }

    private IEnumerator fielderTargets()
    {
        yield return new WaitForSeconds(1);
        while (theBall != null)
        {
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

            //Find closest fielder
            for (int i = 0; i < 9; i++)
            {
                if ((teams.homeTeam[i].transform.position - targetPos).sqrMagnitude < (teams.homeTeam[closest].transform.position - targetPos).sqrMagnitude)
                {
                    closest = i;
                }
            }
            //Closest determined
            for (int i = 0; i < 9; i++)
            {

                //Times when we do not want fielders to be reassigned
                if(i == closest && teams.homeTeam[i].GetComponent<Fielder>().pursueTarget >=9)//If you are on a base near the ball, stay
                {
                    continue;
                }


                if (i == closest && ballInfo.isHeld == 0)
                {
                    teams.homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = -1;
                }
                else
                {
                    //Defense!
                    if (i >= 2 && i <= 5)
                    {
                        if(currentField.baseDefenders[i-2] == null) //Go to base if base is empty
                        {
                            teams.homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = i + 7;
                            currentField.baseDefenders[i - 2] = teams.homeTeam[i].transform.GetComponent<Fielder>();
                        }

                    }
                    else if (i == 6 || i == 1)
                    {
                        //Shortstop and pitcher
                        teams.homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = i;

                        //Remove them from being a base defender if covering
                        for(int j = 0; j < 4; j++)
                        {
                            if (currentField.baseDefenders[j] == teams.homeTeam[i].GetComponent<Fielder>())
                            {
                                currentField.baseDefenders[j] = null;
                            }
                        }
                    }
                    else
                    {
                        //Outfielders chase balls until held
                        if (ballInfo.isHeld == 0)
                        {
                            teams.homeTeam[i].GetComponent<Fielder>().pursueTarget = -1;
                        }
                        else
                        {
                            teams.homeTeam[i].GetComponent<Fielder>().pursueTarget = i;
                        }

                    }

                }
            }
            //Special cases
            if (closest <= 5 && teams.homeTeam[closest].transform.position != currentField.fieldPos[closest + 7].position) //Closest is not on the base
            {
                if (closest == 2 || closest == 3)
                {
                    //Pitcher covers home and first
                    teams.homeTeam[1].transform.GetComponent<Fielder>().pursueTarget = closest + 7;
                    currentField.baseDefenders[closest - 2] = teams.homeTeam[1].transform.GetComponent<Fielder>();

                    //Tell other fielder to go to default position (if they have the ball already)
                    if (teams.homeTeam[closest].GetComponent<Fielder>().holdingBall)
                    {
                        teams.homeTeam[closest].GetComponent<Fielder>().pursueTarget = closest;
                    }
                }
                else if (closest == 4 || closest == 5)
                {
                    //Short stop covers second and third
                    teams.homeTeam[6].transform.GetComponent<Fielder>().pursueTarget = closest + 7;
                    currentField.baseDefenders[closest - 2] = teams.homeTeam[6].transform.GetComponent<Fielder>();

                    if (teams.homeTeam[closest].GetComponent<Fielder>().holdingBall)
                    {
                        teams.homeTeam[closest].GetComponent<Fielder>().pursueTarget = closest;
                    }
                }
            }


            yield return new WaitForSeconds(0.5f);
        }

    }
}
