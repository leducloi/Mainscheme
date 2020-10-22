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
    [SerializeField]
    private Text alertText = null;

    private PlayerUnit currUnit;

    public Button confirmBtn;

    // Start is called before the first frame update
    void Start()
    {
        menu = GetComponent<Canvas>();
        menu.enabled = false;
    }

    private void Update()
    {
        if (menu.enabled && Input.GetKeyDown(KeyCode.Escape))
        {
            menu.enabled = false;
            UIManager.instance.attackSelected();
        }
        else if (menu.enabled && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ))
        {
            confirmBtn.onClick.Invoke();
        }
    }

    public void displayMenu()
    {
        healthText.text = "" + CombatCalculator.instance.currEnemy.hpRemaining;
        hitText.text = "" + CombatCalculator.instance.hitChanceDisplay + "%";
        damageText.text = "" + CombatCalculator.instance.currUnit.equippedWeapon.damage;
        alertText.text = "TODO";
        

        menu.enabled = true;
    }

    public void hideMenu()
    {
        menu.enabled = false;
    }
}
