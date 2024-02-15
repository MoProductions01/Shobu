using System.Collections;
using System.Collections.Generic;
using Radient;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    Shobu Shobu;
    [SerializeField] TMP_Text GameStateText;

    // Start is called before the first frame update
    void Start()
    {
        Shobu = FindObjectOfType<Shobu>();
    }

    void Update()
    {
        GameStateText.text = "Current Player: " + Shobu.CurrentRockColor + "    Current Move: " + Shobu.CurrentMove;
    }

    public void ResetGame()
    {
        Shobu.ResetGame();
    }
}
