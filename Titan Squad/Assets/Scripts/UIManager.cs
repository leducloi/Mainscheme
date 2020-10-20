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
    public GameObject playerSelectedOutline;
    public GameObject enemySelectedOutline;
    public GameObject pauseMenu;
    public GameObject forecastMenu;

    private List<GameObject> enemyOutlines = new List<GameObject>();

    //Not sure if I need to move the text into its own independent script
    //This text displays in the middle of the camera view of which phase it is
    public Text playerPhaseText;
    public Text enemyPhaseText;

    public PlayerUnit currUnit;
    public EnemyUnit currTarget;
   

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
        
        playerSelectedOutline = Instantiate(playerSelectedOutline);
        playerSelectedOutline.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            openPauseMenu();

        if (currUnit != null)
        {
            playerSelectedOutline.transform.position = currUnit.transform.position;
            if (!playerSelectedOutline.activeSelf)
                playerSelectedOutline.SetActive(true);
        }
        else
        {
            if (playerSelectedOutline.activeSelf)
                playerSelectedOutline.SetActive(false);
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
        instance.actionMenu.GetComponent<ActionMenu>().displayMenu(unit);
    }

    public void moveSelected()
    {
        instance.actionMenu.GetComponent<ActionMenu>().enableMove();
    }

    public void attackSelected()
    {
        List<Unit> unitsInRange = MapBehavior.instance.getUnitsInRange(instance.currUnit.transform.position, instance.currUnit.equippedWeapon.maxRange);
        foreach (Unit u in unitsInRange)
        {
            GameObject outline = Instantiate(instance.enemySelectedOutline);
            instance.enemyOutlines.Add(outline);
            outline.transform.position = u.transform.position;
        }
        instance.actionMenu.GetComponent<ActionMenu>().beginAttack(unitsInRange);
    }

    public void targetChosen(GameObject target)
    {
        Vector3 unitLocation = target.GetComponent<Unit>().transform.position;
        foreach (GameObject outline in instance.enemyOutlines)
        {
            if (!outline.transform.position.Equals(unitLocation))
            {
                Destroy(outline);
            }
        }
        instance.forecastMenu.GetComponent<ForecastMenu>().displayMenu();
        instance.currTarget = target.GetComponent<EnemyUnit>();
    }
     
    public void targetConfirmed()
    {
        instance.currUnit.attack(instance.currTarget);
        instance.clearOutlines();
        instance.resetCurrentUnit();
        instance.forecastMenu.GetComponent<ForecastMenu>().hideMenu();
    }

    public void resetCurrentUnit()
    {
        instance.currUnit = null;
        instance.currTarget = null;
        instance.actionMenu.GetComponent<ActionMenu>().currUnit = null;
    }

    public void clearOutlines()
    {
        foreach (GameObject outline in instance.enemyOutlines)
            Destroy(outline);
        instance.enemyOutlines.Clear();
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
