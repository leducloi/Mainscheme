using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    public bool currGreyscale = false;

    public bool outlineShowing = false;
    public bool highIntensity = false;

    private float red;
    private float green;
    private float blue;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        setLowIntensity();
        hideOutline();
    }

    public void setColor(bool player)
    {
        if (player)
        {
            red = 0f;
            green = 255f / 152f;
            blue = 1f;
        }
        else
        {
            red = 1f;
            green = 0f;
            blue = 0f;
        }
    }

    public void makeGreyscale(bool greySetting)
    {
        float greyAmount = greySetting ? 0f : 1f;
        currGreyscale = greySetting;
        spriteRenderer.material.SetFloat("_GrayscaleAmount", greyAmount);
    }

    public void showOutline()
    {
        spriteRenderer.material.SetFloat("_Thickness", 0.0025f);
        if (highIntensity)
            setHighIntensity();
        else
            setLowIntensity();
        outlineShowing = true;
    }

    public void hideOutline()
    {
        spriteRenderer.material.SetFloat("_Thickness", 0f);
        spriteRenderer.material.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
        outlineShowing = false;
    }

    public void setHighIntensity()
    {
        float intensity = Mathf.Pow(2, 1.7f);
        spriteRenderer.material.SetColor("_Color", new Color(red * intensity, green * intensity, blue * intensity, 1f));
        spriteRenderer.material.SetFloat("_Thickness", 0.001f);
        highIntensity = true;
    }

    public void setLowIntensity()
    {
        spriteRenderer.material.SetColor("_Color", new Color(red, green, blue, 1f));
        spriteRenderer.material.SetFloat("_Thickness", 0.0025f);
        highIntensity = false;
    }

    public IEnumerator playHitFlash()
    {
        spriteRenderer.material.SetColor("_OverlayColor", Color.white);
        spriteRenderer.material.SetFloat("_OverlayAmount", 1f);
        float timeElapsed = 0f;
        float flashTime = 0.2f;
        yield return new WaitForSeconds(0.1f);
        while (timeElapsed < flashTime)
        {
            yield return null;
            timeElapsed += Time.deltaTime;
            spriteRenderer.material.SetFloat("_OverlayAmount", 1f - timeElapsed / flashTime);
        }
    }
}
