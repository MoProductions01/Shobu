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
    public Vector2Int PassiveMove {get; set;}

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
            
    public void Reset(Shobu.eMoveType moveType)
    {      
        PossiblePassiveMoves.Clear();
        ValidPassiveMoves.Clear();
        ValidBoardSpaces.Clear();
        PushedRock = null;
        if(moveType == Shobu.eMoveType.AGGRESSIVE) PassiveMove = Vector2Int.zero;
    }  
} // monote - add a namespace
