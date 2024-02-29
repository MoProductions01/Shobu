using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Radient
{
    /// <summary>
    /// Singleton class that handles the details of the user's next rock move
    /// </summary>
    public class RockMove
    {
        // Usual singleton instance stuff.  
        private static RockMove instance = null;
        public static RockMove GetInstance()
        {
            if(instance == null)
            {         
                instance = new RockMove();
            }        
            return instance;
        }
        
        // List of possible Passive moves for the selected rock
        public List<Vector2Int> PossiblePassiveMoves {get; private set; } = new List<Vector2Int>();
        // List of the Passive moves that are actually valid
        public List<Vector2Int> ValidPassiveMoves {get; private set;} =  new List<Vector2Int>();    
        // The BoardSpaces that the valid passive moves can move the rock to
        public List<BoardSpace> ValidBoardSpaces {get; private set; } = new List<BoardSpace>();
        public Rock PushedRock {get; set;}  // If a rock is going to get pushed by the selected rock
        public Vector2Int PassiveMove {get; set;}   // The current Passive move to check Aggressive moves against

        /// <summary>
        /// Returns a list of invalid passive moves by removing the valid ones from
        /// all of the possible ones
        /// </summary>
        /// <returns>List of all moves in the Possible list not in the Valid list</returns>
        public IEnumerable<Vector2Int> GetInvalidPassiveMoves()
        {
            return PossiblePassiveMoves.Except(ValidPassiveMoves).ToList();
        }

        /// <summary>
        /// Adds a Valid passive move to the list after checking the move from
        /// the Possible list.
        /// </summary>
        /// <param name="validPassiveMove">The x,y move to check</param>
        public void AddValidPassiveMove(Vector2Int validPassiveMove)
        {    
            // Add the Valid passive move if it's already not on the list            
            if(ValidPassiveMoves.Contains(validPassiveMove) == false)
            {
                ValidPassiveMoves.Add(validPassiveMove);
            }            
        }     

        /// <summary>
        /// Resets all of the info for the current RockMove (except the current
        /// Passive move, which gets handled in Shobu.EndMove)
        /// </summary>  
        public void Reset()
        {      
            PossiblePassiveMoves.Clear();
            ValidPassiveMoves.Clear();
            ValidBoardSpaces.Clear();
            PushedRock = null;            
        }  
    } 
}
