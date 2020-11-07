using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -----------------SCRIPT INFORMATION-----------------
 * The point of this script is to manage the operations of the game as a generality. This means that 
 * this script is going to decide when maps need to change and whose turn it is. There should only be one, and so it is
 * implemented as a singleton. 
 * ----------------------------------------------------
 * Examples of script behavior: Ending turns, changing maps, loading title screen, selecting units/tiles.
 * Examples of how not to use the script: Moving the player units, any AI, loading specific maps.
 */

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    //Contains the Map Manager
    public static MapManager mapMan;

    [SerializeField]
    GameObject mainMenu;

    public GameObject UIMan;

    //Booleans to control player and enemy phases
    public bool playerPhase;
    public bool enemyPhase;
    //Tracker to keep track of what map we are on
    public int currMap = 0;
    public int currTutorial = 0;

    public bool onTutorial = false;

    public int turnCount = 0;

    public GameObject cursor;
    [SerializeField]
    GameObject fade = null;
   

    void Awake()
    {
        //If there are no instances of GameManager, instantiate it
        if (instance == null)
            instance = this;
        //If there is more than one instance of GameManager, destroy the copy
        else if (instance != this)
            Destroy(gameObject);

        //To ensure this object is not lost between scenes
        DontDestroyOnLoad(gameObject);

        mapMan = GetComponent<MapManager>();
        

        playerPhase = false;
        enemyPhase = false;

        currMap = 0;
        currTutorial = 0;

        mainMenu = Instantiate(mainMenu);
    }

    private void loadNextMap()
    {
        
        if (mainMenu.activeInHierarchy)
            mainMenu.SetActive(false);
        mapMan.deloadCurrMap();

        
        int mapNum = instance.currMap;
        if (onTutorial)
            mapNum = instance.currTutorial;
        


        if (!mapMan.loadMap(mapNum, onTutorial))
            return;

        if (onTutorial)
        {
            currTutorial++;
            if (currTutorial % 4 == 0)
                currTutorial = int.MaxValue;
        }
        else
            currMap++;

        playerPhase = false;
        enemyPhase = false;
        turnCount = 1;
        Instantiate(UIMan);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Called to end the player's turn
    public IEnumerator endPlayerTurn()
    {
        playerPhase = false;
        UIManager.instance.ShowEnemyMessage();
        foreach (Unit u in Level.instance.enemyUnits)
        {
            if (u != null && u.isActiveAndEnabled)
                u.healthBar.resetShields();
        }
        yield return new WaitForSeconds(2);
        
        enemyPhase = true;
        turnCount++;
    }

    //Called to end the enemy's turn
    public IEnumerator endEnemyTurn()
    {
        enemyPhase = false;
        UIManager.instance.ShowPlayerMessage();
        foreach (Unit u in Level.instance.playerUnits)
        {
            if (u != null && u.isActiveAndEnabled)
                u.healthBar.resetShields();
        }
        StartCoroutine(CameraBehavior.instance.panCameraTo(Level.instance.selectedUnits.ToArray()[0].transform.position, 1f));
        yield return new WaitForSeconds(2);
        
        playerPhase = true;
    }

    public void levelFinished()
    {
        StartCoroutine(screenTransition());
    }

    public void loadMainMenu()
    {
        currMap = 0;
        currTutorial = 0;
        onTutorial = false;
        instance.StartCoroutine(screenTransition(true));
    }

    private IEnumerator screenTransition(bool toMenu = false)
    {
        GameObject overlay = Instantiate(fade);
        UnityEngine.UI.Image fadeAlpha = overlay.GetComponentInChildren<UnityEngine.UI.Image>();
        Color alpha = new Color(0, 0, 0, 0);
        while (alpha.a < 1)
        {
            alpha.a += 5f * Time.deltaTime;
            fadeAlpha.color = alpha;
            yield return null;
        }
        if (toMenu)
        {
            mainMenu.SetActive(true);
        }
        else
            loadNextMap();
        while (alpha.a > 0)
        {
            alpha.a -= 5f * Time.deltaTime;
            fadeAlpha.color = alpha;
            yield return null;
        }
        Destroy(overlay);
    }
    

    public void loadTutorial(int tutNum)
    {
        instance.onTutorial = true;
        instance.currTutorial = tutNum;
        instance.levelFinished();
    }

    public void loadLevel(int levelNum)
    {
        instance.onTutorial = false;
        instance.currMap = levelNum;
        instance.levelFinished();
    }
}
