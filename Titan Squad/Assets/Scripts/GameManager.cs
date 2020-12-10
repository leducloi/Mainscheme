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
    public AudioClip[] songs;
    [SerializeField]
    private AudioSource source = null;

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

    public float volumeMusic = .6f;

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
        source.loop = true;
        source.clip = songs[0];
        source.Play();
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
        turnCount = 0;
        Instantiate(UIMan);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Called to end the player's turn
    public IEnumerator endPlayerTurn()
    {
        StartCoroutine(fadeOut());
        playerPhase = false;
        UIManager.instance.ShowEnemyMessage();
        foreach (Unit u in Level.instance.enemyUnits)
        {
            if (u != null && u.isActiveAndEnabled)
                u.healthBar.resetShields();
        }
        yield return new WaitForSeconds(2);

        source.volume = volumeMusic;
        source.clip = songs[3];
        source.Play();
        enemyPhase = true;
    }

    //Called to end the enemy's turn
    public IEnumerator endEnemyTurn()
    {
        StartCoroutine(fadeOut());
        turnCount++;
        enemyPhase = false;
        UIManager.instance.ShowPlayerMessage();
        foreach (Unit u in Level.instance.playerUnits)
        {
            if (u != null && u.isActiveAndEnabled)
                u.healthBar.resetShields();
        }
        StartCoroutine(CameraBehavior.instance.panCameraTo(Level.instance.selectedUnits.ToArray()[0].transform.position, 1f));
        yield return new WaitForSeconds(2);

        source.volume = volumeMusic;
        source.clip = songs[2];
        source.Play();
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
        StartCoroutine(fadeOut());

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
            source.clip = songs[0];
            mapMan.deloadCurrMap();
            mainMenu.SetActive(true);
        }
        else
        {
            loadNextMap();
            source.clip = songs[1];
        }
        while (alpha.a > 0)
        {
            alpha.a -= 5f * Time.deltaTime;
            fadeAlpha.color = alpha;
            yield return null;
        }
        source.volume = volumeMusic;
        source.Play();
        Destroy(overlay);
    }

    private IEnumerator fadeOut()
    {
        float duration = 0.2f;
        float currentDuration = 0;
        float startVolume = source.volume;

        while (currentDuration < duration)
        {
            currentDuration += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0, currentDuration / duration);
            yield return null;
        }
        
    }

    public void resetMission()
    {
        if (onTutorial)
        {
            currTutorial--;
            loadTutorial(currTutorial);
        }
        else
        {
            currMap--;
            loadLevel(currMap);
        }
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
