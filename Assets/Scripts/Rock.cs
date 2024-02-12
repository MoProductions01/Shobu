using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

//using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Rock : MonoBehaviour
{    
    
    [field: SerializeField] public Shobu.eGameColors RockColor {get; set;}   
     
    public Board MyBoard {get; set;}

    private void Start() 
    {
        MyBoard = GetComponentInParent<Board>();        
    }
}
