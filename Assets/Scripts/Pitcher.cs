using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : PlayerCore
{
    public Transform pitchPoint;
    public Transform releasePoint;
    public GameObject ball;
    public float upward;

    [SerializeField] private float pitchSpeed;
    [SerializeField] private float accuracy;

    private GameObject liveBall;

    // Start is called before the first frame update
    void Start()
    {
        //Will be overridden, remove later
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (liveBall != null) { currentField.removeTheBall(); }
            pitch();
        }
    }

    private void pitch()
    {

        //Random Position
        Vector3 targetPos = pitchPoint.position;
        targetPos.y += upward;
        targetPos.y *= 1 + Random.Range(accuracy - 1, 1 - accuracy);
        targetPos.z *= 1 + Random.Range(accuracy - 1, 1 - accuracy);
        Vector3 pitchDirection = targetPos - transform.position; //Relative position vector

        //Random speed
        float ballSpeed = pitchSpeed * Random.Range(0.85f, 1.15f);

        liveBall = Instantiate(ball, releasePoint.position, Quaternion.identity);
        liveBall.GetComponent<Rigidbody>().velocity = pitchDirection * ballSpeed; //Make it normalized later

        Debug.Log(liveBall);
        currentField.currentBall = liveBall;
    }
}
