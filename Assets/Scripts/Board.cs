using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static int NUM_ROWS_COLS = 4;

    public enum eBoardColor {DARK, LIGHT};
    [field: SerializeField] public eBoardColor BoardColor {get; set;} 

    GameObject[,] BoardSpaces = new GameObject[NUM_ROWS_COLS, NUM_ROWS_COLS];

    // Start is called before the first frame update
    void Start()
    {
        ResetRocks();
    }

    // Update is called once per frame
    void Update()
    {
        int i=0; 
        i++;
    }

    void ResetRocks()
    {                
        Array.Clear(BoardSpaces, 0, BoardSpaces.Length);
        
        for(int i=0; i<NUM_ROWS_COLS; i++)
        {
            BoardSpaces[0, i] = transform.GetChild(i).gameObject;
            BoardSpaces[3, i] = transform.GetChild(i + NUM_ROWS_COLS).gameObject;
        }
    }
}
