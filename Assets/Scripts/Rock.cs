using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

//using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Rock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{    
    public enum eRockColor {BLACK, WHITE};
    [field: SerializeField] public eRockColor RockColor {get; set;}   

    int BoardSpaceMask;     
    Board MyBoard;

    private void Start() 
    {
        MyBoard = transform.parent.parent.parent.parent.GetComponent<Board>();
        BoardSpaceMask = LayerMask.GetMask("Board Space");
    }

    public void OnDrag(PointerEventData eventData)
    {        
        Vector3 screenToworld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(screenToworld.x, screenToworld.y, 0f);        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {        
        transform.localScale *= 1.2f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {     
        transform.localScale /= 1.2f;

        Vector3 camPosition = Camera.main.transform.localPosition;
        Vector3 mouseZ = Camera.main.WorldToScreenPoint(new Vector3(0, 0,Input.mousePosition.z));
        Vector3 origin = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mouseZ.z));                       
        RaycastHit2D hit2D =Physics2D.Raycast(new Vector2(origin.x, origin.y), Vector2.up, Mathf.Infinity, BoardSpaceMask);

        if(hit2D.collider) 
        {            
            GameObject hitSpace = hit2D.collider.gameObject;            
            string[] rowCol = hitSpace.name.Split(",");    
            Vector2Int loc = new Vector2Int(Int32.Parse(rowCol[0]), Int32.Parse(rowCol[1]));
           // Debug.Log("loc: " + loc.ToString());

            
            //Board myBoard = transform.parent.parent.parent.parent.GetComponent<Board>();
            Board spaceBoard = hit2D.collider.transform.parent.parent.parent.GetComponent<Board>();
            if(MyBoard == spaceBoard)
            {
                //Debug.Log("On correct board");
                if(hitSpace.GetComponentInChildren<Rock>() != null)
                {
                //    Debug.Log("There's a rock on this space so don't put it here");                    
                }
                else
                {
                  //  Debug.Log("Space is free so place rock");
                    transform.parent = hitSpace.transform;                    
                }                                
            }                       
            else
            {
                //Debug.Log("Incorrect board");                
            }
            transform.localPosition = Vector3.zero;
        } 
        else 
        {
            //Debug.Log("Null Hit");
            MyBoard.PutRockOnPushedList(this);            
        }
    }
}
