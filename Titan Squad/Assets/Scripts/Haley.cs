using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haley : PlayerUnit
{
    const int COMBAT_DRUGS_DURATION = 3;
    const int HEALING_AMOUNT = 10;
    const int GLOVE_RANGE = 5;

    // Start is called before the first frame update
    override
    protected void Start()
    {
        base.Start();


        abilityNames[0] = "Combat Drugs";
        abilityNames[1] = "Medical Tincture";
        abilityNames[2] = "Hit & Run";

        abilityDescriptions[0] = "Haley fires a powerful mixture into an ally, increasing their combat capabilities for " + COMBAT_DRUGS_DURATION + " turns.";
        abilityDescriptions[1] = "Haley jams a potent herbal mixture into an ally, healing them for " + HEALING_AMOUNT + " hitpoints.";
        abilityDescriptions[2] = "Haley makes an attack with her Glove of Power and empowers all allies within " + GLOVE_RANGE + " tiles of the enemy hit with the ability to move.";

    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }

    //Tracking Dart
    override
    public void ability1()
    {
        useActionPoint(1);
    }

    //Sound Dart
    override
    public void ability2()
    {
        useActionPoint(1);
    }

    //Sleep Dart
    override
    public void ability3()
    {
        useActionPoint(1);
    }

}
