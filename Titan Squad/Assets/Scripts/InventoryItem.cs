using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public GameObject hoverText;

    private bool dragging = false;

    public Item itemRep;

    public Image itemImage;
    public Text itemText;

    private string itemDescription = "Default description";

    public bool hovering = false;

    private Color hoverColor = new Color(85f / 255f, 60f / 255f, 103f / 255f);
    private Color normalColor = new Color(58f / 255f, 50f / 255f, 62f / 255f);
    

    // Start is called before the first frame update
    void Start()
    {
        itemImage.raycastTarget = false;
        itemText.raycastTarget = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (itemRep == null)
            Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        if (!Input.GetMouseButton(0))
            GetComponent<Image>().color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        if (!Input.GetMouseButton(0))
            GetComponent<Image>().color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Level.instance.donePlanning && !InventoryManager.instance.trading)
        {
            itemRep.activate();
            InventoryManager.instance.hideInventory();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        StartCoroutine(dragItem());
    }

    IEnumerator dragItem()
    {
        if (dragging)
            yield break;

        bool foundSlot = false;
        GetComponent<Image>().color = hoverColor;

        dragging = true;

        Transform originalParent = transform.parent;
        int originalIndex = transform.GetSiblingIndex();
        transform.SetParent(InventoryManager.instance.transform);
        GetComponent<Image>().raycastTarget = false;

        GameObject drop = null;
        GameObject temp = null;
        while (Input.GetMouseButton(0))
        {
            if (!foundSlot)
            {
                transform.position = Input.mousePosition;

                drop = InventoryManager.instance.getDropLocation();
                if (drop != null && drop.transform.parent.GetComponentsInChildren<InventoryItem>().Length >= 6)
                    drop = null;

                if (drop != null)
                {
                    temp = drop;

                    foundSlot = true;
                    Image raycaster = GetComponent<Image>();
                    raycaster.raycastTarget = true;
                    GetComponent<Outline>().enabled = true;

                    if (drop.GetComponent<InventoryItem>() == null)
                        transform.SetParent(drop.transform);
                    else
                    {
                        int index = drop.transform.GetSiblingIndex();
                        transform.SetParent(drop.transform.parent);
                        transform.SetSiblingIndex(index);
                    }
                }
            }
            else
            {
                if (temp != null)
                {
                    temp = InventoryManager.instance.getDropLocation();
                    if (temp != null && temp != drop)
                    {
                        drop = temp;
                        if (drop.GetComponent<InventoryItem>() == null)
                            transform.SetParent(drop.transform);
                        else
                        {
                            int index = drop.transform.GetSiblingIndex();
                            transform.SetParent(drop.transform.parent);
                            transform.SetSiblingIndex(index);
                        }
                    }
                }
                else
                {
                    GetComponent<Outline>().enabled = false;
                    foundSlot = false;
                    Image raycaster = GetComponent<Image>();
                    raycaster.raycastTarget = false;
                    transform.SetParent(InventoryManager.instance.transform);
                }
            }
            

            yield return null;
        }

        drop = InventoryManager.instance.getDropLocation();
        if (drop != null && drop.transform.parent.GetComponentsInChildren<InventoryItem>().Length >= 6)
            drop = null;

        Image imageRaycast = GetComponent<Image>();
        imageRaycast.raycastTarget = true;
        GetComponent<Outline>().enabled = false;

        if (drop == null)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalIndex);
        }
        else
        {
            if (drop.GetComponent<InventoryItem>() == null)
                transform.SetParent(drop.transform);
            else
            {
                int index = drop.transform.GetSiblingIndex();
                transform.SetParent(drop.transform.parent);
                transform.SetSiblingIndex(index);
            }
        }


        dragging = false;
    }

    public void set(Item invItem)
    {
        itemRep = invItem;
        itemImage.sprite = invItem.itemImage;
        itemText.text = invItem.itemName;
        itemDescription = invItem.itemDescription;
    }
}
