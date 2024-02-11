using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Rock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    
    public enum eRockColor {BLACK, WHITE};
    [field: SerializeField] public eRockColor RockColor {get; set;}         

    private void Start() 
    {
             
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
    }
}
