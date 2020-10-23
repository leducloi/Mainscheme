using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haley : PlayerUnit
{
    const int SOUND_DART_NOISE_RANGE = 5;
    const int SLEEP_DART_DAMAGE = 2;
    const int SLEEP_DART_DURATION = 1;

    // Start is called before the first frame update
    override
    protected void Start()
    {
        base.Start();


        abilityNames[0] = "Tracker Dart";
        abilityNames[1] = "Sound Dart";
        abilityNames[2] = "Sleeper Dart";

        abilityDescriptions[0] = "Haley shoots an unnoticeable tracking dart onto an enemy. Their location is marked for 5 turns, even in Fog of War.";
        abilityDescriptions[1] = "Haley shoots a special dart onto a location in range. It creates a sound that draws enemies within " + SOUND_DART_NOISE_RANGE + " tiles.";
        abilityDescriptions[2] = "Haley shoots a dart onto a valid target. On a hit, it deals " + SLEEP_DART_DAMAGE + " points of damage and causes the target to appear dead for " + SLEEP_DART_DURATION + " turn.";


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

    }

    //Sound Dart
    override
    public void ability2()
    {

    }

    //Sleep Dart
    override
    public void ability3()
    {

    }

}
