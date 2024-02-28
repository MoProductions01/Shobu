
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Radient
{
    /// <summary>
    /// This is the main class for the game.  It handles all of the game logic and
    /// player control
    /// </summary>
    public class Shobu : MonoBehaviour
    {           
        // Overall state of the game.
        public enum eGameState {PLAYING, GAME_OVER};
        eGameState GameState;

        // The colors of the boards
        public enum eBoardColor {DARK, LIGHT};
                    
        // The two colors of the rocks, or the color representing each player                    
        public enum eRockColors {BLACK, WHITE};
        eRockColors CurrentRockColor;
        
        // The two kinds of moves a player can make
        public enum eMoveType {PASSIVE, AGGRESSIVE};  
        eMoveType CurrentMove;

        // Each move has 3 different states.  
        // NONE_SELECTED - no rock has been chosen
        // ROCK_SELECTED - a valid rock has been selected and is waiting for a Board Space to move to
        // ROCK_MOVEMENT - rocks are moving and no player input is allowed
        public enum eMoveState{NONE_SELECTED, ROCK_SELECTED, ROCK_MOVEMENT};
        eMoveState MoveState;
        BoardSpace MoveToBoardSpace; // The BoardSpace that the player's rock is moving to       
        
        [SerializeField] List<Board> Boards = new List<Board>(); // The 4 boards for the game
        List<Board> ValidBoards = new List<Board>(); // Boards that are valid for play for current move
        Rock SelectedRock; // The Rock the player currently has selected           
        int RockMask, BoardSpaceMask, RockLayer; // Various Layer related values for raycasting and collisions    
        int NumRocksMoving = 0; // There can be up to two rocks moving depending on if a rock is being pushed or not                    

        // This is the callback for changes in state so the UI can register to it and update when necessary        
        public delegate void CallbackType(eGameState gameState, eRockColors player, eMoveType moveType);
        public event CallbackType OnGameStateChangeAction;                
        
        Vector2 BoardSpaceDist; // The distance between board spaces in x and y
        Vector3 RockScale;  // The starting scale for Rocks.  Used after rocks shrinking to simulate falling off board   
        
        public TMP_Text DebugText; // Text element for debugging on Display 2

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {          
            // Keep track of the distance between board spaces for when we want to push a rock off the board          
            BoardSpaceDist.x = Boards[0].BoardSpaces[1,0].transform.position.x - Boards[0].BoardSpaces[0,0].transform.position.x;
            BoardSpaceDist.y = Boards[0].BoardSpaces[0,1].transform.position.y - Boards[0].BoardSpaces[0,0].transform.position.y;
            // The rocks "falling" is simulated with a scale change so keep track of default scale for after they're done
            RockScale = Boards[0].GetComponentInChildren<Rock>().transform.localScale;
            
            // Hold various masks and layers for collisions
            RockMask = LayerMask.GetMask("Rock");
            BoardSpaceMask = LayerMask.GetMask("Board Space");
            RockLayer = LayerMask.NameToLayer("Rock");
            
            ResetGame(); // Reset the game to it's starting point
        }

#region GAME_STATE
        /// <summary>
        /// Restarts/resets the game to it's starting state
        /// </summary>
        public void ResetGame()
        {
            foreach(Board board in Boards) board.ResetBoard();  // All boards and rocks            
            RockMove.GetInstance().Reset(); // No current rock move active           
            UpdateValidBoards(0,1);
            ChangeGameState(eGameState.PLAYING, eRockColors.BLACK, eMoveType.PASSIVE);   
            MoveState = eMoveState.NONE_SELECTED;
            Physics.IgnoreLayerCollision(RockLayer, RockLayer, true);            
        }

        /// <summary>
        /// Restarts/resets the game to it's starting state
        /// </summary>        
        void UpdateValidBoards(int firstBoard, int secondBoard)
        {
            ValidBoards.Clear();
            ValidBoards.AddRange(new List<Board>{Boards[firstBoard], Boards[secondBoard]});            
        }

        /// <summary>
        /// Updates the game state and calls the action for any listeners
        /// </summary>
        /// <param name="newGameState"> New GameState. </param>
        /// <param name="newPlayer"> New Player/Rock Color. </param>
        /// <param name="newMoveType"> Passive or Agressive move</param>
        void ChangeGameState(eGameState newGameState, eRockColors newPlayer, eMoveType newMoveType)
        {
            // Update the game state vars
            GameState = newGameState; 
            CurrentRockColor = newPlayer;
            CurrentMove = newMoveType;   
            
            // Call the Game State Action function for any listeners out there         
            if(OnGameStateChangeAction != null)
            {
                OnGameStateChangeAction(GameState, CurrentRockColor, CurrentMove);
            }            
        }          
#endregion                     

#region ROCK_MOVEMENT
        /// <summary>
        /// Callback when a rock falling off the board has finished "falling".
        /// </summary>
        void PushedRockFallTweenDone()
        {            
            if(RockMove.GetInstance().PushedRock == null) {Debug.LogError("ERROR: null pushed rock"); return;}                   

            Rock pushedRock = RockMove.GetInstance().PushedRock;
            pushedRock.transform.localScale = RockScale;    // Reset the scale of the pushed rock after it's tweened to 0
            pushedRock.MyBoard.PutRockOnPushedList(pushedRock); // Remove rock from board
            RockDoneMoving();   // Let game know this rock is done moving to check for state changes
        }
        
        /// <summary>
        /// Callback when a rock being pushed has finished it's movement across the board
        /// </summary>
        void PushedRockMoveTweenDone()
        {                 
            if(RockMove.GetInstance().PushedRock == null) {Debug.LogError("Null pushed rock"); return;}
                         
            Rock pushedRock = RockMove.GetInstance().PushedRock;
            if(Board.AreCoordsOffBoard(pushedRock.PushedCoords) == false)
            {   // If the pushed rock is staying on the board then just re-parent it to the BoardSpace
                // and let the game know it's done moving.                                                               
                pushedRock.transform.parent = 
                    pushedRock.MyBoard.BoardSpaces[pushedRock.PushedCoords.x, pushedRock.PushedCoords.y].transform;                
                RockDoneMoving();
            }
            else
            {   // The rock is being pushed off the board, so get it's "falling" starting
                // The rock "falls" by tweening it's scale to zero.                
                LeanTween.scale(pushedRock.gameObject, Vector3.zero, .5f).
                        setEase(LeanTweenType.linear).setOnComplete(this.PushedRockFallTweenDone);            
            }            
        }    
        
        /// <summary>
        /// Callback when the player selected rock has finished moving
        /// </summary>
        void SelectedRockMoveTweenDone()
        {                       
            if(SelectedRock == null) { Debug.LogError("Null selected rock"); return; }            
            
            // Re-parent, re-positoin and resize the slected rock
            SelectedRock.transform.parent = MoveToBoardSpace.transform;
            SelectedRock.transform.localScale /= 1.2f;         
            SelectedRock.transform.localPosition = Vector3.zero;            
            RockDoneMoving(); // Let game know a rock has finished moving
        }        

        /// <summary>
        /// Lets the game know a rock has fininshed moving.  It could be either a player
        /// selected rock or a rock being pushed by another rock.
        /// </summary>
        void RockDoneMoving()
        {
            if(NumRocksMoving <= 0) { Debug.LogError("No moving rocks"); return; }
                        
            NumRocksMoving--; // Reduce number of rocks moving
            if(NumRocksMoving == 0)
            {   // No more rocks moving so handle that situation
                AllRockMovementDone();
            }
        }  

        /// <summary>
        /// All player and pushed rocks have finished their movement
        /// </summary>
        void AllRockMovementDone()
        {
            if(SelectedRock == null) { Debug.LogError("Null selected rock"); return; }
            
            // All rock movement done so reset the rock and move state
            SelectedRock.MyBoard.ResetSpaceHighlights();            
            SelectedRock = null;                        
            MoveState = eMoveState.NONE_SELECTED; 
            // Set the physics to ignore collisions until the next rock movement state
            Physics.IgnoreLayerCollision(RockLayer, RockLayer, true);  
            // End the move and let the game see what happens next
            EndMove(MoveToBoardSpace.GetComponentInParent<Board>());
        }
#endregion
       
#region USER_INPUT       
       /// <summary>
       /// Handles raycasting depending on the layer mask wanted
       /// </summary>
       /// <param name="layerMask">LayerMask to use for collisions with the ray cast</param>
       /// <returns></returns>
        RaycastHit RayCast(int layerMask)
        {
            // Cast a ray from the camera and return the hit
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;       
            Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);        
            return hit;
        }    

        /// <summary>
        /// Handles the input for when the user is choosing a rock
        /// </summary>
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
                        }
                    }                                 
                }                        
            }
        }

        public void CheckPushedRockCollision(Rock otherRock)
        {
            if(otherRock == RockMove.GetInstance().PushedRock)
            {
               // Debug.Log("HIT PUSHED ROCK!!!!");
                Physics.IgnoreLayerCollision(RockLayer, RockLayer, true); 
                Vector3 moveToPosition = Vector3.zero;
                moveToPosition.x = otherRock.MyBoard.BoardSpaces[0,0].transform.position.x + 
                                    BoardSpaceDist.x * otherRock.PushedCoords.x;
                moveToPosition.y = otherRock.MyBoard.BoardSpaces[0,0].transform.position.y + 
                                    BoardSpaceDist.y * otherRock.PushedCoords.y;
                otherRock.transform.parent = otherRock.transform.parent.parent;
                LeanTween.move(otherRock.gameObject, moveToPosition, .2f).
                        setEase(LeanTweenType.linear).setOnComplete(this.PushedRockMoveTweenDone);                              
            }
        }

        void UndoSelectedRock()
        {
            SelectedRock.MyBoard.ResetSpaceHighlights();         
            SelectedRock.transform.localScale /= 1.2f;
            SelectedRock = null; 
            MoveState = eMoveState.NONE_SELECTED; 
        }
        
        void HandleSelectMoveSpace()
        {
            RaycastHit hit = RayCast(BoardSpaceMask);
            if(hit.collider != null)
            {                    
                BoardSpace hitBoardSpace = hit.collider.GetComponent<BoardSpace>();
                Board hitBoardSpaceBoard = hitBoardSpace.GetComponentInParent<Board>();                 
                if(SelectedRock.MyBoard == hitBoardSpaceBoard)
                {         
                    NumRocksMoving = 1;               
                    if(RockMove.GetInstance().ValidBoardSpaces.Contains(hitBoardSpace))
                    {                     
                        if(CurrentMove == eMoveType.PASSIVE)
                        {
                            RockMove.GetInstance().PassiveMove = hitBoardSpace.SpaceCoords - SelectedRock.GetComponentInParent<BoardSpace>().SpaceCoords;
                        }                                                                                    
                        else
                        {                            
                            if(RockMove.GetInstance().PushedRock != null)
                            {
                                Physics.IgnoreLayerCollision(RockLayer, RockLayer, false);  
                                NumRocksMoving++;                              
                            }                            
                        }                        
                        MoveToBoardSpace = hitBoardSpace;                          
                        SelectedRock.transform.parent = MoveToBoardSpace.transform.parent;
                        LeanTween.move(SelectedRock.gameObject, MoveToBoardSpace.transform.position, .3f).
                                setEase(LeanTweenType.easeInCubic).setOnComplete(this.SelectedRockMoveTweenDone);                        
                        MoveState = eMoveState.ROCK_MOVEMENT;                                                                                                                
                    }
                    else
                    {                        
                        Debug.LogWarning("Valid Board and Rock but Invalid Move");
                       // SelectedRock.MyBoard.ResetSpaceHighlights();                            
                        UndoSelectedRock();    
                    }                  
                }
                else
                {
                    Debug.Log("Invalid Board");    
                    UndoSelectedRock();
                }               
            }            
            else
            {
                Debug.Log("Released over a non BoardSpace");  
                UndoSelectedRock();
            }              
        }
#endregion
        
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
                    HandleSelectMoveSpace();   
                }
            }           
        }
        
       
        void EndMove(Board moveBoard)
        {                                                                                                                    
            if(CurrentMove == eMoveType.PASSIVE)
            {                
                ChangeGameState(GameState, CurrentRockColor, eMoveType.AGGRESSIVE);                 
                if(moveBoard.BoardColor == eBoardColor.DARK)
                {   
                    UpdateValidBoards(1,3);                                 
                }
                else
                {
                    UpdateValidBoards(0,2);                    
                }                    
            }
            else
            {
                if(moveBoard.CheckEndGame() == true)
                {                    
                    ChangeGameState(eGameState.GAME_OVER, CurrentRockColor, CurrentMove);  
                }
                else
                {
                    RockMove.GetInstance().PassiveMove = Vector2Int.zero;                  
                    ChangeGameState(GameState, (eRockColors)( 1 - (int)CurrentRockColor), eMoveType.PASSIVE);  
                    if(CurrentRockColor == eRockColors.BLACK)
                    {                                        
                        UpdateValidBoards(0,1);
                    }
                    else
                    {                        
                        UpdateValidBoards(2,3);
                    }
                } 
            }                
        }
        void PrintDebugInfo()
        {
            DebugText.text = "Current Player: " + CurrentRockColor.ToString() + "\n";
            DebugText.text += "Current Move: " + CurrentMove.ToString() + "\n";
            DebugText.text += "PassiveMove: " + RockMove.GetInstance().PassiveMove.ToString() + "\n";
            DebugText.text += "MoveState: " + MoveState.ToString() + "\n";      
            DebugText.text += "NumRocksMoving: " + NumRocksMoving.ToString() + "\n";
            //if(MoveToPosition != Vector3.zero) DebugText.text += "MoveToPosition: " + MoveToPosition.ToString("F2") + "\n";      
            if(SelectedRock == null) DebugText.text += "No SelectedRock\n";
            else
            {
                DebugText.text += "SelectedRock: " + SelectedRock.name;
                if( SelectedRock.GetComponentInParent<BoardSpace>() != null )
                {
                    DebugText.text += ", selected at: " + SelectedRock.GetComponentInParent<BoardSpace>().SpaceCoords.ToString() + "\n";                     
                }
                else
                {
                    DebugText.text += ", is moving\n";
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
