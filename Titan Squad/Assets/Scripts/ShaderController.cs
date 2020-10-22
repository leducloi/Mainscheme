using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    public bool currGreyscale = false;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        setLowIntensity();
        hideOutline();
    }

    public void makeGreyscale(bool greySetting)
    {
        float greyAmount = greySetting ? 0f : 1f;
        currGreyscale = greySetting;
        spriteRenderer.material.SetFloat("_GrayscaleAmount", greyAmount);
    }

    public void showOutline()
    {
        Debug.Log("Showing Outline");
        spriteRenderer.material.SetFloat("_Thickness", 0.0025f);
    }

    public void hideOutline()
    {
        spriteRenderer.material.SetFloat("_Thickness", 0f);
    }

    public void setHighIntensity()
    {
        float intensity = Mathf.Pow(2, 1.5f);
        float green = 255f / 152f;
        spriteRenderer.material.SetColor("_Color", new Color(0f, green * intensity, 1 * intensity));
    }

    public void setLowIntensity()
    {
        float green = 255f / 152f;
        spriteRenderer.material.SetColor("_Color", new Color(0f, green , 1));
    }
}
