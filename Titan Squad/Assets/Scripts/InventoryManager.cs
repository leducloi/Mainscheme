using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    [SerializeField]
    private Sprite[] sprites = null;

    public GameObject inventory1;
    public Text owner1;
    public Image image1;
    
    public GameObject inventory2;
    public Text owner2;
    public Image image2;

    public GameObject inventory3;
    public Text owner3;
    public Image image3;

    public GameObject bag1;
    public GameObject bag2;
    public GameObject bag3;

    [SerializeField]
    private GameObject inventoryItem = null;

    public bool displaying = false;

    public bool trading = false;

    // Start is called before the first frame update
    void Start()
    {
        //If there are no instances of InventoryManager, just set it to this
        if (instance == null)
            instance = this;
        //If there is more than one instance of InventoryManager, destroy the copy and reset it
        else if (instance != this)
        {
            Destroy(instance);
            instance = this;
        }

        inventory1.SetActive(false);
        inventory2.SetActive(false);
        inventory3.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Level.instance != null && !Level.instance.donePlanning && !displaying)
        {
            //Debug.Log(Level.instance.selectedUnits.Count);
            switch (Level.instance.selectedUnits.Count)
            {
                case 3:
                    PlayerUnit unit3 = Level.instance.selectedUnits[2];
                    if (unit3.name == owner3.text)
                        goto case 2;
                    else
                    {
                        foreach (Transform o in bag3.transform)
                            Destroy(o.gameObject);
                    }
                    
                    switch(unit3.name)
                    {
                        case "Kennedy":
                            image3.sprite = sprites[0];
                            goto default;
                        case "Haley":
                            image3.sprite = sprites[1];
                            goto default;
                        case "Santias":
                            image3.sprite = sprites[2];
                            goto default;
                        default:
                            owner3.text = unit3.name;
                            break;
                    }

                    for (int x = 0; x < unit3.inventory.Count; x++)
                    {
                        GameObject newItem = Instantiate(inventoryItem, bag3.transform);
                        newItem.GetComponent<InventoryItem>().set(unit3.inventory[x]);
                    }
                    goto case 2;
                case 2:
                    PlayerUnit unit2 = Level.instance.selectedUnits[1];
                    if (unit2.name == owner2.text)
                        goto case 1;
                    else
                    {
                        foreach (Transform o in bag2.transform)
                            Destroy(o.gameObject);
                    }

                    switch (unit2.name)
                    {
                        case "Kennedy":
                            image2.sprite = sprites[0];
                            goto default;
                        case "Haley":
                            image2.sprite = sprites[1];
                            goto default;
                        case "Santias":
                            image2.sprite = sprites[2];
                            goto default;
                        default:
                            owner2.text = unit2.name;
                            break;
                    }

                    for (int x = 0; x < unit2.inventory.Count; x++)
                    {
                        GameObject newItem = Instantiate(inventoryItem, bag2.transform);
                        newItem.GetComponent<InventoryItem>().set(unit2.inventory[x]);
                    }
                    goto case 1;
                case 1:
                    PlayerUnit unit1 = Level.instance.selectedUnits[0];
                    if (unit1.name == owner1.text)
                        break;
                    else
                    {
                        foreach (Transform o in bag1.transform)
                            Destroy(o.gameObject);
                    }

                    switch (unit1.name)
                    {
                        case "Kennedy":
                            image1.sprite = sprites[0];
                            goto default;
                        case "Haley":
                            image1.sprite = sprites[1];
                            goto default;
                        case "Santias":
                            image1.sprite = sprites[2];
                            goto default;
                        default:
                            owner1.text = unit1.name;
                            break;
                    }

                    for (int x = 0; x < unit1.inventory.Count; x++)
                    {
                        GameObject newItem = Instantiate(inventoryItem, bag1.transform);
                        newItem.GetComponent<InventoryItem>().set(unit1.inventory[x]);
                    }
                    break;

                default:
                    break;
            }
        }

        if (displaying && Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.instance.playMenuDown();
            hideInventory();
            if (Level.instance.donePlanning)
                UIManager.instance.showActionMenu();
        }

    }

    public void hideInventory()
    {
        if (inventory1.activeInHierarchy)
        {
            List <Item> newInventory = new List<Item>();
            foreach (InventoryItem item in bag1.GetComponentsInChildren<InventoryItem>())
            {
                newInventory.Add(item.itemRep);
            }
            Level.instance.selectedUnits[0].setInventory(newInventory);
        }
        if (inventory2.activeInHierarchy)
        {
            List<Item> newInventory = new List<Item>();
            foreach (InventoryItem item in bag2.GetComponentsInChildren<InventoryItem>())
            {
                newInventory.Add(item.itemRep);
            }
            Level.instance.selectedUnits[1].setInventory(newInventory);
        }
        if (inventory3.activeInHierarchy)
        {
            List<Item> newInventory = new List<Item>();
            foreach (InventoryItem item in bag3.GetComponentsInChildren<InventoryItem>())
            {
                newInventory.Add(item.itemRep);
            }
            Level.instance.selectedUnits[2].setInventory(newInventory);
        }
        inventory1.SetActive(false);
        inventory2.SetActive(false);
        inventory3.SetActive(false);


        trading = false;
        displaying = false;
    }

    public void displayInventory(List<PlayerUnit> unitsToDisplay)
    {
        displaying = true;
        foreach (PlayerUnit u in unitsToDisplay)
        {
            if (u == Level.instance.selectedUnits[0])
                inventory1.SetActive(true);
            else if (u == Level.instance.selectedUnits[1])
                inventory2.SetActive(true);
            else if (u == Level.instance.selectedUnits[2])
                inventory3.SetActive(true);
        }
    }

    public GameObject getDropLocation()
    {
        if (inventory1.activeInHierarchy)
        {
            foreach (InventoryItem item in inventory1.GetComponentsInChildren<InventoryItem>())
            {
                if (item.hovering)
                    return item.gameObject;
            }
        }

        if (inventory2.activeInHierarchy)
        {
            foreach (InventoryItem item in inventory2.GetComponentsInChildren<InventoryItem>())
            {
                if (item.hovering)
                    return item.gameObject;
            }
        }

        if (inventory3.activeInHierarchy)
        {
            foreach (InventoryItem item in inventory3.GetComponentsInChildren<InventoryItem>())
            {
                if (item.hovering)
                    return item.gameObject;
            }
        }

        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        PointerEventData pointerData = new PointerEventData(null);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(pointerData, results);

        
        foreach (RaycastResult rr in results)
        {
            if (rr.gameObject.name.Contains("Inventory 1"))
                return bag1;
            else if (rr.gameObject.name.Contains("Inventory 2"))
                return bag2;
            else if (rr.gameObject.name.Contains("Inventory 3"))
                return bag3;
        }

        return null;

    }
}
