using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    SpriteRenderer BoardSpaceHighlight;
    public Vector2Int SpaceCoords {get; set;} // monote - get the script order sorted so that this is handled better

    

    void Awake()
    {
        BoardSpaceHighlight = GetComponent<SpriteRenderer>();
        string[] locString = name.Split(",");
        SpaceCoords = new Vector2Int(Int32.Parse(locString[0]), Int32.Parse(locString[1]));
    }

    public void ToggleHighlight(bool isEnabled, Color color)
    {
        //Debug.Log("Toggle Highlight");
        BoardSpaceHighlight.enabled = isEnabled;
        BoardSpaceHighlight.color = new Color(color.r, color.g, color.b, .5f);
    }
}