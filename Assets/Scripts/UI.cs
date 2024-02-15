using System.Collections;
using System.Collections.Generic;
using Radient;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    //Shobu Shobu;
    [SerializeField] TMP_Text GameStateText;

    // Start is called before the first frame update
    void Awake()
    {
       // Shobu = FindObjectOfType<Shobu>();
        //Shobu.OnGameStateChange.AddListener(UpdateUI);
       // Shobu.OnGameStateChangeAction += UpdateUI; // Actions prevent anything but subscribing and unsubscribing
    }    

    private void OnEnable()     
    {        
        FindObjectOfType<Shobu>().OnGameStateChangeAction += UpdateUI;
    }

    private void OnDisable() 
    {
        FindObjectOfType<Shobu>().OnGameStateChangeAction -= UpdateUI;    
    }

    public void ResetGame()
    {
        FindObjectOfType<Shobu>().ResetGame();
    }

    void UpdateUI(Shobu.eGameState gameState, Shobu.eRockColors player, Shobu.eMoveType moveType)
    {
        //Debug.Log("Update the UI");
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
