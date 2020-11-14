using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public Canvas menu;
    public GameObject panel = null;

    private RectTransform bounds = null;

    // Start is called before the first frame update
    void Start()
    {
        menu = GetComponent<Canvas>();
        menu.enabled = false;
        bounds = panel.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) && menu.enabled)
            hideMenu();
    }

    public void endTurn()
    {
        Level.instance.endTurn();
        menu.enabled = false;
    }

    public void displayMenu()
    {
        CollisionTile tile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (tile == null)
            return;

        if (menu.enabled)
            return;

        Vector3 nearestTile = tile.coordinate;
        if (!menu.enabled)
        {
            nearestTile.y += .25f;
            bounds.position = Camera.main.WorldToScreenPoint(nearestTile);
            menu.enabled = true;
        }
        else if (nearestTile != MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(bounds.position)).coordinate)
        {
            menu.enabled = false;
        }
    }

    public void hideMenu()
    {
        menu.enabled = false;
    }

    public void quitToMainMenu()
    {
        GameManager.instance.loadMainMenu();
        hideMenu();
    }

    public void resetMap()
    {
        GameManager.instance.resetMission();
        hideMenu();
    }
}
