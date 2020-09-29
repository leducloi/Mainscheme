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
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    override
    public void cutscene()
    {
        //TODO
    }
}
