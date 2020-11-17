using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthStim : Item
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override
    public void activate()
    {
        StartCoroutine(useHealthStim());
    }

    IEnumerator useHealthStim()
    {
        PlayerUnit player = transform.parent.GetComponent<PlayerUnit>();

        MapBehavior.instance.setColor('g');
        MapBehavior.instance.highlightTilesWithin(transform.parent.transform.position, 1);

        List<Unit> unitsInRange = MapBehavior.instance.getAlliesInRange(transform.parent.transform.position, 1);

        //Outline units to select
        foreach (Unit u in unitsInRange)
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
                    MapBehavior.instance.deleteHighlightTiles();
                    List<PlayerUnit> unit = new List<PlayerUnit>();
                    unit.Add((PlayerUnit)Level.instance.getUnitAtLoc(transform.parent.transform.position));
                    InventoryManager.instance.displayInventory(unit);
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
        target.hit(-5);

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

        player.useActionPoint(1);
        Destroy(gameObject);
    }
}
