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

    private List<Unit> enemiesToOutline = new List<Unit>();

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
        playerPhaseText.enabled = false;//Hides the text at the launch of the game
        enemyPhaseText.enabled = false;//Hides the text at the launch of the game

        currTarget = null;
        currUnit = null;

        actionMenu = Instantiate(actionMenu);
        tileMenu = Instantiate(tileMenu);
        pauseMenu = Instantiate(pauseMenu);
        forecastMenu = Instantiate(forecastMenu);
        combatCalculator = Instantiate(combatCalculator);
        
        
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
    
    public void ShowPlayerMessage() {
         playerPhaseText.enabled = true;//Makes the text visible on screen
         StartCoroutine(pause(false));
        }

    public void ShowEnemyMessage() {
         //should be the same as the ShowPlayerMessage
         enemyPhaseText.enabled = true;
         StartCoroutine(pause(true));
        }

    //Add in a pause to disable the text, player = true means the player ended their turn, false is the enemy ended their turn
    IEnumerator pause(bool player)
    {
        yield return new WaitForSeconds(1);
        if (player)
            enemyPhaseText.enabled = false;
        else
            playerPhaseText.enabled = false;
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

    public void resetCurrentUnit()
    {
        instance.currUnit = null;
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
        if (GameManager.instance.playerPhase && (currUnit == null || !currUnit.selected))
        {
            instance.pauseMenu.GetComponent<PauseMenu>().displayMenu();

        }
            
    }

}
