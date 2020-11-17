using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMenu : MonoBehaviour
{

    public Canvas renderCanvas;
    public Button inventoryButton;
    

    // Start is called before the first frame update
    void Start()
    {
        renderCanvas.worldCamera = Camera.main;
        renderCanvas.sortingLayerName = "Default";
    }

    private void Update()
    {
        if (Level.instance.selectedUnits.Count <= 0)
            inventoryButton.gameObject.SetActive(false);
        else
            inventoryButton.gameObject.SetActive(true);
    }
}
