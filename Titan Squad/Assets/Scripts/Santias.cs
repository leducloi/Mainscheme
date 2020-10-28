using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Santias : PlayerUnit
{
    const int CLOAK_DURATION = 1;
    // Start is called before the first frame update
    override
    protected void Start()
    {
        base.Start();

        abilityNames[0] = "Grapple Jump";
        abilityNames[1] = "Cloak";
        abilityNames[2] = "Full Force";

        abilityDescriptions[0] = "Santias fires out his Grappling Hook to an obstacle and lands in a tile adjacent to it.";
        abilityDescriptions[1] = "Santias cloaks himself for " + CLOAK_DURATION + " turn, rendering him untargetable by enemies.";
        abilityDescriptions[2] = "Santias teleports to and attacks a nearby enemy with his Energy Blade. If this attack kills the enemy, Santias can teleport to a nearby location and make another attack.";

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
        yield return null;
    }
}
