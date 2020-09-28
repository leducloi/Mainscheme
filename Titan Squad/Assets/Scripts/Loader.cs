using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -----------------SCRIPT INFORMATION-----------------
 * The only purpose of this script is to load the Game Manager.
 * ----------------------------------------------------
 */

public class Loader : MonoBehaviour
{

    public GameObject gameManager;
    //Used to instantiate the Game Manager
    void Awake()
    {
        if (GameManager.instance == null)
            Instantiate(gameManager);
    }
}
