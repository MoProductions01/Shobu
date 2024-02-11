
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shobu : MonoBehaviour
{    
    public static int NUM_BOARDS = 4;
                 
    [SerializeField] List<Board> Boards = new List<Board>();
    Rock HeldRock;
    int RockMask, BoardSpaceMask;


    // Start is called before the first frame update
    void Start()
    {        
        ResetBoards();
        RockMask = LayerMask.GetMask("Rock");
        BoardSpaceMask = LayerMask.GetMask("Board Space");
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

    RaycastHit RayCast(int layerMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;       
        Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        return hit;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {                        
            RaycastHit hit = RayCast(RockMask);
            if(hit.collider != null)
            {
                HeldRock = hit.collider.GetComponent<Rock>();
                Board hitRockBoard = HeldRock.GetComponentInParent<Board>();
                HeldRock.transform.localScale *= 1.2f;
                Debug.Log("clicked on: " + hitRockBoard.name + " - " + HeldRock.name);                
            }            
        }
        else if(Input.GetMouseButton(0) && HeldRock != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HeldRock.transform.position = new Vector3(mouseWorld.x, mouseWorld.y, 0f);
        }
        else if(Input.GetMouseButtonUp(0) && HeldRock != null)
        {
            RaycastHit hit = RayCast(BoardSpaceMask);
            if(hit.collider != null)
            {
                //Debug.Log("let go over: " + hit.collider.name);
                BoardSpace hitBoardSpace = hit.collider.GetComponent<BoardSpace>();
                Board hitBoardSpaceBoard = hitBoardSpace.GetComponentInParent<Board>();
                //Debug.Log("released on BoardSpace: " + hitBoardSpaceBoard.name + " - " + hitBoardSpace.name);  
               // Debug.Log("HeldRock Board: " + HeldRock.MyBoard.name + ", BoardSpace Board: " + hitBoardSpaceBoard.name);     
                if(HeldRock.MyBoard == hitBoardSpaceBoard)
                {
                  //  Debug.Log("Let go on correct board");
                    if(hitBoardSpace.GetComponentInChildren<Rock>() != null)
                    {
                   //     Debug.Log("There's a rock on this space so don't put it here");
                    }
                    else
                    {
                     //   Debug.Log("Space is free so place rock");
                        HeldRock.transform.parent = hitBoardSpace.transform;
                    }
                }
                else
                {
                   // Debug.Log("Incorrect Board");
                }
                HeldRock.transform.localPosition = Vector3.zero;
            }            
            else
            {
               // Debug.Log("Released over a non BoardSpace");
                HeldRock.MyBoard.PutRockOnPushedList(HeldRock);        
            }   
            HeldRock.transform.localScale /= 1.2f;         
            HeldRock = null;
        }
    }
}
