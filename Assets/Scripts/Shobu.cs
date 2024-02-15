
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor;
using UnityEngine;

public class Shobu : MonoBehaviour
{    
    public static int NUM_BOARDS = 4;

    public enum eGameState {PLAYING, GAME_OVER};
    eGameState GameState;
                 
    public enum eRockColors {BLACK, WHITE};
    eRockColors CurrentRockColor;
    eRockColors WinningColor;
    public enum eMoveType {PASSIVE, AGGRESSIVE};  
    eMoveType CurrentMove;          
    [SerializeField] List<Board> Boards = new List<Board>();    
    int RockMask, BoardSpaceMask;
    List<Board> ValidBoards = new List<Board>();    
    Rock HeldRock;    
    
    public TMP_Text DebugText;            
    
    void Start()
    {        
        ResetGame();
        RockMask = LayerMask.GetMask("Rock");
        BoardSpaceMask = LayerMask.GetMask("Board Space");
    }

    void ResetGame()
    {
        ResetBoards();
        RockMove.GetInstance().Reset(eMoveType.AGGRESSIVE);        
        CurrentRockColor = eRockColors.BLACK;
        CurrentMove = eMoveType.PASSIVE;
        ValidBoards.Add(Boards[0]);
        ValidBoards.Add(Boards[1]);        
        GameState = eGameState.PLAYING; 
        
        // Debug       
       // SetupRockDebug(Boards[0], new Vector2Int(0,0), new Vector2Int(0,2));        
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
        if(GameState == eGameState.GAME_OVER)
        {
            DebugText.text = WinningColor.ToString() + " WINS!!!!!!!";
            return;
        }        
        PrintDebugInfo();


        if(Input.GetMouseButtonDown(0))
        {                        
            RaycastHit hit = RayCast(RockMask);
            if(hit.collider != null)
            {
                Rock rock = hit.collider.GetComponent<Rock>();                
                if(ValidBoards.Contains(rock.MyBoard) == false)
                {
                    Debug.LogWarning("Invalid Board to pick up rock");
                }
                else if(CurrentRockColor != rock.RockColor)
                {
                    Debug.LogWarning("Invalid rock color");                 
                }                                
                else
                {
                    //Debug.Log("-----------------------------clicked on valid board and rock: " + hitRockBoard.name + " - " + rock.name);                      
                   // Debug.Log("-----------------------------clicked on valid board and rock: " + rock.MyBoard.name + " - " + rock.name);                                          
                    //RockMove.GetInstance().Reset();
                    if(CurrentMove == eMoveType.PASSIVE)
                    {   
                        if(rock.MyBoard.UpdatePossiblePassiveMoves(rock))
                        {
                            List<Board> boardsToCheck = new List<Board>(Boards);
                            boardsToCheck.RemoveAll(x => x.BoardColor == rock.MyBoard.BoardColor);
                            foreach(Board board in boardsToCheck)
                            {
                                board.CheckIfAnyValidAggressiveMoves(rock);
                            }  
                            BoardSpace clickedRockSpace = rock.GetComponentInParent<BoardSpace>();
                           // ValidPassiveMoves = (List<Vector2Int>)ValidPassiveMoves.Distinct().ToList();
                            //ValidBoardSpaces.Clear();
                            //Debug.Log("*******Num ValidPassiveMoves: " + ValidPassiveMoves.Count + " ****************** --CVA--");                                                                                                                                     
                           // foreach(Vector2Int passiveMove in ValidPassiveMoves)
                            //foreach(Vector2Int passiveMove in RockMove.GetInstance().GetValidPassiveMoves())
                            foreach(Vector2Int passiveMove in RockMove.GetInstance().ValidPassiveMoves)
                            {
                               // Debug.Log("Valid Passive Move: (" + passiveMove.ToString() + ") --CVA--");                                
                                Vector2Int moveCoords = clickedRockSpace.SpaceCoords + passiveMove;
                                rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y].ToggleHighlight(true, Color.blue);  
                                //RockMove.GetInstance().AddValidBoardSpace(rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);
                                RockMove.GetInstance().ValidBoardSpaces.Add(rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);
                                //ValidBoardSpaces.Add(rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);                              
                            }

                            //List<Vector2Int> invalidPassiveMoves = PassiveMovesToCheck.Except(ValidPassiveMoves).ToList();  
                            //List<Vector2Int> invalidPassiveMoves = RockMove.GetInstance().PassiveMoves().Except(ValidPassiveMoves).ToList();   
                            //Debug.Log("*******Num invalidPassiveMoves: " + invalidPassiveMoves.Count + " ****************** --CVA--");                               
                            
                            foreach(Vector2Int passiveMove in RockMove.GetInstance().GetInvalidPassiveMoves())
                            {
                               // Debug.Log("inValid Passive Move: (" + passiveMove.ToString() + ") --CVA--");                                               
                                Vector2Int moveCoords = clickedRockSpace.SpaceCoords + passiveMove;
                                BoardSpace bs = rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y];
                                bs.ToggleHighlight(true, Color.red);
                               // rock.MyBoard.ValidMoves.Remove(bs);
                            }  
                            if(RockMove.GetInstance().ValidPassiveMoves.Count != 0)
                            {
                                HeldRock = rock;
                                HeldRock.transform.localScale *= 1.2f;
                            }
                            else
                            {
                                Debug.LogWarning("NO VALID AGGRESSIVE MOVES MOVES!!");                                
                            }                                               
                        }
                        else
                        {                                                        
                             Debug.LogWarning("No possible passive moves");           
                        }
                    }    
                    else
                    {
                        Vector2Int moveCoords = rock.GetComponentInParent<BoardSpace>().SpaceCoords + RockMove.GetInstance().PassiveMove;
                        if(rock.MyBoard.CheckAggressiveMove(rock, RockMove.GetInstance().PassiveMove, false))
                        {                          
                            HeldRock = rock;
                            HeldRock.transform.localScale *= 1.2f;                            
                            //ValidBoardSpaces.Add(HeldRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);   
                            // monote - make BoardSpaces private and use an accessor                                
                            //RockMove.GetInstance().AddValidBoardSpace(HeldRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);
                            RockMove.GetInstance().ValidBoardSpaces.Add(HeldRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);
                            HeldRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y].ToggleHighlight(true, Color.blue);                               
                        }
                        else
                        {                            
                            Debug.LogWarning("No valid Aggressive move");
                            if(rock.MyBoard.CheckPassiveSpace(new Vector2Int(moveCoords.x, moveCoords.y)))
                            {
                                rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y].ToggleHighlight(true, Color.red);       
                            }
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
                    //if(hitBoardSpaceBoard.IsValidMove(hitBoardSpace))
                    //if(ValidBoardSpaces.Contains(hitBoardSpace))
                    if(RockMove.GetInstance().ValidBoardSpaces.Contains(hitBoardSpace))
                    {
                       // Debug.Log("Valid Move");           
                        if(CurrentMove == eMoveType.PASSIVE)
                        {
                            RockMove.GetInstance().PassiveMove = hitBoardSpace.SpaceCoords - HeldRock.GetComponentInParent<BoardSpace>().SpaceCoords;
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
        
        RockMove.GetInstance().Reset(CurrentMove);
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
                GameState = eGameState.GAME_OVER;
                WinningColor = HeldRock.RockColor;
                //Debug.Log("GAME OVER!!! " + WinningColor.ToString() + " WINS!!!!!!");
            }
            RockMove.GetInstance().PassiveMove = Vector2Int.zero;
            CurrentMove = eMoveType.PASSIVE;
            CurrentRockColor = (eRockColors)( 1 - (int)CurrentRockColor);
            if(CurrentRockColor == eRockColors.BLACK)
            {                
                ValidBoards.AddRange(new List<Board>{Boards[0], Boards[1]});
            }
            else
            {
                ValidBoards.AddRange(new List<Board>{Boards[2], Boards[3]});
            } 
        }        
    }

    void PrintDebugInfo()
    {
        DebugText.text = "Current Player: " + CurrentRockColor.ToString() + "\n";
        DebugText.text += "Current Move: " + CurrentMove.ToString() + "\n";
        if(RockMove.GetInstance().PassiveMove != Vector2Int.zero) DebugText.text += "PassiveMove: " + RockMove.GetInstance().PassiveMove.ToString() + "\n";
        if(HeldRock == null) DebugText.text += "No HeldRock\n";
        else 
        {
            DebugText.text += "HeldRock at: " + HeldRock.GetComponentInParent<BoardSpace>().SpaceCoords.ToString() + "\n";                     
        }
        if(RockMove.GetInstance().ValidBoardSpaces.Count != 0)
        {
            DebugText.text += "\nValid BoardSpaces:\n";
            foreach(BoardSpace boardSpace in RockMove.GetInstance().ValidBoardSpaces)
            {
                DebugText.text += "(" + boardSpace.SpaceCoords.x + ", " + boardSpace.SpaceCoords.y + ")";
            }
        }
    }
}
