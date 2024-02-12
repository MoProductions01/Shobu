
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Shobu : MonoBehaviour
{    
    public static int NUM_BOARDS = 4;
                 
    public enum eGameColors {BLACK, WHITE};
    eGameColors CurrentPlayer;
    public enum eMoveType {PASSIVE, AGGRESSIVE};  
    eMoveType CurrentMove;          
    [SerializeField] List<Board> Boards = new List<Board>();
    Rock HeldRock;
    int RockMask, BoardSpaceMask;

    List<Board> ValidBoards = new List<Board>();

    public TMP_Text DebugText;


    // Start is called before the first frame update
    void Start()
    {        
        ResetGame();
        RockMask = LayerMask.GetMask("Rock");
        BoardSpaceMask = LayerMask.GetMask("Board Space");
    }

    void ResetGame()
    {
        ResetBoards();
        CurrentPlayer = eGameColors.BLACK;
        CurrentMove = eMoveType.PASSIVE;
        ValidBoards.Add(Boards[0]);
        ValidBoards.Add(Boards[1]);
    }

    public void ResetBoards()
    {
        for(int i=0; i<NUM_BOARDS; i++)
        {
            Boards[i].ResetBoard();
        }
    }

    public void ResetGameDebug()
    {
        ResetGame();
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
        DebugText.text = "Current Player: " + CurrentPlayer.ToString() + "\n";
        DebugText.text += "Current Move: " + CurrentMove.ToString() + "\n";
        if(Input.GetMouseButtonDown(0))
        {                        
            RaycastHit hit = RayCast(RockMask);
            if(hit.collider != null)
            {
                Rock rock = hit.collider.GetComponent<Rock>();
                Board hitRockBoard = rock.GetComponentInParent<Board>();
                //HeldRock = hit.collider.GetComponent<Rock>();
                if(CurrentPlayer != rock.RockColor)
                {
                    Debug.Log("Invalid rock color");                 
                }                
                else if(ValidBoards.Contains(hitRockBoard) == false)
                {
                    Debug.Log("Invalid Board to pick up rock");
                }
                else
                {
                    Debug.Log("clicked on valid board and rock: " + hitRockBoard.name + " - " + rock.name);  
                    HeldRock = rock;
                    HeldRock.transform.localScale *= 1.2f;
                    HeldRock.MyBoard.UpdateValidMoves(HeldRock, eMoveType.PASSIVE);                          
                }                        
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
                    if(hitBoardSpaceBoard.IsValidMove(hitBoardSpace))
                    {
                        Debug.Log("Valid Move");
                        //DebugText.text = "Valid Move";
                        HeldRock.transform.parent = hitBoardSpace.transform;
                    }
                    else
                    {                        
                        Debug.Log("Valid Board and Rock but Invalid Move");
                        //DebugText.text = "Valid Board but Invalid Move";
                    }
                  //  Debug.Log("Let go on correct board");
                   /* if(hitBoardSpace.GetComponentInChildren<Rock>() != null)
                    {
                   //     Debug.Log("There's a rock on this space so don't put it here");
                    }
                    else
                    {
                     //   Debug.Log("Space is free so place rock");
                        HeldRock.transform.parent = hitBoardSpace.transform;
                    }*/
                }
                else
                {
                    Debug.Log("Invalid Board");
                    //DebugText.text = "Invalid Board";
                }               
            }            
            else
            {
               // Debug.Log("Released over a non BoardSpace");
               // HeldRock.MyBoard.PutRockOnPushedList(HeldRock);        
            }   
            HeldRock.transform.localScale /= 1.2f;         
            HeldRock.transform.localPosition = Vector3.zero;
            HeldRock.MyBoard.ResetSpaceHighlights();
            HeldRock = null;            
        }
    }
}
