using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Radient
{
    /// <summary>
    /// Class for each of the 4 boards in the game.  Does stuff like managing board spaces, rocks
    /// on the board and calculcating possible Passive and Aggressive moves.
    /// </summary>
    public class Board : MonoBehaviour
    {
        public static int NUM_ROWS_COLS = 4; // It's a 4x4 grid on each board      
        // The 8 directions a rock piece can move        
        public enum eMoveDirs {UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT, UP_RIGHT};
        // The delta x,y for each of the 8 different move types
        public static List<Vector2Int> MoveDeltas = new List<Vector2Int> 
        {
            new Vector2Int(0, 1),   // UP
            new Vector2Int(-1, 1),  // UP_LEFT
            new Vector2Int(-1, 0),  // LEFT
            new Vector2Int(-1, -1), // DOWN_LEFT
            new Vector2Int(0, -1),  // DOWN
            new Vector2Int(1, -1),  // DOWN_RIGHT
            new Vector2Int(1, 0),   // RIGHT
            new Vector2Int(1, 1),   // UP_RIGHT
        };  

        private Shobu Shobu; // Reference to the Shobo game manager        
        [field: SerializeField] public Shobu.eBoardColor BoardColor {get; private set;} // this Board's color
                
        // The 16 BoardSpaces that are on each board        
        public BoardSpace[,] BoardSpaces {get; private set; } = new BoardSpace[4,4];
        [SerializeField] GameObject PushedOffRocks; // List of rocks that were pushed off the board     

        // Start is called before the first frame update
        void Start()
        {
            Shobu = FindObjectOfType<Shobu>();          
        } 
        
        private void Awake() 
        {                            
            // Create a 2D array of BoardSpaces based on their GameObject name.
            List<BoardSpace> boardSpaces = GetComponentsInChildren<BoardSpace>().ToList();
            foreach(BoardSpace boardSpace in boardSpaces)
            {
                // assign the board space based on x,y of the name
                string[] locString = boardSpace.name.Split(",");
                Vector2Int spaceLoc = new Vector2Int(Int32.Parse(locString[0]), Int32.Parse(locString[1]));                            
                BoardSpaces[spaceLoc.x, spaceLoc.y] = boardSpace;
            }                    
        }  
                
#region AGGRESSIVE_MOVES        
        /// <summary>
        /// Checks to see if there's any valid Aggressive moves for the rock
        /// based on the current list of Passive moves
        /// </summary>
        /// <param name="selectedRock">Rock selected by the user</param>
        /// <returns></returns>
        public void CheckValidAggressiveMoves(Rock selectedRock)
        {          
            // Get a list of rocks of the opposite color in case they can be pushed
            List<Rock> rocksToCheck = GetComponentsInChildren<Rock>().ToList();            
            rocksToCheck.RemoveAll(x => x.RockColor != selectedRock.RockColor);                    
            List<Rock> validRocks = new List<Rock>(); // keep a list of rocks with a valid Aggressive move
            // Go through each possible passive move and make sure there's at least ove
            // valid Aggressive move for it           
            foreach(Vector2Int passiveMove in RockMove.GetInstance().PossiblePassiveMoves)
            {
                validRocks.Clear();            
                foreach(Rock rock in rocksToCheck)
                {   // If the rock has a valid aggressive move then add it to the list
                    if(CheckAggressiveMove(rock, passiveMove, true) == true)
                    {
                        validRocks.Add(rock);      
                        break;             
                    }
                }            
                if(validRocks.Count > 0)
                {   // If the passive move has at least 1 valid rock for it, add it to the list                                 
                    RockMove.GetInstance().AddValidPassiveMove(passiveMove);
                }
            }                    
        }

        /// <summary>
        /// Checks the validity of the current Aggressive move
        /// </summary>
        /// <param name="rock">The rock we want to move</param>
        /// <param name="move">The x,y for the move itself</param>
        /// <param name="isValidAggressiveMoveTest">If it's just a test to see if it's valid then don't
        /// bother keeping track of any rocks that will get pushed.</param>
        /// <returns></returns>
        public bool CheckAggressiveMove(Rock rock, Vector2Int move, bool isValidAggressiveMoveTest)
        {
            BoardSpace rockBoardSpace = rock.GetComponentInParent<BoardSpace>();
            Vector2Int moveToSpace = rockBoardSpace.SpaceCoords + move;              
            
            if(AreCoordsOffBoard(moveToSpace)) return false; // The move is off the board so bail            
            
            Vector2Int moveDir = moveToSpace - rockBoardSpace.SpaceCoords; // Get the direction of the move            
            // Start with moving one space but increase it to two if any of the directions are two spaces
            int numSpacesMoved = 1; 
            if(Math.Abs(moveDir.x) == 2 || Math.Abs(moveDir.y) == 2) 
            {
                numSpacesMoved = 2;
                moveDir /= 2;
            }            
            Rock pushedRock = null; // A rock that could be pushed by this move            
            Vector2Int coordsToCheck = Vector2Int.zero; // Coordinates on the board to check
            int i;
            // Check the spaces in the direction for this move
            for(i=1; i<=numSpacesMoved; i++)
            {
                // Get any possible rock on the space
                coordsToCheck = rockBoardSpace.SpaceCoords + moveDir*i;
                Rock rockCheck = BoardSpaces[coordsToCheck.x, coordsToCheck.y].GetComponentInChildren<Rock>();                
                if(rockCheck == null) continue; // no rock so just move on                
                if(pushedRock != null) return false; // There's a rock on the space but you're already pushing a rock so return;                
                if(rockCheck.RockColor == rock.RockColor) return false; // can't push a rock of your own color
                // There's a rock but it's the first one and not of the same rock color so keep track of the rock you're going to push
                pushedRock = rockCheck;         
            } 
                                               
            // If we've got a pushed rock, check the space that it will be pushed to and see if it's still valid
            if(pushedRock != null) 
            {            
                coordsToCheck = rockBoardSpace.SpaceCoords + moveDir*i; // Get the board             
                if(AreCoordsOffBoard(coordsToCheck) || 
                        BoardSpaces[coordsToCheck.x, coordsToCheck.y].GetComponentInChildren<Rock>() == null)
                {
                    // If the rock will be pushed to an empty space or off the board then it's valid         
                    pushedRock.PushedCoords = coordsToCheck;
                }
                else
                {
                    // Rock is being pushed onto a space with a rock already there, which you can't do so return false
                    return false;
                }              
            }               

            // If we've made it this far, now check to see if it was an actual move or just checking to see if it's
            // valid.  If it's just a check then don't bother updating the PushedRock info
            if(isValidAggressiveMoveTest == false)
            {               
                RockMove.GetInstance().PushedRock = pushedRock;                
            }           
            
            return true; 
        }     
#endregion

#region PASSIVE_MOVES
        /// <summary>
        /// Checks if the board space coordinates are available.  Not availbe
        /// if 1) Off the board or 2) Has a rock on it.  Only for Passive moves
        /// since Aggressive moves can push rocks
        /// </summary>
        /// <param name="spaceCoords"></param>
        /// <returns></returns>
        public bool CheckPassiveBoardSpace(Vector2Int spaceCoords)
        {        
            // Check if it's off the board or if there's a rock on it.
            if(AreCoordsOffBoard(spaceCoords) ||
                                BoardSpaces[spaceCoords.x,spaceCoords.y].GetComponentInChildren<Rock>() != null)
            {
                // off board or space has a rock
                return false;
            }       
            return true;      
        }  

        /// <summary>
        /// Since we want to show the user all the possible passive moves, this will 
        /// check the surrounding spaces and update a list in the RockMove class
        /// </summary>
        /// <param name="rock">Rock to check</param>
        /// <returns>True if there's at least one passive move, false otherwise</returns>
        public bool UpdatePossiblePassiveMoves(Rock rock)
        {               
            // Check all 8 possible directions
            BoardSpace rockBoardSpace = rock.GetComponentInParent<BoardSpace>();              
            for(int dir=0; dir <= (int)eMoveDirs.UP_RIGHT; dir++)
            {            
                Vector2Int spaceToCheck = rockBoardSpace.SpaceCoords + MoveDeltas[dir];  
                // Check first passive board space (moving one space)
                if(CheckPassiveBoardSpace(spaceToCheck)) 
                {   // The first move is possible, so now check the 2nd move (moving two spaces)
                    BoardSpace validSpace = BoardSpaces[spaceToCheck.x,spaceToCheck.y];                                                    
                    RockMove.GetInstance().PossiblePassiveMoves.Add(MoveDeltas[dir]); // Add space to list
                    // Now check the space two moves away
                    spaceToCheck = rockBoardSpace.SpaceCoords + MoveDeltas[dir]*2;
                    if(CheckPassiveBoardSpace(spaceToCheck))
                    {   // It's also valid               
                        validSpace = BoardSpaces[spaceToCheck.x,spaceToCheck.y];                                              
                        RockMove.GetInstance().PossiblePassiveMoves.Add(MoveDeltas[dir]*2);                                                      
                    }
                }            
            }                           
            
            // If we have at least one valid passive move return true
            return RockMove.GetInstance().PossiblePassiveMoves.Count > 0;
        }

#endregion        

        
#region BOARD_MATINCENCE        
        /// <summary>
        /// Turns off all the highlights on the board
        /// </summary>
        public void ResetSpaceHighlights()
        {                    
            foreach(BoardSpace b in BoardSpaces)
            {
                b.ToggleHighlight(false, "both");
            }            
        }

        /// <summary>
        /// Resets the board to the starting state
        /// </summary>
        public void ResetBoard()
        {                           
            ResetSpaceHighlights(); // Shut off all the highlights
            // Re-activate all of the rocks that were pushed off the board
            foreach(Transform t in PushedOffRocks.transform)
            {
                t.gameObject.SetActive(true);
            }       
            // Get all of the rocks and order them by name
            List<Rock> rocks = GetComponentsInChildren<Rock>().ToList();
            rocks = rocks.OrderBy(x => x.name).ToList();

            // Go though the rocks list and re-assign them to their correct board space
            for(int x=0; x<NUM_ROWS_COLS; x++)
            {              
                rocks[x].transform.parent = BoardSpaces[x,0].transform;
                rocks[x].transform.localPosition = Vector3.zero;
            
                rocks[x+NUM_ROWS_COLS].transform.parent = BoardSpaces[x,3].transform;
                rocks[x+NUM_ROWS_COLS].transform.localPosition = Vector3.zero;
            }
        }
#endregion

#region MISC_BOARD_GAME_STATE
        /// <summary>
        /// Check to see if the game has ended based on the board state.
        /// </summary>
        /// <returns></returns>
        public bool CheckEndGame()
        {              
            // If there's no active rocks of one color then it's game over
            List<Rock> rocksOnBoard = GetComponentsInChildren<Rock>().ToList();
            int numBlackRemoved = rocksOnBoard.RemoveAll(x => x.RockColor == Shobu.eRockColors.BLACK);
            int numWhiteRemoved = rocksOnBoard.RemoveAll(x => x.RockColor == Shobu.eRockColors.WHITE);        
            return numBlackRemoved == 0 || numWhiteRemoved == 0;
        }

        /// <summary>
        /// Checks to see if the coordinates are on the board
        /// </summary>
        /// <param name="spaceCoords">Board coordinates to check</param>
        /// <returns></returns>
        public static bool AreCoordsOffBoard(Vector2Int spaceCoords)
        {
            return (spaceCoords.x < 0 || spaceCoords.x >= NUM_ROWS_COLS || 
                    spaceCoords.y < 0 || spaceCoords.y >= NUM_ROWS_COLS);
        }

        /// <summary>
        /// Keep rocks pushed off board on it's own list. 
        /// </summary>
        /// <param name="rock">Rock to put on the pushed off board list</param>
        public void PutRockOnPushedList(Rock rock)
        {
            rock.transform.parent = PushedOffRocks.transform;
            rock.gameObject.SetActive(false);
        }
    }
#endregion

}
