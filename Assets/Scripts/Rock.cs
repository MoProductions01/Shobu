using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

//using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Radient
{
    public class Rock : MonoBehaviour
    {            
        [field: SerializeField] public Shobu.eRockColors RockColor {get; set;}   
        
        public Board MyBoard {get; set;}
        public Vector2Int PushedCoords {get; set;}


        private void Start() 
        {
            MyBoard = GetComponentInParent<Board>();        
        }

        private void OnTriggerEnter(Collider other) 
        {
            //Debug.Log(this.name + " trigger collided with: " + other.name);                        
            FindObjectOfType<Shobu>().CheckPushedRockCollision(this);
        }
    }
}