using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarControl : MonoBehaviour
{
    public Slider shieldControl;
    public Slider healthControl;
    public Slider healthUnderlay;
    public Slider shieldUnderlay;
    public Unit displayUnit;

    public int health;
    public int shields;

    public bool reducingUnderlay = false;

    private Vector3 lowPosition;
    private Vector3 highPosition;

    private Canvas display;

    private void Start()
    {
        shieldControl.maxValue = displayUnit.shieldMax;
        healthControl.maxValue = displayUnit.hpMax;

        shieldUnderlay.maxValue = shieldControl.maxValue;
        healthUnderlay.maxValue = healthControl.maxValue;

        shieldControl.value = shieldControl.maxValue;
        healthControl.value = healthControl.maxValue;
        shieldUnderlay.value = shieldControl.maxValue;
        healthUnderlay.value = healthControl.maxValue;

        health = (int)healthControl.maxValue;
        shields = (int)shieldControl.maxValue;

        lowPosition = new Vector3(0, 0, 0);
        highPosition = new Vector3(0, 1.55f, 0);

        display = transform.parent.gameObject.GetComponent<Canvas>();
        display.enabled = false;
    }

    private void Update()
    {
        smartPosition();
        if (!reducingUnderlay && (shieldUnderlay.value > shieldControl.value || healthUnderlay.value > healthControl.value))
            StartCoroutine(reduceUnderlay());
    }

    public void takeDamage(int damage)
    {
        display.enabled = true;

        int leftoverDamage = damage - (int)shieldControl.value;
        shieldControl.value -= damage;
        if (leftoverDamage > 0)
            healthControl.value -= leftoverDamage;

    }
    

    IEnumerator reduceUnderlay()
    {
        reducingUnderlay = true;

        yield return new WaitForSeconds(0.3f);

        while (shieldUnderlay.value > shieldControl.value || healthUnderlay.value > healthControl.value)
        {
            yield return null;

            if (shieldUnderlay.value > shieldControl.value)
                shieldUnderlay.value -= .05f;

            if (healthUnderlay.value > healthControl.value)
                healthUnderlay.value -= .05f;
        }

        shields = (int)shieldControl.value;
        health = (int)healthControl.value;

        reducingUnderlay = false;

        yield return new WaitForSeconds(1f);

        if (!reducingUnderlay)
            display.enabled = false;
    }

    private void smartPosition()
    {
        CollisionTile bottomLeft = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)));
        float minY = bottomLeft.coordinate.y;

        if (displayUnit.transform.position.y <= minY)
            gameObject.GetComponent<RectTransform>().localPosition = highPosition;
        else
            gameObject.GetComponent<RectTransform>().localPosition = lowPosition;
    }
}
