using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatData : MonoBehaviour
{
    public Unit unit;

    public int damage;
    public int defense;
    public int hitChance;
    public int dodgeChance;
    public int coverDodgeChance;
    public int flankedDodgeChance;
    public int maxRange;
    public int minRange;

    private const int COVER_BONUS = 30;
    private const int FLANK_PENALTY = 30;
    private const int CB_DRUGS_BONUS = 10;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int getDodgeChanceFrom(Unit attacker)
    {
        if (unit.isFlankedBy(attacker))
            return flankedDodgeChance;

        if(unit.takingCover)
            return coverDodgeChance;

        return dodgeChance;
    }

    public void calculateData()
    {
        CollisionTile tileOn = MapBehavior.instance.getTileAtPos(unit.transform.position);

        damage = unit.equippedWeapon.damage + (unit.criticalTargeting) + (unit.bionicEnhancement / 2);
        defense = unit.advancedShielding;

        dodgeChance = unit.combatTraining + unit.bionicEnhancement + (unit.luck / 2) + tileOn.tileDodge + unit.bonusDodge;
        hitChance = unit.equippedWeapon.hitChance + unit.evasiveTactics + unit.bionicEnhancement + (unit.luck / 2);

        if (unit.cbDrugs)
        {
            damage += CB_DRUGS_BONUS / 5;
            dodgeChance += CB_DRUGS_BONUS;
            hitChance += CB_DRUGS_BONUS;
        }

        coverDodgeChance = dodgeChance + (unit.combatTraining) + COVER_BONUS;
        flankedDodgeChance = dodgeChance - FLANK_PENALTY;



        maxRange = unit.equippedWeapon.maxRange;
        minRange = unit.equippedWeapon.minRange;
    }
}
