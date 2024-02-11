using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shobu : MonoBehaviour
{    
    public static int NUM_BOARDS = 4;
                 
    [SerializeField] List<Board> Boards = new List<Board>();

    int RockMask;


    // Start is called before the first frame update
    void Start()
    {        
        ResetBoards();
        RockMask = LayerMask.GetMask("Rock");
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

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);            
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, RockMask))
            {
                Rock hitRock = hit.collider.GetComponent<Rock>();
                Board hitRockBoard = hitRock.GetComponentInParent<Board>();
                Debug.Log("clicked on: " + hitRockBoard.name + " - " + hitRock.name);                
            }            
        }
    }
}
