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
        if(space.x < 0 || space.x >= NUM_ROWS_COLS || space.y < 0 || space.y >= NUM_ROWS_COLS)
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

    public bool IsValidMove(BoardSpace boardSpace)    
    {
       return ValidMoves.Contains(boardSpace);        
    }
    public void UpdateValidMoves(Rock rock, Shobu.eMoveType moveType)
    {
       // if(this.name.Equals("Light1") == false) return;
        //ValidMoves.Clear();
        BoardSpace rockBoardSpace = rock.GetComponentInParent<BoardSpace>();
        string[] locString = rockBoardSpace.name.Split(",");
        Vector2Int rockLoc = new Vector2Int(Int32.Parse(locString[0]), Int32.Parse(locString[1]));
       // Debug.Log("UpdateValidMoves rock: " + rock.name + ", at loc: " + rockLoc.ToString());

      //  Debug.Log("********************* UpdateValidmoves at: " + rockLoc.ToString());
        for(int dir=0; dir <= (int)eMoveDirs.UP_RIGHT; dir++)
        {
         //   Debug.Log("---------------------------------------------");            
            Vector2Int spaceToCheck = rockLoc + MoveDeltas[dir];       
        //    Debug.Log("Delta is: " + MoveDeltas[dir] + ", spaceToCheck is: " + spaceToCheck);     
            bool checkSpace = CheckSpace(spaceToCheck, moveType);
            if(checkSpace)
            {   
            //    Debug.Log("!!!!!!!!!!move to: " + spaceToCheck.ToString() + " is valid");
                BoardSpace validSpace = BoardSpaces[spaceToCheck.x,spaceToCheck.y];
                validSpace.ToggleHighlight(true);
                ValidMoves.Add(validSpace);
                spaceToCheck = rockLoc + MoveDeltas[dir]*2;
                if(CheckSpace(spaceToCheck, moveType))
                {
             //       Debug.Log("Delta is: " + MoveDeltas[dir]*2 + ", spaceToCheck is: " + spaceToCheck);     
             //       Debug.Log("!!!!!!!move to: " + spaceToCheck.ToString() + " is valid");
                    validSpace = BoardSpaces[spaceToCheck.x,spaceToCheck.y];
                    validSpace.ToggleHighlight(true);
                    ValidMoves.Add(validSpace);
                }
            }            
        }
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

        ValidMoves.Clear();
        for(int x = 0; x < NUM_ROWS_COLS; x++)
        {
            for(int y = 0; y < NUM_ROWS_COLS; y++)
            {
                BoardSpaces[x,y].ToggleHighlight(false);
            }
        }  
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
