using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoPanel : MonoBehaviour
{
    public Canvas menu;

    public Image unitImage;
    public Sprite grunt;
    public Sprite skirmisher;
    public Sprite kennedy;
    public Sprite santias;
    public Sprite haley;

    public Text unitName;
    public Text health;
    public Text shields;
    public Text damage;
    public Text defense;
    public Text minRange;
    public Text maxRange;
    public Text hitChance;
    public Text rawDodge;
    public Text coverDodge;
    public Text flankedDodge;

    public Slider targetingSlider;
    public Slider shieldingSlider;
    public Slider trainingSlider;
    public Slider tacticsSlider;
    public Slider bionicSlider;
    public Slider luckSlider;

    private const int SLIDER_MAX = 30;
    private const int SMALL_SLIDER_MAX = 5;
    private const int BIONICS_MAX = 15;

    // Start is called before the first frame update
    void Start()
    {
        menu.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (menu.enabled && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape)))
        {
            hideMenu();
        }
    }

    public void displayUnitMenu()
    {
        CollisionTile nearestTile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (nearestTile == null)
            return;

        Unit currUnit = Level.instance.getUnitAtLoc(nearestTile.coordinate);
        if (currUnit == null)
            return;

        health.text = "" + currUnit.hpRemaining + " / " + currUnit.hpMax;
        shields.text = "" + currUnit.shieldRemaining + " / " + currUnit.shieldMax;


        CombatData unitData = currUnit.cbData;
        damage.text = "Damage\n " + unitData.damage;
        defense.text = "Shield Defense\n " + unitData.defense;
        minRange.text = "Min Range\n " + unitData.minRange;
        maxRange.text = "Max Range\n " + unitData.maxRange;
        hitChance.text = "Hit Chance\n " + unitData.hitChance;
        rawDodge.text = "Raw Dodge\n " + unitData.dodgeChance;
        coverDodge.text = "Cover Dodge\n " + unitData.coverDodgeChance;
        flankedDodge.text = "Flanked Dodge\n " + unitData.flankedDodgeChance;

        if (currUnit.name.Contains("Grunt"))
        {
            unitName.text = "Krimbar Grunt";
            unitImage.sprite = grunt;
        }
        else if (currUnit.name.Contains("Skirmisher"))
        {
            unitName.text = "Krimbar Skirmisher";
            unitImage.sprite = skirmisher;
        }
        else if (currUnit.name.Contains("Kennedy"))
        {
            unitName.text = currUnit.name;
            unitImage.sprite = kennedy;
        }
        else if (currUnit.name.Contains("Santias"))
        {
            unitName.text = currUnit.name;
            unitImage.sprite = santias;
        }
        else if (currUnit.name.Contains("Haley"))
        {
            unitName.text = currUnit.name;
            unitImage.sprite = haley;
        }


        targetingSlider.maxValue = SMALL_SLIDER_MAX;
        targetingSlider.value = currUnit.criticalTargeting;
        targetingSlider.GetComponentInChildren<Text>().text = "" + currUnit.criticalTargeting;

        shieldingSlider.maxValue = SMALL_SLIDER_MAX;
        shieldingSlider.value = currUnit.advancedShielding;
        shieldingSlider.GetComponentInChildren<Text>().text = "" + currUnit.advancedShielding;

        trainingSlider.maxValue = SLIDER_MAX;
        trainingSlider.value = currUnit.combatTraining;
        trainingSlider.GetComponentInChildren<Text>().text = "" + currUnit.combatTraining;

        tacticsSlider.maxValue = SLIDER_MAX;
        tacticsSlider.value = currUnit.evasiveTactics;
        tacticsSlider.GetComponentInChildren<Text>().text = "" + currUnit.evasiveTactics;

        bionicSlider.maxValue = BIONICS_MAX;
        bionicSlider.value = currUnit.bionicEnhancement;
        bionicSlider.GetComponentInChildren<Text>().text = "" + currUnit.bionicEnhancement;

        luckSlider.maxValue = SLIDER_MAX;
        luckSlider.value = currUnit.luck;
        luckSlider.GetComponentInChildren<Text>().text = "" + currUnit.luck;

        menu.enabled = true;
    }

    private void hideMenu()
    {
        menu.enabled = false;
    }
}
