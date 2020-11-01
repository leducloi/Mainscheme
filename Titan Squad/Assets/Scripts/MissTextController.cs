using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissTextController : MonoBehaviour
{
    [SerializeField]
    private RectTransform bounds = null;

    // Start is called before the first frame update
    void Start()
    {
        bounds.position = CombatCalculator.instance.currEnemy.transform.position;
        StartCoroutine(missAnimation());
    }

    IEnumerator missAnimation()
    {
        Image image = GetComponentInChildren<Image>();
        Color startColor = image.color;
        Vector3 endPos = new Vector3(bounds.position.x, bounds.position.y + 1, 0);

        while (bounds.position.y != endPos.y)
        {
            bounds.position = Vector3.MoveTowards(bounds.position, endPos, 1f * Time.deltaTime);
            startColor.a = (endPos.y - bounds.position.y);
            image.color = startColor;
            yield return null;
        }

        Destroy(gameObject);
    }
}
