
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor;
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
    Vector2Int PassiveMove = new Vector2Int(-1, -1);
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
        PassiveMove = Vector2Int.zero;

        // Debug
        Board light0 = Boards[1];
        Rock rock;
        
        for(int i=0; i<4; i++)
        {
            rock = light0.transform.GetChild(0).GetChild(i).GetComponentInChildren<Rock>();
            rock.transform.parent = light0.transform.GetChild(2).GetChild(i).transform;
            rock.transform.localPosition = Vector3.zero; 
        }
        //rock = light0.transform.GetChild(0).GetChild(1).GetComponentInChildren<Rock>();
        //rock.transform.parent = light0.transform.GetChild(2).GetChild(0).transform;
        //rock.transform.localPosition = Vector3.zero;

       /* rock = light0.transform.GetChild(3).GetChild(0).GetComponentInChildren<Rock>();
        rock.transform.parent = light0.transform.GetChild(2).GetChild(0).transform;
        rock.transform.localPosition = Vector3.zero;

        rock = light0.transform.GetChild(3).GetChild(1).GetComponentInChildren<Rock>();
        rock.transform.parent = light0.transform.GetChild(1).GetChild(1).transform;
        rock.transform.localPosition = Vector3.zero;

        rock = light0.transform.GetChild(3).GetChild(2).GetComponentInChildren<Rock>();
        rock.transform.parent = light0.transform.GetChild(2).GetChild(1).transform;
        rock.transform.localPosition = Vector3.zero;*/

     //   rock = light0.transform.GetChild(3).GetChild(3).GetComponentInChildren<Rock>();
      //  rock.transform.parent = light0.transform.GetChild(3).GetChild(0).transform;
       // rock.transform.localPosition = Vector3.zero;
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
        DebugText.text += "PassiveMove: " + PassiveMove.ToString() + "\n";
        if(HeldRock == null) DebugText.text += "No HeldRock\n";
        else 
        {
            DebugText.text += "HeldRock at: " + HeldRock.GetComponentInParent<BoardSpace>().SpaceCoords.ToString() + "\n";        
            DebugText.text += "Num valid moves: " + HeldRock.MyBoard.ValidMoves.Count + "\n";
            foreach(BoardSpace boardSpaces in HeldRock.MyBoard.ValidMoves)
            {
                DebugText.text += boardSpaces.SpaceCoords.ToString() + "\n";
            }
            if(HeldRock.MyBoard.PushedRock != null)
            {
                
            }
        }

        if(Input.GetMouseButtonDown(0))
        {                        
            RaycastHit hit = RayCast(RockMask);
            if(hit.collider != null)
            {
                Rock rock = hit.collider.GetComponent<Rock>();
                Board hitRockBoard = rock.GetComponentInParent<Board>();
                //HeldRock = hit.collider.GetComponent<Rock>();
                if(ValidBoards.Contains(hitRockBoard) == false)
                {
                    Debug.LogWarning("Invalid Board to pick up rock");
                }
                else if(CurrentPlayer != rock.RockColor)
                {
                    Debug.LogWarning("Invalid rock color");                 
                }                                
                else
                {
                    Debug.Log("-----------------------------clicked on valid board and rock: " + hitRockBoard.name + " - " + rock.name);                      
                    if(CurrentMove == eMoveType.PASSIVE)
                    {   
                        if(rock.MyBoard.UpdatePassiveValidMoves(rock, eMoveType.PASSIVE) == false)
                        {
                            Debug.Log("No valid passive moves");                            
                        }
                        else
                        {
                            HeldRock = rock;
                            HeldRock.transform.localScale *= 1.2f;
                        }
                    }    
                    else
                    {
                        if(rock.MyBoard.CheckAggressiveMove(rock, PassiveMove))
                        {
                            Debug.Log("Can grab this piece");
                            HeldRock = rock;
                            HeldRock.transform.localScale *= 1.2f;
                        }
                        else
                        {                            
                            Debug.Log("No valid Aggressive move");
                        }
                    }                                 
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
                        if(CurrentMove == eMoveType.PASSIVE)
                        {
                            PassiveMove = hitBoardSpace.SpaceCoords - HeldRock.GetComponentInParent<BoardSpace>().SpaceCoords;
                        }                                                                                    
                        else
                        {
                            HeldRock.MyBoard.CheckPushedRock();
                        }
                        HeldRock.transform.parent = hitBoardSpace.transform;                        
                        EndMove(hitBoardSpaceBoard);                                                                        
                    }
                    else
                    {                        
                        Debug.LogWarning("Valid Board and Rock but Invalid Move");
                        HeldRock.MyBoard.ResetSpaceHighlights();
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
                Debug.Log("Released over a non BoardSpace");
               // HeldRock.MyBoard.PutRockOnPushedList(HeldRock);        
            }   
            HeldRock.transform.localScale /= 1.2f;         
            HeldRock.transform.localPosition = Vector3.zero;
            HeldRock.MyBoard.ResetSpaceHighlights();
            HeldRock = null;            
        }
        else if(Input.GetMouseButtonUp(0))
        {
            foreach(Board board in Boards)
            {
                board.ResetSpaceHighlights();
            }
        }
    }
    
    void EndMove(Board moveBoard)
    {
        ValidBoards.Clear();
        
        if(CurrentMove == eMoveType.PASSIVE)
        {
            CurrentMove = eMoveType.AGGRESSIVE;                   
            if(moveBoard.BoardColor == Board.eBoardColor.DARK)
            {                
                ValidBoards.AddRange(new List<Board>{Boards[1], Boards[3]});
            }
            else
            {
                ValidBoards.AddRange(new List<Board>{Boards[0], Boards[2]});
            }                    
        }
        else
        {
            if(moveBoard.CheckEndGame() == true)
            {
                Debug.Log("GAME OVER!!! " + HeldRock.RockColor.ToString() + " WINS!!!!!!");
            }
            PassiveMove = Vector2Int.zero;
            CurrentMove = eMoveType.PASSIVE;
            CurrentPlayer = (eGameColors)( 1 - (int)CurrentPlayer);
            if(CurrentPlayer == eGameColors.BLACK)
            {                
                ValidBoards.AddRange(new List<Board>{Boards[0], Boards[1]});
            }
            else
            {
                ValidBoards.AddRange(new List<Board>{Boards[2], Boards[3]});
            } 
        }
    }
}
