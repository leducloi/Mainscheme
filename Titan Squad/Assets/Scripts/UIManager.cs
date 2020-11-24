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

    [SerializeField]
    private AudioClip menuUp = null;
    [SerializeField]
    private AudioClip menuDown = null;
    [SerializeField]
    private AudioSource menuSound = null;

    public GameObject actionMenu;
    public GameObject tileMenu;
    public GameObject pauseMenu;
    public GameObject forecastMenu;
    public GameObject combatCalculator;
    public GameObject abilityInfoWindow;
    public GameObject missText;
    public GameObject endCard;
    public GameObject textBox;
    public GameObject unitInfoMenu;
    public GameObject inventorySystem;

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
            
        else if (instance != this)
        {
            
            Destroy(instance.gameObject);
            instance = this;
        }
        playerPhaseText.gameObject.transform.parent.gameObject.SetActive(false); //Hides the text at the launch of the game
        enemyPhaseText.gameObject.transform.parent.gameObject.SetActive(false); //Hides the text at the launch of the game

        currTarget = null;
        currUnit = null;

        actionMenu = Instantiate(actionMenu, transform);
        tileMenu = Instantiate(tileMenu, transform);
        pauseMenu = Instantiate(pauseMenu, transform);
        forecastMenu = Instantiate(forecastMenu, transform);
        combatCalculator = Instantiate(combatCalculator, transform);
        abilityInfoWindow = Instantiate(abilityInfoWindow, transform);
        escapeClauseMenu = Instantiate(escapeClauseMenu, transform);
        textBox = Instantiate(textBox, transform);
        unitInfoMenu = Instantiate(unitInfoMenu, transform);
        inventorySystem = Instantiate(inventorySystem, transform);

        escapeClauseMenu.GetComponent<Canvas>().enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            openPauseMenu();

        if (Input.GetMouseButtonDown(1))
            openInfoMenu();

        if (selectingAttack && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            playMenuDown();
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
        playMenuUp();
        instance.pauseMenu.GetComponent<PauseMenu>().hideMenu();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().displayMenu(unit);
    }

    public void showActionMenu()
    {
        instance.actionMenu.GetComponentInChildren<ActionMenu>().displayMenu(instance.currUnit.gameObject);
    }

    public void moveSelected()
    {
        playMenuUp();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().enableMove();
    }

    public void attackSelected()
    {
        selectingAttack = true;
        List<Unit> unitsInRange = MapBehavior.instance.getUnitsInRange(instance.currUnit.transform.position, instance.currUnit.equippedWeapon.maxRange, instance.currUnit.equippedWeapon.minRange);
        
        foreach (Unit u in unitsInRange)
        {
            u.showOutline();
            instance.enemiesToOutline.Add(u);
        }
        playMenuUp();
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
        playMenuUp();
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
        playMenuUp();
        instance.currUnit.selectAbility = true;
        instance.abilityInfoWindow.GetComponent<AbilityInfo>().displayInfo(instance.currUnit, 0);
        instance.actionMenu.GetComponentInChildren<ActionMenu>().switchToActionMenu();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().hideMenu();
    }

    public void selectAbility2()
    {
        playMenuUp();
        instance.currUnit.selectAbility = true;
        instance.abilityInfoWindow.GetComponent<AbilityInfo>().displayInfo(instance.currUnit, 1);
        instance.actionMenu.GetComponentInChildren<ActionMenu>().switchToActionMenu();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().hideMenu();
    }

    public void selectAbility3()
    {
        playMenuUp();
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
            if (u != null)
                u.hideOutline();
        instance.enemiesToOutline.Clear();
    }

    public void endTurnSelected()
    {
        instance.pauseMenu.GetComponent<PauseMenu>().endTurn();
    }

    public void openPauseMenu()
    {
        if (MapBehavior.instance == null || Level.instance.levelDone)
            return;

        if (instance.unitInfoMenu.GetComponent<Canvas>().enabled)
            return;

        CollisionTile tileOn = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (tileOn == null)
            return;

        if (tileOn.hasEnemy)
            return;

        if (GameManager.instance.playerPhase && (instance.currUnit == null || !instance.currUnit.selected))
        {
            playMenuUp();
            PauseMenu pMenu = instance.pauseMenu.GetComponent<PauseMenu>();
            pMenu.displayMenu();
        }
    }

    public void openInfoMenu()
    {
        if (MapBehavior.instance == null || Level.instance.levelDone)
            return;

        if (!GameManager.instance.enemyPhase && (instance.currUnit == null || !instance.currUnit.selected) && !CameraBehavior.instance.pauseMovement)
        {
            instance.unitInfoMenu.GetComponent<UnitInfoPanel>().displayUnitMenu();
        }
    }

    public void escapeClauseSelect()
    {
        playMenuUp();
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

    public void attackMissed(Vector3 location)
    {
        Instantiate(instance.missText);
    }
    
    public void completeObjective()
    {
        instance.actionMenu.GetComponentInChildren<ActionMenu>().currObjective.completeObjective();
        instance.actionMenu.GetComponentInChildren<ActionMenu>().menu.enabled = false;
        instance.currUnit.useActionPoint(1);
        instance.currUnit.objectivesCompleted++;
    }

    public void showEndCard()
    {
        instance.endCard = Instantiate(instance.endCard, instance.transform);
    }

    public void resetMission()
    {
        GameManager.instance.resetMission();
    }

    public void quitToMainMenu()
    {
        GameManager.instance.loadMainMenu();
    }

    public void showInventory()
    {
        playMenuUp();
        instance.inventorySystem.GetComponent<InventoryManager>().displayInventory(Level.instance.selectedUnits);
    }

    public void showInventory(List<PlayerUnit> units)
    {
        if (units == null || units.Count == 0)
            units = Level.instance.selectedUnits;
        playMenuUp();
        instance.inventorySystem.GetComponent<InventoryManager>().displayInventory(units);
    }

    public void playMenuUp()
    {
        instance.menuSound.enabled = true;
        instance.menuSound.clip = menuUp;
        instance.menuSound.Play();
    }

    public void playMenuDown()
    {
        instance.menuSound.enabled = true;
        instance.menuSound.clip = menuDown;
        instance.menuSound.Play();
    }
}
