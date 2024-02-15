using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RockMove
{
    private static RockMove instance = null;

    List<Vector2Int> PassiveMovesToCheck = new List<Vector2Int>();
    List<Vector2Int> ValidPassiveMoves =  new List<Vector2Int>();    
    List<BoardSpace> ValidBoardSpaces = new List<BoardSpace>();
    Rock PushedRock;

    public static RockMove GetInstance()
    {
        if(instance == null)
        {         
            instance = new RockMove();
        }        
        return instance;
    }

    public void AddPassiveMove(Vector2Int passiveMove)
    {
        PassiveMovesToCheck.Add(passiveMove);
    }
    public IEnumerable<Vector2Int> PassiveMoves()
    {
        return PassiveMovesToCheck;
    }
    public int NumPassiveMoves()
    {
        return PassiveMovesToCheck.Count;
    }

    public IEnumerable<Vector2Int> GetInvalidPassiveMoves()
    {
        return PassiveMovesToCheck.Except(ValidPassiveMoves).ToList();
    }

    public IEnumerable<Vector2Int> GetValidPassiveMoves()
    {
        return ValidPassiveMoves;
    }
    public int GetNumValidPassiveMoves()
    {
        return ValidPassiveMoves.Count();
    }

    public void AddValidPassiveMove(Vector2Int validPassiveMove)
    {    
        if(ValidPassiveMoves.Contains(validPassiveMove) == false)
        {
            ValidPassiveMoves.Add(validPassiveMove);
        }            
    }    

    public void AddValidBoardSpace(BoardSpace boardSpace)
    {
        ValidBoardSpaces.Add(boardSpace);
    }
    public IEnumerable<BoardSpace> GetValidBoardSpaces()
    {
        return ValidBoardSpaces;
    }

    public void DebugCheck()
    {
        int x = 5;
        x++;
    }


    
    
    public void Reset()
    {
        //Debug.Log("RockMove.Reset() --Sin--");
        PassiveMovesToCheck.Clear();
        ValidPassiveMoves.Clear();
        ValidBoardSpaces.Clear();
        PushedRock = null;
    }  
} // monote - add a namespace
