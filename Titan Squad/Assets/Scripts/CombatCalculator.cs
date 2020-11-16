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
        

        int hitChance = attacker.cbData.hitChance - target.cbData.getDodgeChanceFrom(attacker);

        damageDone = attacker.cbData.damage;
        if (target.shieldRemaining > 0)
            damageDone = Mathf.Clamp(damageDone - target.cbData.defense, 0, int.MaxValue);

        doesHit = (hitChance >= ( (Random.Range(0.0f, 100.0f) + Random.Range(0.0f, 100.0f)) / 2 ) );

        hitChanceDisplay = hitChance;
    }
}
