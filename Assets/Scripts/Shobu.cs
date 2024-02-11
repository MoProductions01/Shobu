using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shobu : MonoBehaviour
{    
    public static int NUM_BOARDS = 4;
                 
    [SerializeField] List<Board> Boards = new List<Board>();


    // Start is called before the first frame update
    void Start()
    {        
        ResetBoards();
    }

    public void ResetBoards()
    {
        for(int i=0; i<NUM_BOARDS; i++)
        {
            Boards[i].ResetBoard();
        }
    }

    public void ResetBoardDebug()
    {
        ResetBoards();
    }
}
