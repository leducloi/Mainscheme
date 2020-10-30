using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Santias : PlayerUnit
{
    const int CLOAK_DURATION = 1;
    // Start is called before the first frame update
    override
    protected void Start()
    {
        base.Start();

        abilityNames[0] = "Grapple Jump";
        abilityNames[1] = "Cloak";
        abilityNames[2] = "Full Force";

        abilityDescriptions[0] = "Santias fires out his Grappling Hook to an obstacle and lands in a tile adjacent to it.";
        abilityDescriptions[1] = "Santias cloaks himself for " + CLOAK_DURATION + " turn, rendering him untargetable by enemies.";
        abilityDescriptions[2] = "Santias teleports to and attacks a nearby enemy with his Energy Blade. If this attack kills the enemy, Santias can teleport to a nearby location and make another attack.";

    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }

    //Grapple Jump
    override
    public void ability1()
    {
        StartCoroutine(grappleJump());
        useActionPoint(1);
    }

    //Cloak
    override
    public void ability2()
    {
        StartCoroutine(cloak());
    }

    //Full Force
    override
    public void ability3()
    {
        useActionPoint(1);
    }

    IEnumerator grappleJump()
    {
        yield return null;
    }

    IEnumerator cloak()
    {
        usingAbility2 = true;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        Color prevColor = sprite.material.GetColor("_OverlayColor");
        float prevAmount = sprite.material.GetFloat("_OverlayAmount");

        Color spriteColor;
        ColorUtility.TryParseHtmlString("#00FFE0", out spriteColor);
        if (cbDrugs)
            spriteColor = Color.red;

        Vector2 shrinkTo = new Vector2(1f, 1f);
        while (shrinkTo.x > 0)
        {
            shrinkTo.x -= .1f;
            transform.localScale = shrinkTo;
            yield return null;
        }

        //Hide sprite
        sprite.color = new Color(1f, 1f, 1f, 0f);
        shrinkTo.x = 1;
        transform.localScale = shrinkTo;

        yield return new WaitForSeconds(0.35f);

        //Set Color
        sprite.material.SetColor("_OverlayColor", spriteColor);
        sprite.material.SetFloat("_OverlayAmount", 0.3f);

        Color fadeIn = new Color(1f, 1f, 1f, 0f);
        float fadeAmount = 0f;
        while (fadeAmount < 150)
        {
            fadeAmount += 0.5f;
            fadeIn.a = fadeAmount / 255f;
            sprite.color = fadeIn;
            yield return null;
        }

        useActionPoint(1);
        isCloaked = true;

        int endTurn = CLOAK_DURATION + GameManager.instance.turnCount;
        while (GameManager.instance.turnCount != endTurn)
            yield return null;
        while (!GameManager.instance.playerPhase)
            yield return null;
        

        if (cbDrugs)
        {
            sprite.material.SetColor("_OverlayColor", Color.red);
            sprite.material.SetFloat("_OverlayAmount", 0.25f);
        }

        float overlayAmount = 0.3f;
        while (fadeAmount < 255)
        {
            if (overlayAmount > 0 && !cbDrugs)
            {
                overlayAmount -= 0.1f;
                sprite.material.SetFloat("_OverlayAmount", overlayAmount);
            }
            fadeAmount += 0.25f;
            fadeIn.a = fadeAmount / 255f;
            sprite.color = fadeIn;
            yield return null;
        }

        isCloaked = false;
        usingAbility2 = false;
    }
}
