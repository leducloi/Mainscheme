using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Color hoverColor;
    public Color normalColor;
    public Image colorControl;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Level.instance.selectedUnits.Count < 3)
        {
            colorControl.enabled = false;
            GetComponentInChildren<Text>().enabled = false;
        }
        else
        {
            colorControl.enabled = true;
            GetComponentInChildren<Text>().enabled = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        colorControl.color = hoverColor;
        GetComponentInChildren<Text>().color = Color.black;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        colorControl.color = normalColor;
        GetComponentInChildren<Text>().color = Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Level.instance.finishPlanning();
    }
}
