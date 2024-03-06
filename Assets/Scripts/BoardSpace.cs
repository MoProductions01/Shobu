using System;
using System.Drawing;
using UnityEngine;

namespace Radient
{
    /// <summary>
    /// Class for each of the spaces on a board.  Mostly for keeping track of 
    /// it's world space as well as highlights for valid or invalid moves
    /// </summary>
    public class BoardSpace : MonoBehaviour
    {
        SpriteRenderer BoardSpaceHighlight; // Graphic for the space highlight
        [SerializeField] private GameObject purpleSquareVFX;
        [SerializeField] private GameObject redSquareVFX;
        [SerializeField] private GameObject spaceShadowVFX;
        public Vector2Int SpaceCoords {get; private set;} // x,y coords for the space

        void Awake()
        {
            purpleSquareVFX.SetActive(false);
            redSquareVFX.SetActive(false);
            //BoardSpaceHighlight = GetComponent<SpriteRenderer>(); // Get a reference to the highlight graphic
            //BoardSpaceHighlight.enabled = false;
            // Get the coordinates based on the GameObjectname
            string[] locString = name.Split(",");
            SpaceCoords = new Vector2Int(Int32.Parse(locString[0]), Int32.Parse(locString[1]));
        }

        /// <summary>
        /// Enables/disables the highlight as well as changing it's color
        /// </summary>
        /// <param name="isEnabled">Whether or not it's even on</param>
        /// <param name="typeOfMove">Type of move that is referencing</param>
        public void ToggleHighlight(bool isEnabled, string typeOfMove)
        {
            switch (typeOfMove)
            {
                case "passive":
                    purpleSquareVFX.SetActive(isEnabled);
                    BoardSpaceHighlight.enabled = isEnabled;
                    BoardSpaceHighlight.color = new UnityEngine.Color(0.643f, 0.18f, 1, .2f);
                    break;

                case "agressive":
                    redSquareVFX.SetActive(isEnabled);
                    BoardSpaceHighlight.color = new UnityEngine.Color(1, 0.118f, 0.118f, .2f);
                    BoardSpaceHighlight.enabled = isEnabled;
                    break;

                case "both":
                    BoardSpaceHighlight.enabled = isEnabled;
                    purpleSquareVFX.SetActive(isEnabled);
                    redSquareVFX.SetActive(isEnabled);
                    break;
            }
            BoardSpaceHighlight.enabled = isEnabled;
            //BoardSpaceHighlight.color = new Color(color.r, color.g, color.b, .5f);
        }
    }
}