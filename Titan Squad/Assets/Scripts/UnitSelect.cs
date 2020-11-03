using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitSelect : MonoBehaviour, IDragHandler//, IPointerEnterHandler, IPointerExitHandler
{
    public Text selectedText;
    public string whichChar;
    public Image spriteReference;
    private PlayerUnit charReference;
    private bool dragging = false;
    

    // Start is called before the first frame update
    void Start()
    {
        foreach (Unit u in Level.instance.playerUnits)
            if (u.gameObject.name.Contains(whichChar))
            {
                charReference = u.GetComponent<PlayerUnit>();
                break;
            }
        
        charReference.GetComponent<SpriteRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        selectedText.text = "" + Level.instance.selectedUnits.Count + " / 3";
        if (!charReference.GetComponent<SpriteRenderer>().enabled)
            spriteReference.color = Color.white;
    }

    public void OnDrag(PointerEventData eventData)
    {
        StartCoroutine(dragUnit());
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    spriteReference.material.SetColor("_Color", Color.white);
    //    spriteReference.material.SetFloat("_Thickness", 0.0025f);
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    spriteReference.material.SetColor("_Color", Color.white);
    //    spriteReference.material.SetFloat("_Thickness", 0f);
    //}

    IEnumerator dragUnit()
    {
        if (dragging)
            yield break;

        if (Level.instance.selectedUnits.Contains(charReference))
            Level.instance.selectedUnits.RemoveAt(Level.instance.selectedUnits.IndexOf(charReference));



        dragging = true;
        spriteReference.color = Color.black;
        charReference.GetComponent<SpriteRenderer>().enabled = true;
        charReference.animator.SetTrigger("Walking");
        charReference.GetComponent<SpriteRenderer>().sortingOrder = 3;

        while (Input.GetMouseButton(0))
        {
            charReference.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            yield return null;
        }

        Vector3 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        endPoint.z = 0;
        foreach (Vector3 position in Level.instance.startPositions)
        {
            if (Mathf.Abs(Vector3.Distance(endPoint, position)) <= 0.7f)
            {
                PlayerUnit switchWith = (PlayerUnit)Level.instance.getUnitAtLoc(position);
                if (switchWith != null)
                {
                    switchWith.GetComponent<SpriteRenderer>().enabled = false;
                    switchWith.movePoint.transform.position = new Vector3(-1, -1, 0);
                    switchWith.transform.position = new Vector3(-1, -1, 0);

                    Level.instance.selectedUnits.RemoveAt(Level.instance.selectedUnits.IndexOf(switchWith));
                    charReference.transform.position = position;
                    charReference.movePoint.transform.position = position;
                    charReference.animator.SetTrigger("Stopped");
                    Level.instance.selectUnit(charReference);
                    dragging = false;
                    yield break;
                }
                else
                {
                    charReference.transform.position = position;
                    charReference.movePoint.position = position;
                    charReference.animator.SetTrigger("Stopped");
                    dragging = false;
                    Level.instance.selectUnit(charReference);
                    charReference.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    yield break;
                }
            }
        }

        charReference.animator.SetTrigger("Stopped");
        charReference.GetComponent<SpriteRenderer>().enabled = false;
        spriteReference.color = Color.white;
        dragging = false;
        charReference.GetComponent<SpriteRenderer>().sortingOrder = 1;
    }
}
