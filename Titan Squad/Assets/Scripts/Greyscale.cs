using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Greyscale : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    public bool currGreyscale = false;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void makeGreyscale(bool greySetting)
    {
        float greyAmount = greySetting ? 1f : 0f;
        currGreyscale = greySetting;
        spriteRenderer.material.SetFloat("_GrayscaleAmount", greyAmount);
    }
}
