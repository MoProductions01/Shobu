using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RockMove
{
    private static RockMove instance = null;

    public static RockMove GetInstance()
    {
        if(instance == null)
        {         
            instance = new RockMove();
        }        
        return instance;
    }

    public List<Vector2Int> PossiblePassiveMoves {get; private set; } = new List<Vector2Int>();
    public List<Vector2Int> ValidPassiveMoves {get; private set;} =  new List<Vector2Int>();    
    public List<BoardSpace> ValidBoardSpaces {get; private set; } = new List<BoardSpace>();
    public Rock PushedRock {get; set;}

    public IEnumerable<Vector2Int> GetInvalidPassiveMoves()
    {
        return PossiblePassiveMoves.Except(ValidPassiveMoves).ToList();
    }

    public void AddValidPassiveMove(Vector2Int validPassiveMove)
    {    
        if(ValidPassiveMoves.Contains(validPassiveMove) == false)
        {
            ValidPassiveMoves.Add(validPassiveMove);
        }            
    } 

    /*public void AddValidBoardSpace(BoardSpace boardSpace)
    {
        ValidBoardSpaces.Add(boardSpace);
    }*/
    /*public IEnumerable<BoardSpace> GetValidBoardSpaces()
    {
        return ValidBoardSpaces;
    }  */

   /* public IEnumerable<Vector2Int> GetValidPassiveMoves()
    {
        return ValidPassiveMoves;
    }*/
    /*public int GetNumValidPassiveMoves()
    {
        return ValidPassiveMoves.Count();
    }*/
    

    
   /* public void AddPassiveMove(Vector2Int passiveMove)
    {
        PossiblePassiveMoves.Add(passiveMove);
    }*/
    /*public IEnumerable<Vector2Int> PassiveMoves()
    {
        return PossiblePassiveMoves;
    }*/
    /*public int NumPassiveMoves()
    {
        return PossiblePassiveMoves.Count;
    }*/
            
    public void Reset()
    {
        //Debug.Log("RockMove.Reset() --Sin--");
        PossiblePassiveMoves.Clear();
        ValidPassiveMoves.Clear();
        ValidBoardSpaces.Clear();
        PushedRock = null;
    }  
} // monote - add a namespace
