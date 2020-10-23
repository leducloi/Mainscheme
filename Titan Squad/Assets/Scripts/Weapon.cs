using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public int damage;
    public int effectiveRange;
    public int maxRange;
    public int noise;

    public string type;

    public Weapon(string name)
    {
        type = name;

        if (name.Equals("Pistol"))
        {
            damage = 100;
            effectiveRange = 5;
            maxRange = 6;
            noise = 2;
        }
        else
        {
            damage = 10;
            effectiveRange = 20;
            maxRange = 30;
            noise = 5;
        }
    }
}
