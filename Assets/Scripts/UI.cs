using TMPro;
using UnityEngine;

namespace Radient
{
    /// <summary>
    /// The main UI script for the in-game UI elements
    /// </summary>
    public class UI : MonoBehaviour
    {        
        [SerializeField] TMP_Text GameStateText; // Text for the current game state        

        /// <summary>
        /// When enabled, subscribe to the game state change stuff so we can update
        /// </summary>
        private void OnEnable()     
        {        
           if(FindObjectOfType<Shobu>() != null )
            {
                FindObjectOfType<Shobu>().OnGameStateChangeAction += UpdateUI;    
            }   
        }

        /// <summary>
        /// When disabled, unsubscribe to the game state change stuff
        /// </summary>
        private void OnDisable() 
        {
            if(FindObjectOfType<Shobu>() != null )
            {
                FindObjectOfType<Shobu>().OnGameStateChangeAction -= UpdateUI;    
            }            
        }

        /// <summary>
        /// Callback for the ResetGame button
        /// </summary>
        public void ResetGame()
        {
            FindObjectOfType<Shobu>().ResetGame();
        }

        /// <summary>
        /// Called from Shobu to update the UI elements
        /// </summary>
        /// <param name="gameState">Current GameState</param>
        /// <param name="player">Which color rock has current turn</param>
        /// <param name="moveType">Passive or Aggressive move</param>
        void UpdateUI(Shobu.eGameState gameState, Shobu.eRockColors player, Shobu.eMoveType moveType)
        {            
            if(gameState == Shobu.eGameState.PLAYING)
            {
                GameStateText.text = "Player: " + player + "  Move: " + moveType;            
            }
            else
            {
                GameStateText.text = "WINNER: " + player + "!!!!!!!!!";
            }
        }
    }
}
