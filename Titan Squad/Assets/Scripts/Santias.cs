using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Santias : PlayerUnit
{
    const int NVB_DURATION = 2;
    // Start is called before the first frame update
    override
    protected void Start()
    {
        base.Start();

        abilityNames[0] = "Night Vision Booster";
        abilityNames[1] = "Grapple Move";
        abilityNames[2] = "Grapple Jump";

        abilityDescriptions[0] = "Santias gives a boost to allied night vision, increasing sight range for " + NVB_DURATION + " turns.";
        abilityDescriptions[1] = "Santias shoots out his grappling hook to a nearby obstacle, pulling himself adjacent to it.";
        abilityDescriptions[2] = "Santias shoots out his grappling hook to a nearby obstacle and uses the momentum to toss himself over it.";

    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }

    //Night Vision Booster
    override
    public void ability1()
    {
        StartCoroutine(nightVisionBooster());
        useActionPoint(1);
    }

    //Grapple Move
    override
    public void ability2()
    {
        useActionPoint(1);
    }

    //Grapple Jump
    override
    public void ability3()
    {
        useActionPoint(1);
    }

    IEnumerator nightVisionBooster()
    {
        usingAbility1 = true;
        int endTurn = GameManager.instance.turnCount + NVB_DURATION;

        foreach (PlayerUnit ally in Level.instance.playerUnits)
            ally.swapVision();

        while (GameManager.instance.turnCount != endTurn)
            yield return null;
        while (!GameManager.instance.playerPhase)
            yield return null;

        foreach (PlayerUnit ally in Level.instance.playerUnits)
            ally.swapVision();

        usingAbility1 = false;
    }
}
