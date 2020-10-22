using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionControl : MonoBehaviour
{
    [SerializeField]
    GameObject parent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Light")
            parent.GetComponent<SpriteRenderer>().enabled = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Light")
            parent.GetComponent<SpriteRenderer>().enabled = false;
    }
}
