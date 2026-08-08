using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class BaseBugManager : MonoBehaviour
{
    //Controls base runners on the minimap
    public static BaseBugManager i;

    [SerializeField] private RectTransform[] basePositions; //1 is first, 0/4 is home
    [SerializeField] private Image[] baseGraphics;
    [SerializeField] private List<RectTransform> runnerGraphics;
    [SerializeField] private Base[] bases; //1 is first, 0/4 is home

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(i != null) { Destroy(gameObject); }
        else { i = this; }
    }

    //Update is called once per frame
    void Update()
    {
        //Look which bases are occupied and move the runner graphics
        for (int i = 0; i < 4; i++)
        {
            if (bases[i].runnerOn != null)
            {
                baseGraphics[i].color = Color.yellow;
            }
            else
            {
                baseGraphics[i].color = Color.white;
            }
        }

        int index = 0; //So we know to pair with our runner.
        foreach (RectTransform r in runnerGraphics)
        {
            Runner theRunner = TeamControl.i.runnersInPlay[index];
            //Cases we don't need to calculate
            if(r.position == basePositions[theRunner.targetBase].position) { index++;  continue; } //icon on runner's target base
            else if(theRunner.retreat && theRunner.targetBase > theRunner.lastBaseTouched) { index++;  continue; } //Hasn't retreated


                float percent = runProgress(theRunner);
            if (percent > 0.98f)
            {
                r.position = basePositions[theRunner.targetBase].position;
            }
            else
            {
                Vector2 sum;
                if(theRunner.retreat) //Account for retreating little guys
                {
                    sum = (basePositions[theRunner.targetBase].position * percent) + (basePositions[theRunner.targetBase+1].position * (1 - percent));
                }
                else
                {
                    //runner goes forward
                    sum = (basePositions[theRunner.targetBase].position * percent) + (basePositions[theRunner.lastBaseTouched].position * (1 - percent));
                }
                   
                r.position = sum;
            }

            index++;
        }
    }

    public void createRunnerBug(int theBase, GameObject icon) //Create runner at given base (home is 0)
    {
        //Set position and add to lists
        RectTransform temp = Instantiate(icon, transform).GetComponent<RectTransform>();
        temp.position = basePositions[theBase].position;
        runnerGraphics.Add(temp);
    }

    public void removeRunnerBug(int index)
    {
        Destroy(runnerGraphics[index].gameObject);
        runnerGraphics.RemoveAt(index);
    }

    public void removeAllRunnerBugs()
    {
        foreach(RectTransform r in runnerGraphics)
        {
            Destroy(r.gameObject);
        }
        runnerGraphics.Clear();
        Debug.Log("removal");
    }

    private float runProgress(Runner theRunner)
    {
        //returns the percentage the baseRunner is to the base
        float targetMag, originMag, currentMag;

        if(theRunner.retreat == true) //Sometimes a frame where they retreat but target base is still ahead.
        {
            //target and last based touched will be same. Calculate differently.
            originMag = Ballpark.i.basePos[theRunner.targetBase+1].position.magnitude;
        }
        else
        {
            originMag = Ballpark.i.basePos[theRunner.lastBaseTouched].position.magnitude;
        }
        //Calculate magnitudes
        targetMag = Ballpark.i.basePos[theRunner.targetBase].position.magnitude;
        currentMag = theRunner.transform.position.magnitude;

        return Math.Abs(currentMag - originMag) / Math.Abs(targetMag - originMag);
    }

    private void OnEnable()
    {
        
        //StartCoroutine(moveIcons());
    }
    private void OnDisable()
    {

        //StopCoroutine(moveIcons());
    }

    //private IEnumerator moveIcons() //Running this in update seems too processor heavy
    //{
    //    while(true)
    //    {
            

    //        yield return new WaitForSeconds(Time.deltaTime);
        
    //    }
    //}
}
