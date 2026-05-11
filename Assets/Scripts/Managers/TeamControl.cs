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

            currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
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
        Debug.Log("Fielders reset");
        for(int i = 0; i <9; i++)
        {
            homeTeam[i].gameObject.transform.position = currentField.fieldPos[i+1].position;
            if (i == 0)
            {
                homeTeam[i].changeState(1);
            }
            else
            {
                homeTeam[i].changeState(2);
            }
        }

        awayTeam[0].changeState(0);
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

        //Batter
        GameObject bat = Instantiate(awayTeamPrefab[0]);
        awayTeam[0] = bat.GetComponent<Player>();

        yield return new WaitForSeconds(0.1f); //Delay
        resetFielders();
    }

    private void OnEnable()
    {
        Ballpark.resetField += resetFielders;
    }

    private void OnDisable()
    {
        Ballpark.resetField -= resetFielders;
    }


}
