using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This is an example script of how levels are to be created. All levels will inherit the Level class which has some
 * functionality already done that will be common to all levels. The specifics of each level come from which enemies
 * it contains, the locations for the player to place their units on the start, and the into/outro cutscenes (time willing)
 * 
 * 
 * ---------------------CURRENTLY TODO---------------------
 */

public class SampleLevel : Level
{
    // Start is called before the first frame update
    protected override void Start()
    {
        activeObjectives = new List<GameObject>();
        activeObjectives.Add(objectives[0]);

        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    override
    protected void levelCompleted()
    {
        base.levelCompleted();
    }


    override
    public IEnumerator cutscene()
    {
        //TODO
        yield return null;
    }

    
}
