using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static int NUM_ROWS_COLS = 4;

    Shobu Shobu;

    public enum eBoardColor {DARK, LIGHT};
    [field: SerializeField] public eBoardColor BoardColor {get; set;} 
    public enum eMoveDirs {UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT, UP_RIGHT};
    public List<BoardSpace> ValidMoves = new List<BoardSpace>();
    //public List<Shobu.RockMove> ValidRockMoves = new List<Shobu.RockMove>();
    Vector2Int PassiveMove = Vector2Int.zero;
    public Rock PushedRock = null;
    public Vector2Int PushedRockCoords = new Vector2Int(-1, -1);
    List<Vector2Int> MoveDeltas = new List<Vector2Int> 
    {
        new Vector2Int(0, 1),    // UP
        new Vector2Int(-1, 1),  // UP_LEFT
        new Vector2Int(-1, 0),  // LEFT
        new Vector2Int(-1, -1),  // DOWN_LEFT
        new Vector2Int(0, -1),  // DOWN
        new Vector2Int(1, -1),  // DOWN_RIGHT
        new Vector2Int(1, 0),   // RIGHT
        new Vector2Int(1, 1),    // UP_RIGHT
    };
    
    //public List<List<BoardSpace>> BoardSpaces = new List<List<BoardSpace>>(NUM_ROWS_COLS);
    public BoardSpace[,] BoardSpaces = new BoardSpace[4,4];
    [SerializeField] GameObject PushedOffRocks;      

    bool CheckSpace(Vector2Int space, Shobu.eMoveType moveType)
    {        
        //if(space.x < 0 || space.x >= NUM_ROWS_COLS || space.y < 0 || space.y >= NUM_ROWS_COLS)
        if(AreCoordsOffBoard(space))
        {
            // off board
            return false;
        }
        bool hasRock = BoardSpaces[space.x,space.y].GetComponentInChildren<Rock>() != null;
        if(hasRock)
        {   // There's a rock on this space
            if(moveType == Shobu.eMoveType.PASSIVE)
            {   // can't move onto a rock in Passive move
                return false;
            }            
        }
        return true;      
    }   

    public bool IsValidMove(BoardSpace newBoardSpace)    
    {
       return ValidMoves.Contains(newBoardSpace);        
    }
    bool AreCoordsOffBoard(Vector2Int coords)
    {
        return (coords.x < 0 || coords.x >= NUM_ROWS_COLS || 
                coords.y < 0 || coords.y >= NUM_ROWS_COLS);
    }

    public bool CheckAggressiveMove(Rock rock, Vector2Int move)
    {
        BoardSpace rockBoardSpace = rock.GetComponentInParent<BoardSpace>();
        Vector2Int moveToSpace = rockBoardSpace.SpaceCoords + move;              

        //if(moveToSpace.x < 0 || moveToSpace.x >= NUM_ROWS_COLS || moveToSpace.y < 0 || moveToSpace.y >= NUM_ROWS_COLS)
        if(AreCoordsOffBoard(moveToSpace))
        {
            Debug.LogWarning("Aggressive move is off board so invalid");
            return false;
        }
        BoardSpaces[moveToSpace.x, moveToSpace.y].ToggleHighlight(true, Color.red);
        Vector2Int moveDir = moveToSpace - rockBoardSpace.SpaceCoords;
        Debug.Log("Checking Aggressive move from: " + rockBoardSpace.SpaceCoords.ToString() +" to: " + moveToSpace.ToString() +
            ", movePath was: " + moveDir.ToString());
        int numSpacesMoved = 1;
        if(Math.Abs(moveDir.x) == 2 || Math.Abs(moveDir.y) == 2) 
        {
            numSpacesMoved = 2;
            moveDir /= 2;
        }        
        Debug.Log("movePath was in this dir: " + moveDir.ToString() + " with #moves: " + numSpacesMoved);
        //bool validMove = true;
        //bool pushingRock = false;
        PushedRock = null;
        int i;
        Vector2Int coordsToCheck = Vector2Int.zero;
        for(i=1; i<=numSpacesMoved; i++)
        {
            coordsToCheck = rockBoardSpace.SpaceCoords + moveDir*i;
            Rock rockCheck = BoardSpaces[coordsToCheck.x, coordsToCheck.y].GetComponentInChildren<Rock>();
            if(rockCheck == null)
            {
                Debug.Log("Board space: " + coordsToCheck.ToString() + " has no rock so keep checking");
            }
            else if(PushedRock != null)
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
                Debug.Log("Board space: " + coordsToCheck.ToString() + " has a rock of the other color and we're not already pushing a rock so push this new rock");
                PushedRock = rockCheck;                                                
            }
        } 
                                
        if(PushedRock == null)
        {
            Debug.Log("We made it through the checks and aren't pushing a rock so it's a clear path");                        
        }
        else
        {
            Debug.Log("We made it through the checks and are pushing a rock, so check next space. i: " + i);
            coordsToCheck = rockBoardSpace.SpaceCoords + moveDir*i;
            //if(coordsToCheck.x < 0 || coordsToCheck.x >= NUM_ROWS_COLS || coordsToCheck.y < 0 || coordsToCheck.y >= NUM_ROWS_COLS)
            if(AreCoordsOffBoard(coordsToCheck))
            {
                Debug.Log("Pushed rock will go off board");     
                PushedRockCoords = coordsToCheck;           
            }
            else if(BoardSpaces[coordsToCheck.x, coordsToCheck.y].GetComponentInChildren<Rock>() != null)
            {
                Rock rockCheck = BoardSpaces[coordsToCheck.x, coordsToCheck.y].GetComponentInChildren<Rock>();
                Debug.LogWarning("Will push rock into another rock so invalid");
                return false;
            }
            else
            {
                Debug.Log("Pushed rock will move into an empty space at: " + coordsToCheck.ToString());    
                PushedRockCoords = coordsToCheck;                          
            }
        }               

        // made it so it's a valid move    
        ValidMoves.Add(BoardSpaces[moveToSpace.x, moveToSpace.y]);   
        ValidMoves[0].ToggleHighlight(true, Color.blue); 
        return true; 
    }

    public void CheckPushedRock()
    {
        if(PushedRock != null)
        {
            if(AreCoordsOffBoard(PushedRockCoords))
            {
                PutRockOnPushedList(PushedRock);
            }
            else
            {
                PushedRock.transform.parent = 
                    BoardSpaces[PushedRockCoords.x, PushedRockCoords.y].transform;
                PushedRock.transform.localPosition = Vector3.zero;
            }
        }
    }

    public bool UpdatePassiveValidMoves(Rock rock, Shobu.eMoveType moveType)
    {
       // if(this.name.Equals("Light1") == false) return;
        ValidMoves.Clear();
        //ValidRockMoves.Clear();
        BoardSpace rockBoardSpace = rock.GetComponentInParent<BoardSpace>();        
      //  Debug.Log("********************* UpdateValidmoves at: " + rockLoc.ToString());
        for(int dir=0; dir <= (int)eMoveDirs.UP_RIGHT; dir++)
        {
         //   Debug.Log("---------------------------------------------");            
            Vector2Int spaceToCheck = rockBoardSpace.SpaceCoords + MoveDeltas[dir];       
        //    Debug.Log("Delta is: " + MoveDeltas[dir] + ", spaceToCheck is: " + spaceToCheck);     
            bool checkSpace = CheckSpace(spaceToCheck, moveType);
            if(checkSpace)
            {   
            //    Debug.Log("!!!!!!!!!!move to: " + spaceToCheck.ToString() + " is valid");
                BoardSpace validSpace = BoardSpaces[spaceToCheck.x,spaceToCheck.y];
                
            //    ValidRockMoves.Add(new Shobu.RockMove((eMoveDirs)dir, 1));
                ValidMoves.Add(validSpace);
                spaceToCheck = rockBoardSpace.SpaceCoords + MoveDeltas[dir]*2;
                if(CheckSpace(spaceToCheck, moveType))
                {
             //       Debug.Log("Delta is: " + MoveDeltas[dir]*2 + ", spaceToCheck is: " + spaceToCheck);     
             //       Debug.Log("!!!!!!!move to: " + spaceToCheck.ToString() + " is valid");
                    validSpace = BoardSpaces[spaceToCheck.x,spaceToCheck.y];                    
                    //ValidRockMoves.Add(new Shobu.RockMove((eMoveDirs)dir, 2));
                    ValidMoves.Add(validSpace);
                }
            }            
        }        
        foreach(BoardSpace boardSpace in ValidMoves)
        {
            boardSpace.ToggleHighlight(true, Color.blue);
        }
        return ValidMoves.Count > 0;
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
        ValidMoves.Clear();
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
        int numBlackRemoved = rocksOnBoard.RemoveAll(x => x.RockColor == Shobu.eGameColors.BLACK);
        int numWhiteRemoved = rocksOnBoard.RemoveAll(x => x.RockColor == Shobu.eGameColors.WHITE);

        Debug.Log("numBlackRemoved: " + numBlackRemoved + ", numWhiteRemoved: " + numWhiteRemoved);
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
