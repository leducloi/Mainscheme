using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * If we want to implement custom controls, it is important to use controls that are defined here.
 * That way, if we want to change them at a later time, we simply have to change them in one place.
 */

public class Controls : MonoBehaviour
{
    public Controls instance = null;
    public string up;
    public string down;
    public string left;
    public string right;
    public string confirm;

    void Start()
    {
        //If there are no instances of Controls, instantiate it
        if (instance == null)
            instance = this;
        //If there is more than one instance of Controls, destroy the copy
        else if (instance != this)
            Destroy(gameObject);
        //To ensure this object is not lost between scenes
        DontDestroyOnLoad(gameObject);


        up = "w";
        down = "s";
        left = "a";
        right = "d";
        confirm = "spacebar";
    }
}
