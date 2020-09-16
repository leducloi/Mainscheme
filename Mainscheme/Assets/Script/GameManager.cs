using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public BaseGrid grid;
    public bool playerPhase = false;
    public bool enemyPhase = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        InitGame();
    }

    // Update is called once per frame
    void InitGame()
    {
        grid = new BaseGrid();
        playerPhase = true;
    }

    public void endPlayerTurn()
    {
        playerPhase = false;
        enemyPhase = true;
    }

    public void endEnemyTurn()
    {
        playerPhase = true;
        enemyPhase = false;
    }
}
