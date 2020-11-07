using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public string type = "none";
    public string description;
    public string interactText;
    public bool completed = false;
    public bool active = false;
    
    public List<CollisionTile> tilesToHighlight;

    private bool showing = false;
    private GameObject tileHolder;

    // Start is called before the first frame update
    void Start()
    {
        switch (type)
        {
            case "Route":
                description = "Route the Enemy";
                interactText = "--";
                break;
            case "Assassination":
                description = "Eliminate Target";
                interactText = "--";
                break;
            case "Sabotage":
                description = "Sabotage Targets";
                interactText = "Plant C4";
                break;
            case "Data Retrieval":
                description = "Retrieve Data";
                interactText = "Download";
                break;
            case "Rescue":
                description = "Rescue Civilians";
                interactText = "Release";
                break;
            case "Exfill":
                description = "Exfill";
                interactText = "Trigger Exfill";
                break;
            default:
                break;
        }

        calculateHighlightTiles();
    }

    // Update is called once per frame
    void Update()
    {
        if (active && !showing)
        {
            showing = true;
            tileHolder = MapBehavior.instance.highlightObjectiveTiles(tilesToHighlight);
        }
        if (!active && showing)
        {
            MapBehavior.instance.deleteObjectiveTiles(tileHolder);
            showing = false;
        }
    }

    private void calculateHighlightTiles()
    {
        tilesToHighlight = new List<CollisionTile>();
        switch (type)
        {
            case "Assassination":
                foreach (EnemyUnit u in Level.instance.getAssasinationTargets())
                {
                    if (u != null)
                        tilesToHighlight.Add(MapBehavior.instance.getTileAtPos(u.transform.position));
                }
                break;

            case "Sabotage":
            case "Data Retrieval":
            case "Rescue":
                {
                    CollisionTile currTile = MapBehavior.instance.getTileAtPos(transform.position);
                    List<CollisionTile> neighbors = MapBehavior.instance.findNeighborTiles(currTile);

                    foreach (CollisionTile tile in neighbors)
                        if (tile != null && tile.passable)
                            tilesToHighlight.Add(tile);
                }
                break;

            case "Exfill":
                {
                    CollisionTile currTile = MapBehavior.instance.getTileAtPos(transform.position);
                    List<CollisionTile> neighbors = MapBehavior.instance.findNeighborTiles(currTile);

                    tilesToHighlight.Add(currTile);

                    foreach (CollisionTile tile in neighbors)
                        if (tile != null && tile.passable)
                            tilesToHighlight.Add(tile);

                    CollisionTile add = MapBehavior.instance.getTileAtPos(currTile.coordinate + new Vector3(1, 1, 0));
                    if (add != null) tilesToHighlight.Add(add);

                    add = MapBehavior.instance.getTileAtPos(currTile.coordinate + new Vector3(-1, 1, 0));
                    if (add != null) tilesToHighlight.Add(add);

                    add = MapBehavior.instance.getTileAtPos(currTile.coordinate + new Vector3(-1, -1, 0));
                    if (add != null) tilesToHighlight.Add(add);

                    add = MapBehavior.instance.getTileAtPos(currTile.coordinate + new Vector3(1, -1, 0));
                    if (add != null) tilesToHighlight.Add(add);
                }
                break;

            default:
                break;
        }
    }

    public void beginObjective()
    {
        active = true;
        completed = false;
    }

    public void completeObjective()
    {
        completed = true;

        if (type != "Exfill")
        {
            active = false;
        }
        else
        {
            foreach (CollisionTile tile in tilesToHighlight)
            {
                if (tile.hasPlayer)
                {
                    PlayerUnit player = (PlayerUnit)Level.instance.getUnitAtLoc(tile.coordinate);
                    player.exfilled = true;
                    player.useActionPoint(2);
                    player.gameObject.SetActive(false);
                    MapBehavior.instance.getMap().unitDefeated(player.transform.position, false);
                }
            }
        }

        Level.instance.updateObjectives();
    }
}
