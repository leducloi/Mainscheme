using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathArrowControl : MonoBehaviour
{
    public static PathArrowControl instance = null;
    // Start is called before the first frame update
    [SerializeField]
    private GameObject straightX;
    [SerializeField]
    private GameObject straightY;
    [SerializeField]
    private GameObject cornerTopLeft;
    [SerializeField]
    private GameObject cornerBottomLeft;
    [SerializeField]
    private GameObject cornerTopRight;
    [SerializeField]
    private GameObject cornerBottomRight;
    [SerializeField]
    private GameObject pointerDown;
    [SerializeField]
    private GameObject pointerUp;
    [SerializeField]
    private GameObject pointerLeft;
    [SerializeField]
    private GameObject pointerRight;

    private Transform cursorHolder;



    List<Tuple<GameObject, Vector3>> arrowSetMap;
    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(instance);
            instance = this;
        }
        //CollisionTile[] path = MapBehavior.instance.getPathTo(new Vector3(0.5f, 0.5f, 0f), new Vector3(5.5f, 5.5f, 0));
        arrowSetMap = new List<Tuple<GameObject, Vector3>>();
        instantiateAllArrows();
        hideAllArrows();
        cursorHolder = new GameObject("CursorHolder").transform;
        cursorHolder = Instantiate(cursorHolder);
    }

    void Update()
    {
        if (arrowSetMap.Count != 0)
        {
            renderArrowSetMap();
        }
        else
        {
            destroyAllArrows();
        }
    }

    public void destroyAllArrows()
    {
        foreach (Transform child in cursorHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
            arrowSetMap = new List<Tuple<GameObject, Vector3>>();
        }
    }

    void hideAllArrows()
    {
    }

    void instantiateAllArrows()
    {
    }

    public void setPathArrow(CollisionTile[] path)
    {
        //destroyAllArrows();
        int index = 0;
        while (index < path.Length - 2)
        {
            CollisionTile currentTile = path[index]; float x0 = currentTile.coordinate.x, y0 = currentTile.coordinate.y;
            CollisionTile nextFirstTile = path[index + 1]; float x1 = nextFirstTile.coordinate.x, y1 = nextFirstTile.coordinate.y;
            CollisionTile nextSecondTile = path[index + 2]; float x2 = nextSecondTile.coordinate.x, y2 = nextSecondTile.coordinate.y;
            if (y0 == y1 && y1 == y2)
            {
                Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(straightX, nextFirstTile.coordinate);
                arrowSetMap.Add(newArrow);
            }

            if (x0 == x1 && x1 == x2)
            {
                Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(straightY, nextFirstTile.coordinate);
                arrowSetMap.Add(newArrow);
            }

            if (x1 > x0 && x1 == x2 && y1 == y0)
            {
                if (y2 > y1)
                {
                    Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(cornerBottomRight, nextFirstTile.coordinate);
                    arrowSetMap.Add(newArrow);
                }
                else if (y1 > y2)
                {
                    Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(cornerTopRight, nextFirstTile.coordinate);
                    arrowSetMap.Add(newArrow);
                }

            }

            if (x1 < x0 && x1 == x2 && y1 == y0)
            {
                if (y2 > y1)
                {
                    Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(cornerBottomLeft, nextFirstTile.coordinate);
                    arrowSetMap.Add(newArrow);
                }
                else if (y2 < y1)
                {
                    Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(cornerTopLeft, nextFirstTile.coordinate);
                    arrowSetMap.Add(newArrow);
                }

            }

            if (x2 > x1 && y1 == y2 && x0 == x1)
            {
                if (y0 > y1)
                {
                    Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(cornerBottomLeft, nextFirstTile.coordinate);
                    arrowSetMap.Add(newArrow);
                }
                else if (y1 > y0)
                {
                    Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(cornerTopLeft, nextFirstTile.coordinate);
                    arrowSetMap.Add(newArrow);
                }
            }

            if (x2 < x1 && y1 == y2 && x0 == x1)
            {
                if (y0 < y1)
                {
                    Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(cornerTopRight, nextFirstTile.coordinate);
                    arrowSetMap.Add(newArrow);
                }
                else if (y0 > y1)
                {
                    Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(cornerBottomRight, nextFirstTile.coordinate);
                    arrowSetMap.Add(newArrow);
                }
            }

            index++;
        }

        CollisionTile endTile = path[path.Length - 1]; float endX = endTile.coordinate.x, endY = endTile.coordinate.y;
        CollisionTile beforeEndTile = path[path.Length - 2]; float bEndX = beforeEndTile.coordinate.x, bEndY = beforeEndTile.coordinate.y;
        if (endY == bEndY)
        {
            if (endX > bEndX)
            {
                Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(pointerRight, endTile.coordinate);
                arrowSetMap.Add(newArrow);
            }
            else if (endX < bEndX)
            {
                Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(pointerLeft, endTile.coordinate);
                arrowSetMap.Add(newArrow);
            }
        }

        if (endX == bEndX)
        {
            if (endY > bEndY)
            {
                Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(pointerUp, endTile.coordinate);
                arrowSetMap.Add(newArrow);
            }
            else if (endY < bEndY)
            {
                Tuple<GameObject, Vector3> newArrow = new Tuple<GameObject, Vector3>(pointerDown, endTile.coordinate);
                arrowSetMap.Add(newArrow);
            }
        }
    }

    void renderArrowSetMap()
    {
        
        foreach (Tuple<GameObject, Vector3> eachArrow in arrowSetMap)
        {
            GameObject newArrow = Instantiate(eachArrow.Item1, eachArrow.Item2, Quaternion.identity) as GameObject;
            newArrow.transform.SetParent(cursorHolder.transform);
            newArrow.SetActive(true);
        }
    }

}
