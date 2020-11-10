using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public int damage;
    public int techDamage;
    public int effectiveRange;
    public int maxRange;
    public int minRange;
    public int hitChance;

    public string type;

    public Weapon(string name)
    {
        type = name;

        switch (name)
        {
            case "Pistol":
                damage = 5;
                techDamage = 0;
                effectiveRange = 5;
                maxRange = 6;
                minRange = 1;
                hitChance = 75;
                break;
            case "Glove of Power":
                damage = 5;
                techDamage = 0;
                effectiveRange = 5;
                maxRange = 6;
                minRange = 1;
                hitChance = 75;
                break;
            case "Railgun":
                damage = 10;
                techDamage = 0;
                effectiveRange = 12;
                maxRange = 12;
                minRange = 6;
                hitChance = 75;
                break;
            case "Grenade Launcher":
                damage = 8;
                techDamage = 3;
                effectiveRange = 10;
                maxRange = 10;
                minRange = 5;
                hitChance = 75;
                break;
            case "Energy Blade":
                damage = 12;
                techDamage = 8;
                effectiveRange = 1;
                maxRange = 1;
                minRange = 1;
                hitChance = 85;
                break;
            case "V1rus":
                damage = 3;
                techDamage = 6;
                effectiveRange = 6;
                maxRange = 7;
                minRange = 1;
                hitChance = 75;
                break;
            case "Krimbar Rifle":
                damage = 1;
                techDamage = 0;
                effectiveRange = 6;
                maxRange = 10;
                minRange = 3;
                hitChance = 75;
                break;
            case "Krimbar Power Sword":
                damage = 8;
                techDamage = 2;
                effectiveRange = 1;
                maxRange = 1;
                minRange = 1;
                hitChance = 85;
                break;
            default:
                damage = 0;
                techDamage = 0;
                effectiveRange = 0;
                maxRange = 0;
                minRange = 0;
                break;

        }
        
    }
}
