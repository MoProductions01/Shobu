using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shobu : MonoBehaviour
{    
    public static int NUM_BOARDS = 4;
     
    [SerializeField] public GameObject BoardSquareLocations {get; set;}
    [SerializeField] Board[] Boards = new Board[NUM_BOARDS];    


    // Start is called before the first frame update
    void Start()
    {        
                       
    }

    public void ResetBoards()
    {
        for(int i=0; i<NUM_BOARDS; i++)
        {
            Boards[i].ResetBoard(i);
        }
    }
}
