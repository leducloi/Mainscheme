using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public Canvas menu;

    // Start is called before the first frame update
    void Start()
    {
        menu = GetComponent<Canvas>();
        menu.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && menu.enabled)
            menu.enabled = false;
    }

    public void endTurn()
    {
        Level.instance.endTurn();
        menu.enabled = false;
    }

    public void displayMenu()
    {
        if (!menu.enabled)
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        menu.enabled = true;
    }
}
