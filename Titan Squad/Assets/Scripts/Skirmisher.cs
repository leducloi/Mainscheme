using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skirmisher : EnemyUnit
{
    // Start is called before the first frame update
    override
    protected void Start()
    {
        weapons[0] = new Weapon("Krimbar Power Sword");
        weapons[1] = new Weapon("--");

        base.Start();

        takesCover = false;

        hpMax = 5;
        hpRemaining = 5;

        shieldMax = 2;
        shieldRemaining = 2;

        movement = 6;
        
        combatTraining = 15;
        evasiveTactics = 20;
        bionicEnhancement = 5;
        luck = 6;
        criticalTargeting = 2;
        advancedShielding = 0;
}

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }
}
