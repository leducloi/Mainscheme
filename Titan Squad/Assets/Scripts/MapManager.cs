using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -----------------SCRIPT INFORMATION-----------------
 * The point of this script is quite simply to decide which map is supposed to be loaded at a given time.
 * ----------------------------------------------------
 * Examples of script behavior: Loading maps.
 * Examples of how not to use the script: Moving the player units, any AI, determining when to change maps.
 */

public class MapManager : MonoBehaviour
{
    //Contains all the maps of the game
    public GameObject[] mapList;
    public GameObject[] tutorialList;
    public GameObject currMap;



    //Called by GameManager to load in a new map
    public bool loadMap(int mapNumber, bool tutorial = false)
    {
        if (tutorial)
        {
            if (mapNumber >= tutorialList.Length)
            {
                deloadCurrMap();
                GameManager.instance.loadMainMenu();
                return false;
            }
            else
            {
                currMap = Instantiate(tutorialList[mapNumber]);
                return true;
            }
        }
        if (mapNumber >= mapList.Length)
        {
            deloadCurrMap();
            GameManager.instance.loadMainMenu();
            return false;
        }
        else
        {
            currMap = Instantiate(mapList[mapNumber]);
        }
        return true;
    }

    public void deloadCurrMap()
    {
        Destroy(currMap);
    }
}
