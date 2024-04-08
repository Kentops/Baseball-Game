using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class StrikeIdentifier : MonoBehaviour
{
    public StrikeZone strikeCheck;
    public bool wasStrike; //False means ball

    private Ballpark theField;

    // Start is called before the first frame update
    void Start()
    {
        theField = GameObject.FindGameObjectWithTag("Field").GetComponent<Ballpark>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if(other.gameObject.GetComponent<BaseBall>().grounded == true)
            {
                wasStrike = false;
                Debug.Log("Ball!");
            }
            else if (strikeCheck.isStrike == true)
            {
                wasStrike = true;
                Debug.Log("Strike!");
            }
            else
            {
                wasStrike = false;
                Debug.Log("Ball!");
            }
            strikeCheck.isStrike = false;
            StrikeZone.theBall = null;
            theField.removeTheBall();
            //Report it below
        }

    }
}
