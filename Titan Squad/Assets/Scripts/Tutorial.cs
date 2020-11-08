using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : Level
{
    // Start is called before the first frame update
    protected override void Start()
    {
        activeObjectives = new List<GameObject>();
        foreach (GameObject objective in objectives)
        {
            if (objective.GetComponent<Objective>().type != "Exfill")
                activeObjectives.Add(objective);
        }
        isTutorial = true;

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
