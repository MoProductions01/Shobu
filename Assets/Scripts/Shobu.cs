
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Radient
{
    public class Shobu : MonoBehaviour
    {           
        public enum eGameState {PLAYING, GAME_OVER};
        eGameState GameState;
                    
        public enum eRockColors {BLACK, WHITE};
        eRockColors CurrentRockColor;
        
        public enum eMoveType {PASSIVE, AGGRESSIVE};  
        eMoveType CurrentMove;

        public enum eMoveState{NONE_SELECTED, ROCK_SELECTED, ROCK_MOVEMENT};
        eMoveState MoveState;
        Vector3 MoveToPosition = Vector3.zero;

        //public eRockColors WinningColor {get; private set;}
        [SerializeField] List<Board> Boards = new List<Board>();    
        int RockMask, BoardSpaceMask;
        List<Board> ValidBoards = new List<Board>();    
        Rock SelectedRock;    
        
        public TMP_Text DebugText;            

        //[SerializeField] UnityEvent OnGameStateChange;
        public delegate void CallbackType(eGameState gameState, eRockColors player, eMoveType moveType);
        public event CallbackType OnGameStateChangeAction;
        // event that prevents you from doing anything but  C# mechanim called Event.
        // allows public expose subscribing to an event and de-subscribing yourself
        // but can't remove anyone else's listeners

         void ChangeGameState(eGameState gameState, eRockColors player, eMoveType move)
        {
            GameState = gameState; 
            CurrentRockColor = player;
            CurrentMove = move;
            //OnGameStateChange.Invoke();
            if(OnGameStateChangeAction != null)
            {
                OnGameStateChangeAction(GameState, CurrentRockColor, CurrentMove);
            }            
        }          

        
        void Start()
        {        
            ResetGame();
            RockMask = LayerMask.GetMask("Rock");
            BoardSpaceMask = LayerMask.GetMask("Board Space");
        }

        public void ResetGame()
        {
            ResetBoards();
            RockMove.GetInstance().Reset();                    
            ValidBoards.Add(Boards[0]);
            ValidBoards.Add(Boards[1]);     
            ChangeGameState(eGameState.PLAYING, eRockColors.BLACK, eMoveType.PASSIVE);   
            MoveState = eMoveState.NONE_SELECTED;
            
            // Debug       
        // SetupRockDebug(Boards[0], new Vector2Int(0,0), new Vector2Int(0,2));        
        }

        public void ResetBoards()
        {
            foreach(Board board in Boards)
            {
                board.ResetBoard();
            }
        }

        void ResetSelectedRock()
        {
            Debug.Log("ResetSelectedRock()");
            if(SelectedRock != null)
            {
                SelectedRock.transform.localScale /= 1.2f;         
                SelectedRock.transform.localPosition = Vector3.zero;
                SelectedRock.MyBoard.ResetSpaceHighlights();            
                SelectedRock = null; 
            }            
            MoveState = eMoveState.NONE_SELECTED;  
        }
       
        RaycastHit RayCast(int layerMask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;       
            Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);        
            return hit;
        }    

        void HandleSelectRock()
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
                    RockMove.GetInstance().Reset();
                    if(CurrentMove == eMoveType.PASSIVE)
                    {   // PASSIVE move
                        if(rock.MyBoard.UpdatePossiblePassiveMoves(rock))
                        {
                            BoardSpace clickedRockSpace = rock.GetComponentInParent<BoardSpace>();
                            List<Board> boardsToCheck = new List<Board>(Boards);
                            boardsToCheck.RemoveAll(x => x.BoardColor == rock.MyBoard.BoardColor);
                            foreach(Board board in boardsToCheck)
                            {
                                board.CheckIfAnyValidAggressiveMoves(rock);
                            }                                  
                            foreach(Vector2Int passiveMove in RockMove.GetInstance().ValidPassiveMoves)
                            {                                
                                Vector2Int moveCoords = clickedRockSpace.SpaceCoords + passiveMove;
                                if(moveCoords.x > 3 || moveCoords.x < 0 || moveCoords.y > 3 || moveCoords.y < 0)
                                {
                                    Debug.LogError("WTF: " + moveCoords.ToString());
                                }
                                rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y].ToggleHighlight(true, Color.blue);                                   
                                RockMove.GetInstance().ValidBoardSpaces.Add(rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);                                    
                            }                                                          
                            
                            foreach(Vector2Int passiveMove in RockMove.GetInstance().GetInvalidPassiveMoves())
                            {                                
                                Vector2Int moveCoords = clickedRockSpace.SpaceCoords + passiveMove;
                                BoardSpace bs = rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y];
                                bs.ToggleHighlight(true, Color.red);                                
                            }  
                            if(RockMove.GetInstance().ValidPassiveMoves.Count != 0)
                            {
                                SelectedRock = rock;
                                SelectedRock.transform.localScale *= 1.2f;
                                MoveState = eMoveState.ROCK_SELECTED;
                            }
                            else
                            {
                                Debug.LogWarning("No valid aggressive moves");                                
                            }
                        }
                        else
                        {                                                        
                            Debug.LogWarning("No possible passive moves");           
                        }
                    }    
                    else
                    {   // AGGRESSIVE move                    
                        Vector2Int moveCoords = rock.GetComponentInParent<BoardSpace>().SpaceCoords + RockMove.GetInstance().PassiveMove;
                        if(rock.MyBoard.CheckAggressiveMove(rock, RockMove.GetInstance().PassiveMove, false))
                        {                          
                            SelectedRock = rock;
                            SelectedRock.transform.localScale *= 1.2f;                                                                                    
                            // monote - make BoardSpaces private and use an accessor                                                                
                            RockMove.GetInstance().ValidBoardSpaces.Add(SelectedRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);
                            SelectedRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y].ToggleHighlight(true, Color.blue);                               
                            MoveState = eMoveState.ROCK_SELECTED;    
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

        void HandleSelectBoardSpace()
        {
            RaycastHit hit = RayCast(BoardSpaceMask);
            if(hit.collider != null)
            {                    
                BoardSpace hitBoardSpace = hit.collider.GetComponent<BoardSpace>();
                Board hitBoardSpaceBoard = hitBoardSpace.GetComponentInParent<Board>();                 
                if(SelectedRock.MyBoard == hitBoardSpaceBoard)
                {                        
                    if(RockMove.GetInstance().ValidBoardSpaces.Contains(hitBoardSpace))
                    {                     
                        if(CurrentMove == eMoveType.PASSIVE)
                        {
                            RockMove.GetInstance().PassiveMove = hitBoardSpace.SpaceCoords - SelectedRock.GetComponentInParent<BoardSpace>().SpaceCoords;
                        }                                                                                    
                        else
                        {
                            SelectedRock.MyBoard.CheckPushedRock();
                        }
                        //SelectedRock.transform.parent = hitBoardSpace.transform;                        
                        //EndMove(hitBoardSpaceBoard);    
                        MoveToPosition = hitBoardSpace.transform.position;
                        SelectedRock.transform.parent = hitBoardSpace.transform.parent;
                        MoveState = eMoveState.ROCK_MOVEMENT;                                                                                                                
                    }
                    else
                    {                        
                        Debug.LogWarning("Valid Board and Rock but Invalid Move");
                        SelectedRock.MyBoard.ResetSpaceHighlights();                            
                        ResetSelectedRock(); 
                    }                  
                }
                else
                {
                    Debug.Log("Invalid Board");    
                    ResetSelectedRock();                         
                }               
            }            
            else
            {
                Debug.Log("Released over a non BoardSpace");  
                ResetSelectedRock();              
            }   
           // ResetSelectedRock(); 
        }
        
        void Update()
        {
            if(GameState == eGameState.GAME_OVER) return;                
            PrintDebugInfo();
            if(MoveState == eMoveState.ROCK_MOVEMENT) return;

            if(Input.GetMouseButtonDown(0))
            {                   
                if(MoveState == eMoveState.NONE_SELECTED)
                {
                    HandleSelectRock();                        
                }                    
                else if(MoveState == eMoveState.ROCK_SELECTED)
                {
                    HandleSelectBoardSpace();   
                }
            }
            /*else if(Input.GetMouseButton(0) && SelectedRock != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                SelectedRock.transform.position = new Vector3(mouseWorld.x, mouseWorld.y, 0f);
            }*/
            /*else if(Input.GetMouseButtonUp(0) && SelectedRock )
            {
                HandleSelectBoardSpace();          
            }*/
            else if(Input.GetMouseButtonUp(0))
            {
                //Debug.Log("MousebuttonUp");
                foreach(Board board in Boards)
                {
                   // board.ResetSpaceHighlights();
                }
            }
        }
        
        void EndMove(Board moveBoard)
        {
            ValidBoards.Clear();
                        
            eGameState newGameState = GameState;
            eRockColors newRockColor = CurrentRockColor; 
            eMoveType newMoveType = CurrentMove;                                   
            if(CurrentMove == eMoveType.PASSIVE)
            {
                newMoveType = eMoveType.AGGRESSIVE;                   
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
                    newGameState = eGameState.GAME_OVER;                    
                }
                else
                {
                    RockMove.GetInstance().PassiveMove = Vector2Int.zero;
                    newMoveType = eMoveType.PASSIVE;
                    newRockColor = (eRockColors)( 1 - (int)CurrentRockColor);
                    if(newRockColor == eRockColors.BLACK)
                    {                
                        ValidBoards.AddRange(new List<Board>{Boards[0], Boards[1]});
                    }
                    else
                    {
                        ValidBoards.AddRange(new List<Board>{Boards[2], Boards[3]});
                    }
                } 
            }    
            ChangeGameState(newGameState, newRockColor, newMoveType);    
        }

        void PrintDebugInfo()
        {
            DebugText.text = "Current Player: " + CurrentRockColor.ToString() + "\n";
            DebugText.text += "Current Move: " + CurrentMove.ToString() + "\n";
            DebugText.text += "PassiveMove: " + RockMove.GetInstance().PassiveMove.ToString() + "\n";
            DebugText.text += "MoveState: " + MoveState.ToString() + "\n";      
            //if(MoveToPosition != Vector3.zero) DebugText.text += "MoveToPosition: " + MoveToPosition.ToString("F2") + "\n";      
            if(SelectedRock == null) DebugText.text += "No SelectedRock\n";
            else //if (SelectedRock.GetComponentInParent<BoardSpace>() != null)
            {
                DebugText.text += "SelectedRock: " + SelectedRock.name;// + ", selected at: " + SelectedRock.GetComponentInParent<BoardSpace>().SpaceCoords.ToString() + "\n";                     
                if( SelectedRock.GetComponentInParent<BoardSpace>() != null )
                {
                    DebugText.text += ", selected at: " + SelectedRock.GetComponentInParent<BoardSpace>().SpaceCoords.ToString() + "\n";                     
                }
                else
                {
                    DebugText.text += ", is moving to: " + MoveToPosition.ToString("F2") + "\n";
                }
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
}
