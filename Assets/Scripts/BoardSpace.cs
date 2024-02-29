using System;
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
        public Vector2Int SpaceCoords {get; private set;} // x,y coords for the space

        void Awake()
        {            
            BoardSpaceHighlight = GetComponent<SpriteRenderer>(); // Get a reference to the highlight graphic
            // Get the coordinates based on the GameObjectname
            string[] locString = name.Split(",");
            SpaceCoords = new Vector2Int(Int32.Parse(locString[0]), Int32.Parse(locString[1]));
        }

        /// <summary>
        /// Enables/disables the highlight as well as changing it's color
        /// </summary>
        /// <param name="isEnabled">Whether or not it's even on</param>
        /// <param name="color">Color for the highlight</param>
        public void ToggleHighlight(bool isEnabled, Color color)
        {            
            BoardSpaceHighlight.enabled = isEnabled;
            BoardSpaceHighlight.color = new Color(color.r, color.g, color.b, .5f);
        }
    }
}