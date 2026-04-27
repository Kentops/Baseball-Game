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
                //If base defender moved off base by player, remove them from defensive scheme
                if (teams.homeTeam[i].GetComponent<Fielder>().offPosition == true)
                {
                    removeBaseDefender(i);
                }
    
                //Times when we do not want fielders to be reassigned
                if (teams.homeTeam[i].GetComponent<Fielder>().pursueTarget >=9 && ballInfo.isHeld != 0 //If you are on a base and the ball is held, stay
                    || teams.homeTeam[i].GetComponent<Fielder>().holdingBall) //If you are holding the ball, stay
                {
                    continue;
                }


                if (i == closest && ballInfo.isHeld == 0) //Abandon base and cover ball
                {
                    teams.homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = -1;
                    //Remove them from being a base defender if covering
                    removeBaseDefender(i);
                }

                else if(isBaseDefender(i)) //TESTING
                {
                    //I am defending, do nothing
                    continue;
                }

                else
                {
                    //Defense!
                    if (i >= 1 && i <= 4)
                    {
                        if (currentField.baseDefenders[i - 1] == null) //Basemen go to base if base is empty
                        {
                            teams.homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = i + 9;
                            currentField.baseDefenders[i - 1] = teams.homeTeam[i].transform.GetComponent<Fielder>();
                        }
                        else
                        {
                            //Base being defended, go to default (So they don't chase after someone has the ball)
                            teams.homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = i + 1;
                        }

                    }
                    else if (i == 5 || i == 0)
                    {
                        //Shortstop and pitcher - go to normal spots
                        teams.homeTeam[i].transform.GetComponent<Fielder>().pursueTarget = i+1;

                        //Remove them from being a base defender if covering
                        removeBaseDefender(i);
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
                            teams.homeTeam[i].GetComponent<Fielder>().pursueTarget = i+1;
                        }

                    }

                }
            }
            //Special cases
            if (closest <= 5 && !isBaseDefender(closest)) //Closest is not on the base
            {
                if (closest == 1 || closest == 2)
                {
                    //Pitcher covers home and first
                    removeBaseDefender(0); //Remove if already a defender
                    teams.homeTeam[0].transform.GetComponent<Fielder>().pursueTarget = closest + 9;
                    currentField.baseDefenders[closest - 1] = teams.homeTeam[0].transform.GetComponent<Fielder>();

                    //Tell other fielder to go to default position (if they have the ball already)
                    if (teams.homeTeam[closest].GetComponent<Fielder>().holdingBall)
                    {
                        teams.homeTeam[closest].GetComponent<Fielder>().pursueTarget = closest;
                    }
                }
                else if (closest == 3 || closest == 4)
                {
                    //Short stop covers second and third
                    removeBaseDefender(5);
                    teams.homeTeam[5].transform.GetComponent<Fielder>().pursueTarget = closest + 9;
                    currentField.baseDefenders[closest - 1] = teams.homeTeam[5].transform.GetComponent<Fielder>();

                    if (teams.homeTeam[closest].GetComponent<Fielder>().holdingBall)
                    {
                        teams.homeTeam[closest].GetComponent<Fielder>().pursueTarget = closest;
                    }
                }
            }


            yield return new WaitForSeconds(0.5f);
        }

    }

    private bool isBaseDefender(int id) //Checks if fielder {id} is a base defender
    {
        for (int j = 0; j < 4; j++)
        {
            if (currentField.baseDefenders[j] == teams.homeTeam[id].GetComponent<Fielder>())
            {
                return true;
            }
        }
        //else
        return false;
    }

    private void removeBaseDefender(int id) //If fielder {id} is a base defender, remove them from it
    {
        for (int j = 0; j < 4; j++)
        {
            if (currentField.baseDefenders[j] == teams.homeTeam[id].GetComponent<Fielder>())
            {
                currentField.baseDefenders[j] = null;
            }
        }
    }
}
