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

        [SerializeField] private GameObject circleVFX;
        [SerializeField] private GameObject triangleVFX;
        [SerializeField] private GameObject shineVFX;

        //AUDIO
        public AudioSource audioSource1;//HighlightRock
        public AudioSource audioSource2;//MoveRock
        public AudioSource audioSource3;//PushAnotherRock
        public AudioSource audioSource4;//PushedOffBoard
        public AudioSource audioSource5;//Win

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

            //AUDIO
            if (audioSource4 != null && audioSource4.clip != null)
            {
                audioSource4.Play();
            }
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
                pushedRock.transform.parent = pushedRock.MyBoard.BoardSpaces[pushedRock.PushedCoords.x, pushedRock.PushedCoords.y].transform;
                shineVFX.transform.localPosition = pushedRock.GetComponentInParent<Transform>().position;
                shineVFX.GetComponent<Animator>().SetTrigger("Play");
                shineVFX.SetActive(true);
                //AUDIO
                if (audioSource3 != null && audioSource3.clip != null)
        {
            audioSource3.Play();
        }

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
        /// I turned rock to rock collision (trigger) on when the SelectedRock gets going, so if there's
        /// a collision check if it's the rock that will be pushed.  If so, handle it.
        /// </summary>
        /// <param name="otherRock">Other rock that collided with the Selected Rock</param>
        public void CheckPushedRockCollision(Rock otherRock)
        {
            if(otherRock == RockMove.GetInstance().PushedRock)
            {               
                Physics.IgnoreLayerCollision(RockLayer, RockLayer, true); // Only want one collision so re-ignore in the physics
                // It's possible that the pushed rock will go off the board so calculate the push location
                // instead of using the space it will end up at's location
                Vector3 moveToPosition = Vector3.zero;
                moveToPosition.x = otherRock.MyBoard.BoardSpaces[0,0].transform.position.x + 
                                    BoardSpaceDist.x * otherRock.PushedCoords.x;
                moveToPosition.y = otherRock.MyBoard.BoardSpaces[0,0].transform.position.y + 
                                    BoardSpaceDist.y * otherRock.PushedCoords.y;
                // Get the movement going
                otherRock.transform.parent = otherRock.transform.parent.parent;
                LeanTween.move(otherRock.gameObject, moveToPosition, .2f).
                        setEase(LeanTweenType.linear).setOnComplete(this.PushedRockMoveTweenDone);                              
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
            triangleVFX.SetActive(false);
            circleVFX.SetActive(false);
            SelectedRock.transform.localPosition = Vector3.zero;
            shineVFX.transform.position = SelectedRock.GetComponentInParent<Transform>().position;
            shineVFX.GetComponent<Animator>().SetTrigger("Play");
            shineVFX.SetActive(true);
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

        /// <summary>
        /// Called when the selected move is done.
        /// </summary>
        /// <param name="moveBoard">The board a move was on. </param>
        void EndMove(Board moveBoard)
        {                                                                                                                    
            if(CurrentMove == eMoveType.PASSIVE)
            {   // Switch to Aggressive move and determine valid boards based on the
                // color of the board the Passive move was made on.               
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
                {   // If a user has won then change the state to that.                    
                    ChangeGameState(eGameState.GAME_OVER, CurrentRockColor, CurrentMove); 
                    
                    if (audioSource5 != null && audioSource5.clip != null)
        {
            audioSource5.Play();
        }
                }
                else
                {   // Switch to Passive mode, resetting the RockMove and determining
                    // valid boards based on the rock color whose turn it now is.
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
        /// The user has clicked on a valid rock for a passive move so check to 
        /// see what should happen.
        /// </summary>
        /// <param name="rock">Rock the user has selected</param>
        void HandlePassiveMoveChecks(Rock rock)
        {
            // Check the rock's board for all possible passive moves
            if(rock.MyBoard.UpdatePossiblePassiveMoves(rock))
            {   // We've got at least one valid passive move                
                // Get a list of boards that are the other color to the
                // one the selected rock is on.
                List<Board> boardsToCheck = new List<Board>(Boards);
                boardsToCheck.RemoveAll(x => x.BoardColor == rock.MyBoard.BoardColor);
                // Get the space on the board the rock is on
                BoardSpace clickedRockSpace = rock.GetComponentInParent<BoardSpace>();
                // Check to see how many valid Aggressive moves are available on the boards
                foreach(Board board in boardsToCheck)
                {   // This fills in the relevant files in the RockMove class
                    board.CheckValidAggressiveMoves(rock);
                }  
                // Loop through any valid Passive moves generated by the Aggressive move check                                
                foreach(Vector2Int passiveMove in RockMove.GetInstance().ValidPassiveMoves)
                {   // For each valid Passive move on both valid boards highlight it and add
                    // it to the global ValidBoardSpaces for checking in the next step
                    Vector2Int moveCoords = clickedRockSpace.SpaceCoords + passiveMove;                    
                    rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y].ToggleHighlight(true, "passive");                                   
                    RockMove.GetInstance().ValidBoardSpaces.Add(rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);                                    
                }
                // Check if we have any valid passive moves   
                if(RockMove.GetInstance().ValidPassiveMoves.Count != 0)
                {   // We've got at least one valid passive move
                    SelectedRock = rock; // Update the Selected rock
                    SelectedRock.transform.localScale *= 1.2f;  // Make selected rock a little bigger
                    if (CurrentRockColor == eRockColors.BLACK)
                    {
                        triangleVFX.SetActive(true);
                        triangleVFX.transform.localPosition = SelectedRock.transform.position;

                    }
                    else if (CurrentRockColor == eRockColors.WHITE)
                    {
                        circleVFX.SetActive(true);
                        circleVFX.transform.localPosition = SelectedRock.transform.position;
                    }

                    MoveState = eMoveState.ROCK_SELECTED; // Change the move state since we have a valid space we can move to
                }                                                       
                // Loop through any invalid moves
                foreach(Vector2Int passiveMove in RockMove.GetInstance().GetInvalidPassiveMoves())
                {   // Turn on the invalid highlight for each invalid move
                    Vector2Int moveCoords = clickedRockSpace.SpaceCoords + passiveMove;
                    BoardSpace boardSpace = rock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y];
                    boardSpace.ToggleHighlight(true, "agressive");                                
                }                                  
            }  
        }

        /// <summary>
        /// The user has clicked on a valid rock for a aggressive move so check to 
        /// see what should happen.
        /// </summary>
        /// <param name="rock">Rock the user has selected</param>
        void HandleAggressiveMoveChecks(Rock rock)
        {
            // Check to see if there's any valid aggressive moves for the chosen rock
            Vector2Int moveCoords = rock.GetComponentInParent<BoardSpace>().SpaceCoords + RockMove.GetInstance().PassiveMove;
            if(rock.MyBoard.CheckAggressiveMove(rock, RockMove.GetInstance().PassiveMove, false))
            {   // There's at least one valid aggressive move                   
                SelectedRock = rock; // Update user selected rock
                SelectedRock.transform.localScale *= 1.2f; // Make rock a little bigger
                if(CurrentRockColor == eRockColors.BLACK)
                {
                    triangleVFX.SetActive(true);
                    triangleVFX.transform.localPosition = SelectedRock.transform.position;

                }
                else if(CurrentRockColor == eRockColors.WHITE)
                {
                    circleVFX.SetActive(true);
                    circleVFX.transform.localPosition = SelectedRock.transform.position;
                }

                RockMove.GetInstance().ValidBoardSpaces.Add(SelectedRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y]);
                SelectedRock.MyBoard.BoardSpaces[moveCoords.x, moveCoords.y].ToggleHighlight(true, "agressive");                               
                MoveState = eMoveState.ROCK_SELECTED; 
                
                
            }            
        }

        /// <summary>
        /// Handles the input for when the user is choosing a rock
        /// </summary>
        void HandleSelectRock()
        {
            RaycastHit hit = RayCast(RockMask);
            if(hit.collider == null) return; // bail if you didn't click on a rock


            Rock rock = hit.collider.GetComponent<Rock>();  // get the rock component
            // Leave if the selected rock isn't on a valid board or it's the wrong color
            if(ValidBoards.Contains(rock.MyBoard) == false || (CurrentRockColor != rock.RockColor)) return;                                           
            
            // We've got a valid rock so reset the RockMove info
            RockMove.GetInstance().Reset();  
            if(CurrentMove == eMoveType.PASSIVE)
            {   // PASSIVE move
                HandlePassiveMoveChecks(rock);   
                
                //AUDIO
            if (audioSource1 != null && audioSource1.clip != null)
        {
            audioSource1.Play();
        }
            }    
            else
            {   // AGGRESSIVE move
                HandleAggressiveMoveChecks(rock);   
                
                //AUDIO
            if (audioSource1 != null && audioSource1.clip != null)
        {
            audioSource1.Play();
        }
            }                                                                               
        }

        /// <summary>
        /// Handles selecting a BoardSpace for the selected rock to move to
        /// </summary>
        void HandleSelectMoveSpace()
        {
            RaycastHit hit = RayCast(BoardSpaceMask);
            if(hit.collider == null) return;
                             
            BoardSpace hitBoardSpace = hit.collider.GetComponent<BoardSpace>();
            Board hitBoardSpaceBoard = hitBoardSpace.GetComponentInParent<Board>();   
            if(SelectedRock.MyBoard != hitBoardSpaceBoard) {  UndoSelectedRock(); return; } // invalid board           
            if(RockMove.GetInstance().ValidBoardSpaces.Contains(hitBoardSpace) == false) {UndoSelectedRock();  return; } // valid board but invalid move                                

             //AUDIO
                if (audioSource2 != null && audioSource2.clip != null)
        {
            audioSource2.Play();
        }

            // We have a valid move so there's at least 1 rock that's going to be moving to keep track of   
            NumRocksMoving = 1;             
            if(CurrentMove == eMoveType.PASSIVE)
            {   // Passive move, so keep track of what the move was for the Aggressive move part
                RockMove.GetInstance().PassiveMove = hitBoardSpace.SpaceCoords - SelectedRock.GetComponentInParent<BoardSpace>().SpaceCoords;
               
                //AUDIO
                if (audioSource2 != null && audioSource2.clip != null)
        {
            audioSource2.Play();
        }
            }                                                                                    
            else
            {   // Aggressive move, so see if any rocks will be pushed by this move                         
                if(RockMove.GetInstance().PushedRock != null)
                {   // We've got a rock that will get pushed so turn the collision back on
                    Physics.IgnoreLayerCollision(RockLayer, RockLayer, false);  
                    NumRocksMoving++; // Increase num rocks moving so that the game won't move on until both are done

                   
                }                            
            }             
            // Set up the tween for the selected rock to move           
            MoveToBoardSpace = hitBoardSpace;                          
            SelectedRock.transform.parent = MoveToBoardSpace.transform.parent;
            LeanTween.move(SelectedRock.gameObject, MoveToBoardSpace.transform.position, .3f).
                    setEase(LeanTweenType.easeInCubic).setOnComplete(this.SelectedRockMoveTweenDone);                        
            MoveState = eMoveState.ROCK_MOVEMENT;                                                                                                                                                                                                       
        }

        /// <summary>
        /// Handles when the user had a selected rock but chose an invalid move
        /// </summary>
        void UndoSelectedRock()
        {   
            SelectedRock.MyBoard.ResetSpaceHighlights(); // Reset all the highlights         
            SelectedRock.transform.localScale /= 1.2f;  // Reset scale
            triangleVFX.SetActive(false);
            circleVFX.SetActive(false);
            SelectedRock = null; // No more selected rock
            MoveState = eMoveState.NONE_SELECTED; 
        }

        /// <summary>
        /// Called by the engine once per frame
        /// </summary>                  
        void Update()
        {
#if UNITY_EDITOR            
            PrintDebugInfo(); // Prints whatever debug info you want to the 2nd display
#endif
             // No game input if it's game over or the rocks are moving
            if(GameState == eGameState.GAME_OVER) return;                     
            if(MoveState == eMoveState.ROCK_MOVEMENT) return;            

            if(Input.GetMouseButtonDown(0))
            {   // If the user clicks it's either for a rock or a board space to move to                  
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
#endregion        
                              
#if UNITY_EDITOR   
        /// <summary>
        /// Prints whatever debug info you want to Display 2 so it doesn't cover up anything
        /// on the main game screen
        /// </summary>      
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
#endif
    }
}
