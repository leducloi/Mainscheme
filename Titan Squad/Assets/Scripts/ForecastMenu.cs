using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForecastMenu : MonoBehaviour
{
    public Canvas menu;

    [SerializeField]
    private Text healthText;
    [SerializeField]
    private Text hitText;
    [SerializeField]
    private Text damageText;
    [SerializeField]
    private Text alertText;

    private PlayerUnit currUnit;

    // Start is called before the first frame update
    void Start()
    {
        menu = GetComponent<Canvas>();
        menu.enabled = false;
    }

    public void displayMenu()
    {
        menu.enabled = true;
    }

    public void hideMenu()
    {
        menu.enabled = false;
    }
}
