using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Item : MonoBehaviour
{
    public Sprite itemImage;
    public string itemName;
    public bool used = false;
    public string itemDescription;

    public abstract void activate();
}
