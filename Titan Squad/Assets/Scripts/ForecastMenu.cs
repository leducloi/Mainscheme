using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForecastMenu : MonoBehaviour
{
    public Canvas menu;

    [SerializeField]
    private Text healthText = null;
    [SerializeField]
    private Text hitText = null;
    [SerializeField]
    private Text damageText = null;

    private PlayerUnit currUnit;

    // Start is called before the first frame update
    void Start()
    {
        menu.enabled = false;
    }

    private void Update()
    {
        if (menu.enabled)
            smartPosition();

        if (menu.enabled && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            menu.enabled = false;
            UIManager.instance.attackSelected();
        }
        else if (menu.enabled && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0) ))
        {
            UIManager.instance.targetConfirmed();
            hideMenu();
        }
    }

    public void displayMenu()
    {
        healthText.text = "" + (CombatCalculator.instance.currEnemy.hpRemaining + CombatCalculator.instance.currEnemy.shieldRemaining);
        hitText.text = "" + CombatCalculator.instance.hitChanceDisplay + "%";
        damageText.text = "" + CombatCalculator.instance.damageDone;
        smartPosition();

        menu.enabled = true;
    }

    public void hideMenu()
    {
        menu.enabled = false;
    }

    private void smartPosition()
    {
        if (CombatCalculator.instance.currEnemy == null)
        {
            hideMenu();
            return;
        }

        Vector3 location = CombatCalculator.instance.currEnemy.transform.position;
        Vector3 screenLoc = Camera.main.WorldToScreenPoint(location);

        RectTransform rt = GetComponent<RectTransform>();
        

        if (screenLoc.x < Screen.width / 2)
        {
            Vector2 setting = new Vector2(0, .5f);
            rt.anchorMin = setting;
            rt.anchorMax = setting;
            rt.pivot = setting;

            location.x += .5f;
        }
        else
        {
            Vector2 setting = new Vector2(1, .5f);
            rt.anchorMin = setting;
            rt.anchorMax = setting;
            rt.pivot = setting;

            location.x -= .5f;
        }
        CollisionTile bottomRow = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)));
        CollisionTile topRow = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height - 2, 0)));
        if (location.y == topRow.coordinate.y)
            location.y -= 1;
        else if (location.y == bottomRow.coordinate.y)
            location.y += 1;

        rt.position = Camera.main.WorldToScreenPoint(location);
    }
}
