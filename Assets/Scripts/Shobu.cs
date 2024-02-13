
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

    public struct RockMove
    {
        public Board.eMoveDirs moveDir;
        public int numSpaces;

        public RockMove(Board.eMoveDirs moveDir, int numSpaces)
        {
            this.moveDir = moveDir;
            this.numSpaces = numSpaces;
        }
    }

    public static List<RockMove> PassiveMovesToCheck = new List<RockMove>();
    public static List<RockMove> ValidRockMoves =  new List<RockMove>();
    
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
        CurrentRockColor = eRockColors.BLACK;
        CurrentMove = eMoveType.PASSIVE;
        ValidBoards.Add(Boards[0]);
        ValidBoards.Add(Boards[1]);
        PassiveMove = Vector2Int.zero;
        GameState = eGameState.PLAYING; 
        // Debug
        Board light0 = Boards[1];
        Board light1 = Boards[3];
        Rock rock;
        
       /* for(int i=0; i<4; i++)
        {
            rock = light0.transform.GetChild(0).GetChild(i).GetComponentInChildren<Rock>();
            rock.transform.parent = light0.transform.GetChild(2).GetChild(i).transform;
            rock.transform.localPosition = Vector3.zero; 

            rock = light1.transform.GetChild(0).GetChild(i).GetComponentInChildren<Rock>();
            rock.transform.parent = light1.transform.GetChild(2).GetChild(i).transform;
            rock.transform.localPosition = Vector3.zero; 
        }*/
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
        //public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask)
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
            DebugText.text += "Num valid spaces: " + HeldRock.MyBoard.ValidMoves.Count + "\n";
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
                        if(rock.MyBoard.UpdatePossiblePassiveMoves(rock, eMoveType.PASSIVE) == false)
                        {
                            Debug.Log("No valid passive moves");                            
                        }
                        else
                        {
                            HeldRock = rock;
                            HeldRock.transform.localScale *= 1.2f;
                            BoardSpace heldRockSpace = HeldRock.GetComponentInParent<BoardSpace>();
                            //if(hitRockBoard.BoardColor == Board.eBoardColor.DARK)
                            if(HeldRock.MyBoard.BoardColor == Board.eBoardColor.DARK)
                            {     
                                //Debug.Log("Check board: " + Boards[1].name + " and board: " + Boards[3].name);           
                                bool boardA = Boards[1].CheckIfAnyValidAggressiveMoves(HeldRock);
                                bool boardB = Boards[3].CheckIfAnyValidAggressiveMoves(HeldRock);
                              //  Debug.Log("boardA "+ Boards[1].name + ": " + boardA + ", boardB: " + Boards[3].name + ": " + boardB);
                            }
                            else
                            {
                              //  Debug.Log("Check board: " + Boards[0].name + " and board: " + Boards[2].name);           
                                bool boardA = Boards[0].CheckIfAnyValidAggressiveMoves(HeldRock);
                                bool boardB = Boards[2].CheckIfAnyValidAggressiveMoves(HeldRock);
                              //  Debug.Log("boardA "+ Boards[1].name + ": " + boardA + ", boardB: " + Boards[3].name + ": " + boardB);
                            } 
                            ValidRockMoves = (List<RockMove>)ValidRockMoves.Distinct().ToList();
                            List<RockMove> invalidRockMoves = PassiveMovesToCheck.Except(ValidRockMoves).ToList();
                            //ValidRockMoves = (List<RockMove>) (ValidRockMoves.Distinct());
                            //ValidRockMoves = (List<RockMove>)v.ToList();
                            Debug.Log("*******Num ValidRockMoves: " + ValidRockMoves.Count + " ****************** --CVA--");                            
                            foreach(RockMove rockMove in ValidRockMoves)
                            {
                                Debug.Log("Valid Rock Move: (" + rockMove.moveDir + ", " + rockMove.numSpaces + ") --CVA--");                                
                            }
                            Debug.Log("*******Num invalidRockMoves: " + invalidRockMoves.Count + " ****************** --CVA--");   
                            foreach(RockMove rockMove in invalidRockMoves)
                            {
                                Debug.Log("inValid Rock Move: (" + rockMove.moveDir + ", " + rockMove.numSpaces + ") --CVA--");               
                                Vector2Int move = Board.MoveDeltas[(int)rockMove.moveDir] * rockMove.numSpaces;
                                Vector2Int moveCoords = heldRockSpace.SpaceCoords + move;
                                BoardSpace bs = HeldRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y];
                                bs.ToggleHighlight(true, Color.red);
                                HeldRock.MyBoard.ValidMoves.Remove(bs);
                            }                         
                        }
                    }    
                    else
                    {
                        if(rock.MyBoard.CheckAggressiveMove(rock, PassiveMove, false))
                        {
                          //  Debug.Log("Can grab this piece");
                            HeldRock = rock;
                            HeldRock.transform.localScale *= 1.2f;
                        }
                        else
                        {                            
                            Debug.LogWarning("No valid Aggressive move");
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
