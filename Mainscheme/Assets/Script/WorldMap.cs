using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
    [SerializeField] 
    private BaseGrid baseGrid;
    private void Start()
    {
        //pathFinding = baseGrid.GetPathFinding();
    }

    private void Update()
    {}
}
