using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndCard : MonoBehaviour
{
    public GameObject unit1Stats;
    public GameObject unit2Stats;
    public GameObject unit3Stats;
    public Text turnText;

    public GameObject victoryButton;
    public GameObject defeatButtons;

    public GameObject missionText;

    // Start is called before the first frame update
    void Start()
    {
        if (Level.instance.victory)
        {
            missionText.GetComponent<Text>().text = "Mission Completed";
            victoryButton.SetActive(true);
            defeatButtons.SetActive(false);
        }
        else
        {
            missionText.GetComponent<Text>().text = "Mission Failed";
            victoryButton.SetActive(false);
            defeatButtons.SetActive(true);
        }
        turnText.text = "Turn " + GameManager.instance.turnCount;

        unit1Stats.SetActive(false);
        unit2Stats.SetActive(false);
        unit3Stats.SetActive(false);
        int numUnits = Level.instance.selectedUnits.Count;
        switch (numUnits)
        {
            case 3:
                unit3Stats.SetActive(true);
                setupText(unit3Stats, Level.instance.selectedUnits[2]);
                goto case 2;
            case 2:
                unit2Stats.SetActive(true);
                setupText(unit2Stats, Level.instance.selectedUnits[1]);
                goto case 1;
            case 1:
                unit1Stats.SetActive(true);
                setupText(unit1Stats, Level.instance.selectedUnits[0]);
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void finishMap()
    {
        Level.instance.continuePlay = true;
    }

    public void setupText(GameObject holder, PlayerUnit unit)
    {
        Text[] textBoxes = holder.GetComponentsInChildren<Text>();

        textBoxes[0].text = unit.gameObject.name;

        textBoxes[1].text = "Damage Done: " + unit.damageDone;
        textBoxes[2].text = "Enemies Killed: " + unit.enemiesKilled;
        textBoxes[3].text = "Objectives: " + unit.objectivesCompleted;
        textBoxes[4].text = "Damage Taken: " + unit.damageTaken;
        textBoxes[5].text = "Abilities Used: " + unit.abilitiesUsed;
        textBoxes[6].text = "Ultimates Used: " + unit.ultimatesUsed;
    }

    public void resetMap()
    {
        GameManager.instance.resetMission();
    }

    public void quitToMenu()
    {
        GameManager.instance.loadMainMenu();
    }
}
