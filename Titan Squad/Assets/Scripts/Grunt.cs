using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grunt : EnemyUnit
{
    // Start is called before the first frame update
    override
    protected void Start()
    {
        base.Start();

        weapons[0] = new Weapon("Krimbar Rifle");
        weapons[1] = new Weapon("--");

        hpMax = 8;
        hpRemaining = 8;

        shieldMax = 3;
        shieldRemaining = 3;

        movement = 5;

        combatTraining = 20;
        evasiveTactics = 5;
        bionicEnhancement = 2;
        luck = 0;
        criticalTargeting = 3;
        advancedShielding = 2;
    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }
}
