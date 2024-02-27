using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Radient
{
    public class Board : MonoBehaviour
    {
        public static int NUM_ROWS_COLS = 4;

        Shobu Shobu;

        public enum eBoardColor {DARK, LIGHT};
        [field: SerializeField] public eBoardColor BoardColor {get; set;} 
        public enum eMoveDirs {UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT, UP_RIGHT};
        //public List<BoardSpace> ValidMoves = new List<BoardSpace>();
        //public List<Shobu.RockMove> ValidRockMoves = new List<Shobu.RockMove>();
        /*Vector2Int PassiveMove = Vector2Int.zero;
        public Rock PushedRock = null;
        public Vector2Int PushedRockCoords = new Vector2Int(-1, -1);*/
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
        
        //public List<List<BoardSpace>> BoardSpaces = new List<List<BoardSpace>>(NUM_ROWS_COLS);
        public BoardSpace[,] BoardSpaces = new BoardSpace[4,4];
        [SerializeField] GameObject PushedOffRocks;      

        public bool CheckPassiveSpace(Vector2Int space)
        {        
            //if(space.x < 0 || space.x >= NUM_ROWS_COLS || space.y < 0 || space.y >= NUM_ROWS_COLS)
            if(AreCoordsOffBoard(space) ||
                                BoardSpaces[space.x,space.y].GetComponentInChildren<Rock>() != null)
            {
                // off board or space has a rock
                return false;
            }       
            return true;      
        }   
        
        bool AreCoordsOffBoard(Vector2Int coords)
        {
            return (coords.x < 0 || coords.x >= NUM_ROWS_COLS || 
                    coords.y < 0 || coords.y >= NUM_ROWS_COLS);
        }
        

        public bool CheckIfAnyValidAggressiveMoves(Rock heldRock)
        {               
            List<Rock> rocksToCheck = GetComponentsInChildren<Rock>().ToList();
            List<Rock> validRocks = new List<Rock>();        
            rocksToCheck.RemoveAll(x => x.RockColor != heldRock.RockColor);        
            //foreach(Vector2Int passiveMove in RockMove.GetInstance().PassiveMoves())       
            //foreach(Vector2Int passiveMove in Shobu.PassiveMovesToCheck)
            foreach(Vector2Int passiveMove in RockMove.GetInstance().PossiblePassiveMoves)
            {
                validRocks.Clear();            
                foreach(Rock rock in rocksToCheck)
                {
                    if(CheckAggressiveMove(rock, passiveMove, true) == true)
                    {
                        validRocks.Add(rock);      
                        // monote - add a continue/break here              
                    }
                }            
                if(validRocks.Count > 0)
                {                                   
                    RockMove.GetInstance().AddValidPassiveMove(passiveMove);
                }
            }        
            return false;
        }

        public bool CheckAggressiveMove(Rock rock, Vector2Int move, bool isTest)
        {
            BoardSpace rockBoardSpace = rock.GetComponentInParent<BoardSpace>();
            Vector2Int moveToSpace = rockBoardSpace.SpaceCoords + move;              
            
            if(AreCoordsOffBoard(moveToSpace))
            {
                //Debug.LogWarning("Aggressive move is off board so invalid");
                return false;
            }
            
            Vector2Int moveDir = moveToSpace - rockBoardSpace.SpaceCoords;
            //Debug.Log("Checking Aggressive move from: " + rockBoardSpace.SpaceCoords.ToString() +" to: " + moveToSpace.ToString() + ", movePath was: " + moveDir.ToString());
            int numSpacesMoved = 1;
            if(Math.Abs(moveDir.x) == 2 || Math.Abs(moveDir.y) == 2) 
            {
                numSpacesMoved = 2;
                moveDir /= 2;
            }        
        // Debug.Log("movePath was in this dir: " + moveDir.ToString() + " with #moves: " + numSpacesMoved);        
            //PushedRock = null;
            Rock pushedRock = null;
            int i;
            Vector2Int coordsToCheck = Vector2Int.zero;
            for(i=1; i<=numSpacesMoved; i++)
            {
                coordsToCheck = rockBoardSpace.SpaceCoords + moveDir*i;
                Rock rockCheck = BoardSpaces[coordsToCheck.x, coordsToCheck.y].GetComponentInChildren<Rock>();
                if(rockCheck == null)
                {
                //  Debug.Log("Board space: " + coordsToCheck.ToString() + " has no rock so keep checking");
                }
                else if(pushedRock != null)
                {
                    Debug.LogWarning("Can't push more than 1 rock so invalid");
                    return false;
                }
                else if(rockCheck.RockColor == rock.RockColor)
                {
                    Debug.LogWarning("Board space: " + coordsToCheck.ToString() + " has Rock of own color in path so invalid.");
                    return false;
                }
                else
                {   
                // Debug.Log("Board space: " + coordsToCheck.ToString() + " has a rock of the other color and we're not already pushing a rock so push this new rock");
                    pushedRock = rockCheck;                                                
                }
            } 
                                    
            if(pushedRock == null)
            {
            // Debug.Log("We made it through the checks and aren't pushing a rock so it's a clear path");                        
            }
            else
            {
            // Debug.Log("We made it through the checks and are pushing a rock, so check next space. i: " + i);
                coordsToCheck = rockBoardSpace.SpaceCoords + moveDir*i;
                //if(coordsToCheck.x < 0 || coordsToCheck.x >= NUM_ROWS_COLS || coordsToCheck.y < 0 || coordsToCheck.y >= NUM_ROWS_COLS)
                if(AreCoordsOffBoard(coordsToCheck))
                {
                //  Debug.Log("Pushed rock will go off board");     
                    //PushedRockCoords = coordsToCheck;           
                    pushedRock.PushedCoords = coordsToCheck;
                }
                else if(BoardSpaces[coordsToCheck.x, coordsToCheck.y].GetComponentInChildren<Rock>() != null)
                {
                // Rock rockCheck = BoardSpaces[coordsToCheck.x, coordsToCheck.y].GetComponentInChildren<Rock>();
                    Debug.LogWarning("Will push rock into another rock so invalid");
                    return false;
                }
                else
                {
                // Debug.Log("Pushed rock will move into an empty space at: " + coordsToCheck.ToString());    
                    //PushedRockCoords = coordsToCheck;                          
                    pushedRock.PushedCoords = coordsToCheck;
                }
            }               

            // made it so it's a valid move    
            if(isTest == false)
            {
            // ValidMoves.Add(BoardSpaces[moveToSpace.x, moveToSpace.y]);   
            // ValidMoves[0].ToggleHighlight(true, Color.blue); 
            RockMove.GetInstance().PushedRock = pushedRock;
            }
            else
            {
            // PushedRock = null;
                //PushedRockCoords = new Vector2Int(-1,-1);
            }
            
            return true; 
        }

        public void CheckPushedRock()
        {        
            if(RockMove.GetInstance().PushedRock != null)
            {
                Rock pushedRock = RockMove.GetInstance().PushedRock;
                if(AreCoordsOffBoard(pushedRock.PushedCoords))
                {
                    PutRockOnPushedList(pushedRock);
                }
                else
                {
                    pushedRock.transform.parent = 
                        BoardSpaces[pushedRock.PushedCoords.x, pushedRock.PushedCoords.y].transform;
                    pushedRock.transform.localPosition = Vector3.zero;
                }
            }
        }

        public bool UpdatePossiblePassiveMoves(Rock rock)
        {               
            BoardSpace rockBoardSpace = rock.GetComponentInParent<BoardSpace>();              
            for(int dir=0; dir <= (int)eMoveDirs.UP_RIGHT; dir++)
            {            
                Vector2Int spaceToCheck = rockBoardSpace.SpaceCoords + MoveDeltas[dir];      
                if(CheckPassiveSpace(spaceToCheck))
                {                   
                    BoardSpace validSpace = BoardSpaces[spaceToCheck.x,spaceToCheck.y];                                
                    //RockMove.GetInstance().AddPassiveMove(MoveDeltas[dir]);
                    RockMove.GetInstance().PossiblePassiveMoves.Add(MoveDeltas[dir]);
                    spaceToCheck = rockBoardSpace.SpaceCoords + MoveDeltas[dir]*2;
                    if(CheckPassiveSpace(spaceToCheck))
                    {                    
                        validSpace = BoardSpaces[spaceToCheck.x,spaceToCheck.y];    
                        //RockMove.GetInstance().AddPassiveMove(MoveDeltas[dir]*2);                                                      
                        RockMove.GetInstance().PossiblePassiveMoves.Add(MoveDeltas[dir]*2);                                                      
                    }
                }            
            }                           

            //return RockMove.GetInstance().NumPassiveMovesToCheck() > 0;             
            //return RockMove.GetInstance().NumPassiveMoves() > 0;
            return RockMove.GetInstance().PossiblePassiveMoves.Count > 0;
            //return Shobu.PassiveMovesToCheck.Count > 0;
        }

        

        // Start is called before the first frame update
        void Start()
        {
            Shobu = FindObjectOfType<Shobu>();          
        } 

        private void Awake() 
        {        
        // if(this.name.Equals("Light1") == false) return;
            
            List<BoardSpace> boardSpaces = GetComponentsInChildren<BoardSpace>().ToList();
            foreach(BoardSpace boardSpace in boardSpaces)
            {
                // assign the board space based on x,y of the name
                string[] locString = boardSpace.name.Split(",");
                Vector2Int spaceLoc = new Vector2Int(Int32.Parse(locString[0]), Int32.Parse(locString[1]));                
            //   Debug.Log("spaceLoc (" + spaceLoc.x + "," + spaceLoc.y + ") is getting boardSpace: " + boardSpace.name);  
                BoardSpaces[spaceLoc.x, spaceLoc.y] = boardSpace;
            }                    
        }

        public void ResetSpaceHighlights()
        {
        // if(this.name.Equals("Light1") == false) return;
    //        Debug.Log("ResetSpaceHighlights");
        // ValidMoves.Clear();
            for(int x = 0; x < NUM_ROWS_COLS; x++)
            {
                for(int y = 0; y < NUM_ROWS_COLS; y++)
                {
                    BoardSpaces[x,y].ToggleHighlight(false, Color.clear);
                }
            }  
        }

        public bool CheckEndGame()
        {   // boardObjectColliders.RemoveAll(x => x.GetComponentInParent<BoardObject>() == null);                
            List<Rock> rocksOnBoard = GetComponentsInChildren<Rock>().ToList();
            int numBlackRemoved = rocksOnBoard.RemoveAll(x => x.RockColor == Shobu.eRockColors.BLACK);
            int numWhiteRemoved = rocksOnBoard.RemoveAll(x => x.RockColor == Shobu.eRockColors.WHITE);

        // Debug.Log("numBlackRemoved: " + numBlackRemoved + ", numWhiteRemoved: " + numWhiteRemoved);
            return numBlackRemoved == 0 || numWhiteRemoved == 0;
        }

        public void ResetBoard()
        {                   
        // if(this.name.Equals("Light1") == false) return;
            ResetSpaceHighlights();

            foreach(Transform t in PushedOffRocks.transform)
            {
                t.gameObject.SetActive(true);
            }       
            List<Rock> rocks = GetComponentsInChildren<Rock>().ToList();
            rocks = rocks.OrderBy(x => x.name).ToList();

            for(int x=0; x<NUM_ROWS_COLS; x++)
            {              
                rocks[x].transform.parent = BoardSpaces[x,0].transform;
                rocks[x].transform.localPosition = Vector3.zero;
            
                rocks[x+NUM_ROWS_COLS].transform.parent = BoardSpaces[x,3].transform;
                rocks[x+NUM_ROWS_COLS].transform.localPosition = Vector3.zero;
            }
        }

        public void PutRockOnPushedList(Rock rock)
        {
            rock.transform.parent = PushedOffRocks.transform;
            rock.gameObject.SetActive(false);
        }
    }
}
