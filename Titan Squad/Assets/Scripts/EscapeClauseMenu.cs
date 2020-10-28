using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeClauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent.gameObject.GetComponent<Canvas>().enabled)
            smartPosition();
    }

    private void smartPosition()
    {
        Vector3 location = UIManager.instance.currUnit.transform.position;
        Vector3 screenLoc = Camera.main.WorldToScreenPoint(location);

        RectTransform rt = GetComponent<RectTransform>();


        if (screenLoc.y > Screen.height / 2)
        {
            Vector2 setting = new Vector2(.5f, 1f);
            rt.anchorMin = setting;
            rt.anchorMax = setting;
            rt.pivot = setting;

            location.y -= .5f;
        }
        else
        {
            Vector2 setting = new Vector2(.5f, 0);
            rt.anchorMin = setting;
            rt.anchorMax = setting;
            rt.pivot = setting;

            location.y += 1f;
        }

        rt.position = Camera.main.WorldToScreenPoint(location);
    }
}
