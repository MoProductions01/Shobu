
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
    Rock HeldRock;
    int RockMask, BoardSpaceMask;
    Vector2Int PassiveMove = new Vector2Int(-1, -1);
    List<Board> ValidBoards = new List<Board>();    
    public TMP_Text DebugText;    

    public static List<Vector2Int> PassiveMovesToCheck = new List<Vector2Int>();
    public static List<Vector2Int> ValidPassiveMoves =  new List<Vector2Int>();
    List<BoardSpace> ValidBoardSpaces = new List<BoardSpace>();
    
    // Start is called before the first frame update
    void Start()
    {        
        ResetGame();
        RockMask = LayerMask.GetMask("Rock");
        BoardSpaceMask = LayerMask.GetMask("Board Space");
    }

    void SetupRockDebug(Board board, Vector2Int curRockPos, Vector2Int newRockPos)
    {
        Rock rock;
        rock = board.transform.GetChild(curRockPos.y).GetChild(curRockPos.x).GetComponentInChildren<Rock>();
        rock.transform.parent = board.transform.GetChild(newRockPos.y).GetChild(newRockPos.x).transform;
        rock.transform.localPosition = Vector3.zero; 
    }

    void ResetGame()
    {
        ResetBoards();
        CurrentRockColor = eRockColors.BLACK;
        CurrentMove = eMoveType.PASSIVE;
        ValidBoards.Add(Boards[0]);
        ValidBoards.Add(Boards[1]);
        PassiveMove = Vector2Int.zero;
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
        DebugText.text = "Current Player: " + CurrentRockColor.ToString() + "\n";
        DebugText.text += "Current Move: " + CurrentMove.ToString() + "\n";
        if(PassiveMove != Vector2Int.zero) DebugText.text += "PassiveMove: " + PassiveMove.ToString() + "\n";
        if(HeldRock == null) DebugText.text += "No HeldRock\n";
        else 
        {
            DebugText.text += "HeldRock at: " + HeldRock.GetComponentInParent<BoardSpace>().SpaceCoords.ToString() + "\n";                     
        }
        if(ValidBoardSpaces.Count != 0)
        {
            DebugText.text += "\nValid BoardSpaces:\n";
            foreach(BoardSpace boardSpace in ValidBoardSpaces)
            {
                DebugText.text += "(" + boardSpace.SpaceCoords.x + ", " + boardSpace.SpaceCoords.y + ")";
            }
        }
        
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
                    if(CurrentMove == eMoveType.PASSIVE)
                    {   
                        if(rock.MyBoard.UpdatePossiblePassiveMoves(rock, eMoveType.PASSIVE) == true)
                        {
                            List<Board> boardsToCheck = new List<Board>(Boards);
                            boardsToCheck.RemoveAll(x => x.BoardColor == rock.MyBoard.BoardColor);
                            foreach(Board board in boardsToCheck)
                            {
                                board.CheckIfAnyValidAggressiveMoves(rock);
                            }  
                            BoardSpace clickedRockSpace = rock.GetComponentInParent<BoardSpace>();
                            ValidPassiveMoves = (List<Vector2Int>)ValidPassiveMoves.Distinct().ToList();
                            ValidBoardSpaces.Clear();
                            //Debug.Log("*******Num ValidPassiveMoves: " + ValidPassiveMoves.Count + " ****************** --CVA--");                                                                                                                                     
                            foreach(Vector2Int passiveMove in ValidPassiveMoves)
                            {
                               // Debug.Log("Valid Passive Move: (" + passiveMove.ToString() + ") --CVA--");                                
                                Vector2Int moveCoords = clickedRockSpace.SpaceCoords + passiveMove;
                                rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y].ToggleHighlight(true, Color.blue);  
                                ValidBoardSpaces.Add(rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);                              
                            }

                            List<Vector2Int> invalidPassiveMoves = PassiveMovesToCheck.Except(ValidPassiveMoves).ToList();     
                            //Debug.Log("*******Num invalidPassiveMoves: " + invalidPassiveMoves.Count + " ****************** --CVA--");                               
                            
                            foreach(Vector2Int passiveMove in invalidPassiveMoves)
                            {
                               // Debug.Log("inValid Passive Move: (" + passiveMove.ToString() + ") --CVA--");                                               
                                Vector2Int moveCoords = clickedRockSpace.SpaceCoords + passiveMove;
                                BoardSpace bs = rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y];
                                bs.ToggleHighlight(true, Color.red);
                               // rock.MyBoard.ValidMoves.Remove(bs);
                            }  
                            if(ValidPassiveMoves.Count != 0)
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
                        Vector2Int moveCoords = rock.GetComponentInParent<BoardSpace>().SpaceCoords + PassiveMove;
                        if(rock.MyBoard.CheckAggressiveMove(rock, PassiveMove, false))
                        {                          
                            HeldRock = rock;
                            HeldRock.transform.localScale *= 1.2f;                            
                            ValidBoardSpaces.Add(HeldRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);                                   
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
                    if(ValidBoardSpaces.Contains(hitBoardSpace))
                    {
                       // Debug.Log("Valid Move");           
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
            ValidBoardSpaces.Clear();
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
                GameState = eGameState.GAME_OVER;
                WinningColor = HeldRock.RockColor;
                Debug.Log("GAME OVER!!! " + WinningColor.ToString() + " WINS!!!!!!");
            }
            PassiveMove = Vector2Int.zero;
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
}
