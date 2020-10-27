using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInfo : MonoBehaviour
{

    public Canvas menu;
    public Text abilityName;
    public Text abilityDesc;
    public RectTransform bounds;

    private int abilityNum;
    private PlayerUnit currUnit;

    // Start is called before the first frame update
    void Start()
    {
        menu.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (menu.enabled && Input.GetKeyDown(KeyCode.Escape))
        {
            menu.enabled = false;
            UIManager.instance.abilityMenu();
        }
        if (menu.enabled && ( Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) ))
        {
            switch (abilityNum)
            {
                case 0:
                    currUnit.ability1();
                    break;
                case 1:
                    currUnit.ability2();
                    break;
                case 2:
                    currUnit.ability3();
                    break;
                default:
                    Debug.Log("Error: Invalid ability number recieved.");
                    break;
            }
            menu.enabled = false;
        }
    }

    public void displayInfo(PlayerUnit unit, int ability)
    {
        abilityName.text = unit.abilityNames[ability];
        abilityDesc.text = unit.abilityDescriptions[ability];

        abilityNum = ability;
        currUnit = unit;

        menu.enabled = true;
    }

    public void smartPosition()
    {
        CollisionTile tryPos = MapBehavior.instance.getTileAtPos(currUnit.transform.position + new Vector3(0, 2, 0));
        if (tryPos == null || Camera.main.WorldToScreenPoint(tryPos.coordinate).y > Screen.height)
        {
            Vector2 setting = new Vector2(.5f, 1);
            bounds.anchorMax = setting;
            bounds.anchorMin = setting;
            bounds.pivot = setting;

            bounds.position = Camera.main.WorldToScreenPoint(currUnit.transform.position) + new Vector3(0, -16, 0);
        }
        else
        {
            Vector2 setting = new Vector2(.5f, 0);
            bounds.anchorMax = setting;
            bounds.anchorMin = setting;
            bounds.pivot = setting;

            bounds.position = Camera.main.WorldToScreenPoint(tryPos.coordinate);
        }
    }
}
