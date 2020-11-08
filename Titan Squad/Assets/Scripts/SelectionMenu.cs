using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionMenu : MonoBehaviour
{

    public Canvas renderCanvas;
    

    // Start is called before the first frame update
    void Start()
    {
        renderCanvas.worldCamera = Camera.main;
        renderCanvas.sortingLayerName = "Default";
    }

    private void Update()
    {
        
    }
}
