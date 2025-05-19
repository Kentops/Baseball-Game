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

    // Start is called before the first frame update
    void Start()
    {
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
        Ballpark.deadBall += resetFielders;
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

    
}
