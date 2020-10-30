using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 This is the UIManager
 */

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;
    public GameObject actionMenu;
    public GameObject tileMenu;
    public GameObject pauseMenu;
    public GameObject forecastMenu;
    public GameObject combatCalculator;
    public GameObject abilityInfoWindow;

    public GameObject escapeClauseMenu;

    public List<Unit> enemiesToOutline = new List<Unit>();

    //Not sure if I need to move the text into its own independent script
    //This text displays in the middle of the camera view of which phase it is
    public Text playerPhaseText;
    public Text enemyPhaseText;

    public PlayerUnit currUnit;
    public EnemyUnit currTarget;

    private bool selectingAttack = false;
   

    void Awake()
    {
        if (instance == null){
            instance = this;
        }
            
        else if (instance != this){
            Destroy(gameObject);
        }
        playerPhaseText.gameObject.transform.parent.gameObject.SetActive(false); //Hides the text at the launch of the game
        enemyPhaseText.gameObject.transform.parent.gameObject.SetActive(false); //Hides the text at the launch of the game

        currTarget = null;
        currUnit = null;

        actionMenu = Instantiate(actionMenu);
        tileMenu = Instantiate(tileMenu);
        pauseMenu = Instantiate(pauseMenu);
        forecastMenu = Instantiate(forecastMenu);
        combatCalculator = Instantiate(combatCalculator);
        abilityInfoWindow = Instantiate(abilityInfoWindow);
        escapeClauseMenu = Instantiate(escapeClauseMenu);

        escapeClauseMenu.GetComponent<Canvas>().enabled = false;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            openPauseMenu();

        if (selectingAttack && Input.GetKeyDown(KeyCode.Escape))
        {
            selectingAttack = false;
            clearOutlines();
        }
    }

    //all below are functions to display the player or enemy turn text
    
    public void ShowPlayerMessage()
    {
        playerPhaseText.text = "Turn " + GameManager.instance.turnCount;//Makes the text visible on screen
        playerPhaseText.gameObject.transform.parent.gameObject.SetActive(true);
        StartCoroutine(pause(false));
    }

    public void ShowEnemyMessage()
    {
        enemyPhaseText.text = "Turn " + GameManager.instance.turnCount;//Makes the text visible on screen
        enemyPhaseText.gameObject.transform.parent.gameObject.SetActive(true);
        StartCoroutine(pause(true));
    }

    //Add in a pause to disable the text, player = true means the player ended their turn, false is the enemy ended their turn
    IEnumerator pause(bool player)
    {
        yield return new WaitForSeconds(2f);
        if (player)
            enemyPhaseText.gameObject.transform.parent.gameObject.SetActive(false);
        else
            playerPhaseText.gameObject.transform.parent.gameObject.SetActive(false);
    }

    public void unitSelected(GameObject unit)
    {
        instance.pauseMenu.GetComponent<PauseMenu>().hideMenu();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().displayMenu(unit);
    }

    public void moveSelected()
    {
        instance.actionMenu.GetComponentInChildren<ActionMenu>().enableMove();
    }

    public void attackSelected()
    {
        selectingAttack = true;
        List<Unit> unitsInRange = MapBehavior.instance.getUnitsInRange(instance.currUnit.transform.position, instance.currUnit.equippedWeapon.maxRange);
        
        foreach (Unit u in unitsInRange)
        {
            u.showOutline();
            instance.enemiesToOutline.Add(u);
        }
        instance.actionMenu.GetComponentInChildren<ActionMenu>().beginAttack(unitsInRange);
    }

    public void targetChosen(GameObject target)
    {
        instance.currTarget = target.GetComponent<EnemyUnit>();
        Vector3 unitLocation = target.GetComponent<Unit>().transform.position;
        foreach (Unit u in instance.enemiesToOutline)
        {
            if (u == null)
                continue;

            if (u != currTarget)
            {
                u.hideOutline();
            }
            else
            {
                u.setAndLockHighIntensity();
            }
        }
        CombatCalculator.instance.calculate(instance.currUnit, target.GetComponent<Unit>());
        instance.forecastMenu.GetComponentInChildren<ForecastMenu>().displayMenu();
    }
     
    public void targetConfirmed()
    {
        instance.currUnit.attack(instance.currTarget);
        instance.clearOutlines();
        instance.resetCurrentUnit();
        instance.forecastMenu.GetComponentInChildren<ForecastMenu>().hideMenu();
    }

    public void abilityMenu()
    {
        instance.actionMenu.GetComponentInChildren<ActionMenu>().switchToAbilityMenu();
    }

    public void selectAbility1()
    {
        instance.currUnit.selectAbility = true;
        instance.abilityInfoWindow.GetComponent<AbilityInfo>().displayInfo(instance.currUnit, 0);
        instance.actionMenu.GetComponentInChildren<ActionMenu>().switchToActionMenu();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().hideMenu();
    }

    public void selectAbility2()
    {
        instance.currUnit.selectAbility = true;
        instance.abilityInfoWindow.GetComponent<AbilityInfo>().displayInfo(instance.currUnit, 1);
        instance.actionMenu.GetComponentInChildren<ActionMenu>().switchToActionMenu();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().hideMenu();
    }

    public void selectAbility3()
    {
        instance.currUnit.selectAbility = true;
        instance.abilityInfoWindow.GetComponent<AbilityInfo>().displayInfo(instance.currUnit, 2);
        instance.actionMenu.GetComponentInChildren<ActionMenu>().switchToActionMenu();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().hideMenu();
    }

    public void resetCurrentUnit()
    {
        //instance.currUnit = null;
        instance.currTarget = null;
        instance.actionMenu.GetComponentInChildren<ActionMenu>().currUnit = null;
    }

    public void clearOutlines()
    {
        foreach (Unit u in instance.enemiesToOutline)
            u.hideOutline();
        instance.enemiesToOutline.Clear();
    }

    public void endTurnSelected()
    {
        instance.pauseMenu.GetComponent<PauseMenu>().endTurn();
    }

    public void openPauseMenu()
    {
        CollisionTile tileOn = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (tileOn.hasEnemy)
            return;

        if (GameManager.instance.playerPhase && (instance.currUnit == null || !instance.currUnit.selected))
        {
            instance.pauseMenu.GetComponent<PauseMenu>().displayMenu();

        }
            
    }

    public void escapeClauseSelect()
    {
        instance.escapeClauseMenu.GetComponent<Canvas>().enabled = true;
    }

    public void escapeClauseDeposit()
    {
        Kennedy ken = (Kennedy)instance.currUnit;
        ken.placeDepositSpot();
        instance.escapeClauseMenu.GetComponent<Canvas>().enabled = false;
    }

    public void escapeClauseTeleport()
    {
        Kennedy ken = (Kennedy)instance.currUnit;
        ken.teleportToSpot();
        instance.escapeClauseMenu.GetComponent<Canvas>().enabled = false;
    }

}
