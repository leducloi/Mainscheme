using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCalculator : MonoBehaviour
{
    public static CombatCalculator instance;
    public Unit currUnit;
    public Unit currEnemy;

    public bool doesHit = false;
    public int hitChanceDisplay = 0;
    public int damageDone = 0;

    const int BASE_HIT = 75;
    const int RANGE_PENALTY = -40;
    const int STEALTH_BONUS = 50;
    const int FLANKING_BONUS = 30;
    const int COVER_BONUS = 30;

    // Start is called before the first frame update
    void Start()
    {
        //If there are no instances of CombatCalculator, just set it to this
        if (instance == null)
            instance = this;
        //If there is more than one instance of CombatCalculator, destroy the copy and reset it
        else if (instance != this)
        {
            Destroy(instance);
            instance = this;
        }
    }

    public void calculate(Unit attacker, Unit target)
    {
        currUnit = attacker;
        currEnemy = target;

        CollisionTile defenderTile = MapBehavior.instance.getTileAtPos(target.transform.position);

        int hitChance = BASE_HIT;

        //Include attacker stat bonuses
        hitChance += attacker.combatTraining + attacker.bionicEnhancement + (attacker.luck / 2);

        if (target.isFlankedBy(attacker))
        {
            //Include the flanking bonus
            hitChance += FLANKING_BONUS;
        }
        else if (target.takingCover)
        {
            //Include defender stat bonuses
            hitChance -= COVER_BONUS;
        }

        hitChance -= target.combatTraining + target.evasiveTactics + target.bionicEnhancement + (target.luck / 2);

        //Include the dodge chance from the terrain
        hitChance -= defenderTile.tileDodge + target.bonusDodge;

        //If the target is outside of the effective range, include a range penalty
        if (!MapBehavior.instance.hasLineTo(attacker.transform.position, target.transform.position, attacker.equippedWeapon.effectiveRange, attacker.equippedWeapon.minRange))
        {
            hitChance += RANGE_PENALTY;
        }

        hitChanceDisplay = hitChance;
        damageDone = attacker.equippedWeapon.damage;

        doesHit = (hitChance >= Random.Range(0.0f, 100.0f));
    }
}
