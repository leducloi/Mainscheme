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

    public bool movingBar = false;

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
    }

    public void takeDamage(int damage)
    {
        if (damage < 0)
        {
            recieveHealing(-damage);
            return;
        }

        display.enabled = true;

        int leftoverDamage = damage - (int)shieldControl.value;
        shieldControl.value -= damage;
        if (leftoverDamage > 0)
            healthControl.value -= leftoverDamage;

        StartCoroutine(reduceUnderlay());
    }

    public void resetShields()
    {
        display.enabled = true;

        shieldUnderlay.value = shieldControl.maxValue;

        StartCoroutine(increaseOverlay());
    }

    public void recieveHealing(int healing)
    {
        if (healing < 0)
        {
            takeDamage(-healing);
            return;
        }

        display.enabled = true;

        healthUnderlay.value += healing;

        StartCoroutine(increaseOverlay());
    }

    IEnumerator increaseOverlay()
    {
        if (movingBar)
            yield break;

        movingBar = true;

        //yield return new WaitForSeconds(0.3f);

        float adjustmentFactor = ((shieldUnderlay.value - shieldControl.value) + (healthUnderlay.value - healthControl.value)) / 60f;

        while (shieldUnderlay.value > shieldControl.value || healthUnderlay.value > healthControl.value)
        {
            yield return new WaitForSeconds(1f / 60f);

            if (shieldUnderlay.value > shieldControl.value)
                shieldControl.value += adjustmentFactor;

            if (healthUnderlay.value > healthControl.value)
                healthControl.value += adjustmentFactor;
        }

        shields = (int)shieldControl.value;
        health = (int)healthControl.value;

        movingBar = false;

        yield return new WaitForSeconds(1f);

        if (!movingBar)
            display.enabled = false;
    }
    

    IEnumerator reduceUnderlay()
    {
        if (movingBar)
            yield break;

        movingBar = true;

        //yield return new WaitForSeconds(0.3f);

        float adjustmentFactor = ((shieldUnderlay.value - shieldControl.value) + (healthUnderlay.value - healthControl.value)) / 60f;

        while (shieldUnderlay.value > shieldControl.value || healthUnderlay.value > healthControl.value)
        {
            yield return new WaitForSeconds(1f / 60f);

            if (shieldUnderlay.value > shieldControl.value)
                shieldUnderlay.value -= adjustmentFactor;

            else if (healthUnderlay.value > healthControl.value)
                healthUnderlay.value -= adjustmentFactor;
        }

        shields = (int)shieldControl.value;
        health = (int)healthControl.value;

        movingBar = false;

        yield return new WaitForSeconds(1f);

        if (!movingBar)
            display.enabled = false;
    }

    private void smartPosition()
    {
        CollisionTile bottomLeft = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)));

        float minY;

        if (bottomLeft == null)
            minY = 0;
        else
            minY = bottomLeft.coordinate.y;

        if (displayUnit.transform.position.y <= minY)
            gameObject.GetComponent<RectTransform>().localPosition = highPosition;
        else
            gameObject.GetComponent<RectTransform>().localPosition = lowPosition;
    }

    public void showBar()
    {
        display.enabled = true;
    }

    public void hideBar()
    {
        display.enabled = false;
    }
}
