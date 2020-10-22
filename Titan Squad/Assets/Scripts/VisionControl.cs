using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionControl : MonoBehaviour
{
    [SerializeField]
    GameObject parent;

    int collidingWith = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Light")
        {
            parent.GetComponent<SpriteRenderer>().enabled = true;
            collidingWith++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Light")
        {
            collidingWith--;
            if (collidingWith == 0)
                parent.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
