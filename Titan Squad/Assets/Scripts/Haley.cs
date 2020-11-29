using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haley : PlayerUnit
{
    const int COMBAT_DRUGS_DURATION = 3;
    const int COMBAT_DRUGS_RANGE = 7;
    const int HEALING_AMOUNT = 10;
    const int GLOVE_RANGE = 4;

    

    // Start is called before the first frame update
    override
    protected void Start()
    {
        weapons[0] = new Weapon("Glove of Power");
        weapons[1] = new Weapon("Medical Syringe");

        base.Start();


        abilityNames[0] = "Combat Drugs";
        abilityNames[1] = "Medical Tincture";
        abilityNames[2] = "Hit & Run";

        abilityDescriptions[0] = "Haley fires a powerful mixture into an ally, increasing their combat capabilities for " + COMBAT_DRUGS_DURATION + " turns.";
        abilityDescriptions[1] = "Haley jams a potent herbal mixture into an ally, healing them for " + HEALING_AMOUNT + " hitpoints.";
        abilityDescriptions[2] = "Haley makes an attack with her Glove of Power and empowers all allies within " + GLOVE_RANGE + " tiles of the enemy hit with the ability to move.";

        hpMax = 10;
        hpRemaining = 10;
        shieldMax = 5;
        shieldRemaining = 5;

        combatTraining = 20;
        evasiveTactics = 20;
        bionicEnhancement = 0;
        luck = 10;
        criticalTargeting = 0;
        advancedShielding = 1;
    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }

    //Combat Drugs
    override
    public void ability1()
    {
        StartCoroutine(combatDrugs());
    }

    //Healing Tincture
    override
    public void ability2()
    {
        StartCoroutine(medicalTincture());
    }

    //Sleep Dart
    override
    public void ability3()
    {
        StartCoroutine(hitAndRun());
    }

    IEnumerator combatDrugs()
    {
        yield return null;

        usingAbility1 = true;

        //Get valid units
        List<Unit> unitsInRange = MapBehavior.instance.getAlliesInRange(transform.position, COMBAT_DRUGS_RANGE);
        List<Unit> selectableUnits = new List<Unit>();
        foreach (Unit u in unitsInRange)
        {
            PlayerUnit temp = (PlayerUnit)u;
            if (temp.cbDrugs || temp == this)
            {
                continue;
            }
            if (u != null)
            {
                u.showOutline();
                selectableUnits.Add(u);
            }
        }

        //Wait for a valid selection
        PlayerUnit target = null;
        while (target == null)
        {
            yield return null;

            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                {
                    yield return null;
                    foreach (Unit u in selectableUnits)
                    {
                        if (u != null)
                            u.hideOutline();
                    }
                    usingAbility1 = false;
                    UIManager.instance.playMenuDown();
                    UIManager.instance.abilityMenu();
                    yield break;
                }
                yield return null;
            }

            //Check if the location selected is valid
            CollisionTile tile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (tile == null)
                continue;

            //Retry if the unit is not one of the selectable units
            Unit temp = Level.instance.getUnitAtLoc(tile.coordinate);
            if (selectableUnits.IndexOf(temp) == -1)
                continue;

            //If the selection was on a valid unit, we're done
            target = (PlayerUnit)temp;
        }

        //Selection was made, now hide outlines
        foreach (Unit u in selectableUnits)
        {
            if (u != null)
                u.hideOutline();
        }

        //Animation
        unitAudio.clip = abilitySounds[0];
        unitAudio.Play();
        
        //Get the shader material for ease of access
        Material targetShader = target.GetComponent<SpriteRenderer>().material;

        float redAmt = 1f;
        targetShader.SetColor("_OverlayColor", Color.red);
        targetShader.SetFloat("_OverlayAmount", redAmt);
        yield return new WaitForSeconds(0.3f);

        //Gradually reduce the red amount until at the proper amount
        while (redAmt > .25f)
        {
            redAmt -= 0.05f;
            targetShader.SetFloat("_OverlayAmount", redAmt);
            yield return new WaitForSecondsRealtime(1f / 60f);
        }
        //Start particle effect
        ParticleSystem.MainModule ma = target.GetComponent<ParticleSystem>().main;
        ma.startColor = Color.red;
        target.GetComponent<ParticleSystem>().Play();

        target.cbDrugs = true;
        abilitiesUsed++;
        useActionPoint(1);

        //Set up durations
        int endTurn = GameManager.instance.turnCount + COMBAT_DRUGS_DURATION;
        int startTurn = GameManager.instance.turnCount;
        while (GameManager.instance.turnCount != startTurn + 1)
            yield return null;

        //Ability cooldown over
        usingAbility1 = false;
        while (GameManager.instance.turnCount != endTurn)
            yield return null;
        target.cbDrugs = false;
        while (!GameManager.instance.playerPhase)
            yield return null;

        //Stop particle system
        target.GetComponent<ParticleSystem>().Stop();

        //Just fix color if cloaked
        if (target.isCloaked)
        {
            targetShader.SetFloat("_OverlayAmount", 0.3f);
            targetShader.SetColor("_OverlayColor", new Color(0f, 1f, 224f / 255f));
            yield break;
        }

        //Fade out the red
        while (redAmt > 0)
        {
            redAmt -= .05f;
            targetShader.SetFloat("_OverlayAmount", redAmt);
            yield return new WaitForSecondsRealtime(1f / 60f);
        }
    }

    IEnumerator medicalTincture()
    {
        usingAbility2 = true;

        MapBehavior.instance.setColor('g');
        MapBehavior.instance.highlightTilesWithin(transform.position, 1);

        List<Unit> unitsInRange = MapBehavior.instance.getAlliesInRange(transform.position, 1);
        if (unitsInRange.IndexOf(this) != -1)
            unitsInRange.Remove(this);

        //Outline units to select
        foreach(Unit u in unitsInRange)
        {
            if (u != null)
                u.showOutline();
        }

        //Wait for a valid selection
        PlayerUnit target = null;
        while (target == null)
        {
            yield return null;

            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                {
                    yield return null;
                    foreach (Unit u in unitsInRange)
                    {
                        if (u != null)
                            u.hideOutline();
                    }
                    usingAbility2 = false;
                    MapBehavior.instance.deleteHighlightTiles();
                    UIManager.instance.playMenuDown();
                    UIManager.instance.abilityMenu();
                    yield break;
                }
                yield return null;
            }

            //Check if the location selected is valid
            CollisionTile tile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (tile == null)
                continue;

            //Retry if the unit is not one of the selectable units
            Unit temp = Level.instance.getUnitAtLoc(tile.coordinate);
            if (unitsInRange.IndexOf(temp) == -1)
                continue;

            //If the selection was on a valid unit, we're done
            target = (PlayerUnit)temp;
        }

        //Animation
        unitAudio.clip = abilitySounds[1];
        unitAudio.Play();


        //Get rid of outlines
        MapBehavior.instance.deleteHighlightTiles();
        foreach (Unit u in unitsInRange)
        {
            if (u != null)
                u.hideOutline();
        }

        //Store particle system information to revert after glow
        ParticleSystem.MainModule mm = target.GetComponent<ParticleSystem>().main;
        ParticleSystem.MinMaxGradient prevColor = mm.startColor;
        mm.startColor = new ParticleSystem.MinMaxGradient(Color.green);
        bool wasPlaying = target.GetComponent<ParticleSystem>().isPlaying;

        //If particle system was not already going, play it
        if (!wasPlaying)
            target.GetComponent<ParticleSystem>().Play();

        //Store shader information to revert after glow
        Material shader = target.GetComponent<SpriteRenderer>().material;
        Color startColor = shader.GetColor("_OverlayColor");
        float startAmount = shader.GetFloat("_OverlayAmount");

        shader.SetColor("_OverlayColor", Color.green);
        float colorAmount = 0f;

        //Glow green
        while (colorAmount < 1f)
        {
            colorAmount += 0.05f;
            shader.SetFloat("_OverlayAmount", colorAmount);
            yield return new WaitForSecondsRealtime(1f / 60f);
        }
        yield return new WaitForSeconds(0.33f);
        while (colorAmount > 0f)
        {
            colorAmount -= 0.05f;
            shader.SetFloat("_OverlayAmount", colorAmount);
            yield return new WaitForSecondsRealtime(1f / 60f);
        }

        //Activate ability effect
        abilitiesUsed++;
        useActionPoint(1);
        target.hit(-HEALING_AMOUNT);

        //Reset particle system
        if (!wasPlaying)
            target.GetComponent<ParticleSystem>().Stop();
        mm.startColor = prevColor;

        //Reset shader to previous color
        shader.SetColor("_OverlayColor", startColor);
        while (colorAmount < startAmount)
        {
            colorAmount += 0.001f;
            shader.SetFloat("_OverlayAmount", colorAmount);
            yield return new WaitForSecondsRealtime(1f / 60f);
        }
        target.hideOutline();

        //Wait for cooldown
        int endTurn = GameManager.instance.turnCount + 1;
        while (endTurn != GameManager.instance.turnCount)
            yield return null;

        usingAbility2 = false;
    }

    IEnumerator hitAndRun()
    {
        usingAbility3 = true;

        yield return null;

        List<Unit> selectableUnits = MapBehavior.instance.getUnitsInRange(transform.position, equippedWeapon.maxRange, equippedWeapon.minRange);

        foreach (Unit u in selectableUnits)
        {
            if (u != null)
                u.showOutline();
        }

        MapBehavior.instance.setColor('g');

        Unit target = null;
        CollisionTile lastTile = null;
        while (target == null)
        {
            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                {
                    yield return null;
                    foreach (Unit u in selectableUnits)
                    {
                        if (u != null)
                            u.hideOutline();
                    }
                    MapBehavior.instance.deleteHighlightTiles();
                    usingAbility3 = false;
                    UIManager.instance.playMenuDown();
                    UIManager.instance.abilityMenu();
                    yield break;
                }

                CollisionTile hoverTile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (hoverTile == null)
                {
                    yield return null;
                    continue;
                }

                Unit temp = Level.instance.getUnitAtLoc(hoverTile.coordinate);
                if (temp == null || !selectableUnits.Contains(temp))
                {
                    MapBehavior.instance.deleteHighlightTiles();
                    lastTile = hoverTile;
                    yield return null;
                    continue;
                }
                if (hoverTile != lastTile)
                {
                    MapBehavior.instance.highlightTilesWithin(hoverTile.coordinate, GLOVE_RANGE);
                    lastTile = hoverTile;
                }
                yield return null;
            }

            CollisionTile selectedTile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (selectedTile == null)
                continue;
            Unit selection = Level.instance.getUnitAtLoc(selectedTile.coordinate);

            if (selectableUnits.Contains(selection))
                target = selection;

            yield return null;
        }


        Level.instance.pauseAutoEnd = true;

        MapBehavior.instance.deleteHighlightTiles();
        foreach (Unit u in selectableUnits)
        {
            if (u != null && u != target)
                u.hideOutline();
        }

        playShoot();

        if (target.hpRemaining <= equippedWeapon.damage)
            enemiesKilled++;
        target.hit(equippedWeapon.damage);
        damageDone += equippedWeapon.damage;

        target.hideOutline();

        abilitiesUsed++;
        ultimatesUsed++;
        actionPoints -= 2;

        
        Vector3 origin = target.transform.position;
        foreach (PlayerUnit unit in Level.instance.playerUnits)
        {
            Vector3 location = unit.transform.position;
            Vector2 difference = new Vector2(Mathf.Abs(location.x - origin.x), Mathf.Abs(location.y - origin.y));
            if (difference.x + difference.y <= GLOVE_RANGE)
            {
                yield return null;
                unit.bonusMove = true;
                unit.canMove = true;
                unit.selected = true;
                unit.actionPoints++;
                if (unit != this)
                {
                    unit.animator.SetTrigger("Walking");
                    unit.setAndLockHighIntensity();
                    unit.hasTurn = true;
                }
                UIManager.instance.currUnit = unit;
                
                while (unit.selected)
                    yield return null;

                unit.bonusMove = false;
            }
            yield return new WaitForSeconds(0.1f);
        }

        Level.instance.pauseAutoEnd = false;

        int endTurn = ULT_COOLDOWN + GameManager.instance.turnCount;
        while (endTurn != GameManager.instance.turnCount)
            yield return null;
        usingAbility3 = false;
    }
}
