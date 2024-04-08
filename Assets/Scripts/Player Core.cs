using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    //The base class of all players
    public float speed;
    public Ballpark currentField;

    // Start is called before the first frame update
    void Start()
    {
        //This is overridden
        currentField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
