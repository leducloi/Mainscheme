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
        if (Input.GetKeyDown(KeyCode.Escape) && menu.enabled)
            hideMenu();
    }

    public void endTurn()
    {
        Level.instance.endTurn();
        menu.enabled = false;
    }

    public void displayMenu()
    {
        if (!menu.enabled)
        {
            Vector3 nearestTile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition)).coordinate;
            nearestTile.y += .25f;
            bounds.position = Camera.main.WorldToScreenPoint(nearestTile);
        }
            
        menu.enabled = true;
    }

    public void hideMenu()
    {
        menu.enabled = false;
    }
}
