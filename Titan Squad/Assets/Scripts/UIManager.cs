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

    //Not sure if I need to move the text into its own independent script
    //This text displays in the middle of the camera view of which phase it is
    public Text playerPhaseText;
    public Text enemyPhaseText;
   

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

        actionMenu = Instantiate(actionMenu);
        tileMenu = Instantiate(tileMenu);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //all below are functions to display the player or enemy turn text
    
    //public void showPlayerPhaseText (){
    //    //I use a coroutine to show the text and allow the player to still interact with their units
    //    if(firstTime == false){//checks  for the function to be called
    //       StartCoroutine(ShowPlayerMessage());   
    //    }
        
        
        
          
    //}

    public void ShowPlayerMessage() {
         playerPhaseText.enabled = true;//Makes the text visible on screen
         StartCoroutine(pause(false));
        }

    //public void showEnemyPhaseText (){
    //    //Shold function the same as the player phase text function
    //    if(firstTime == false){
    //    StartCoroutine(ShowEnemyMessage());
    //    }
    //    else{
    //        firstTime = false;//changes to false so the text does not overlap at the beginning of the game
    //    }
        
    //}
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

}
