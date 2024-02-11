using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Rock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public enum eRockColor {DARK, LIGHT};

    [field: SerializeField] private eRockColor RockColor {get; set;}
    
   // private RectTransform RectTransform;
    // Start is called before the first frame update
    void Start()
    {
        //RectTransform = GetComponent<RectTransform>();
    }    

    public void OnDrag(PointerEventData eventData)
    {        
        Vector3 screenToworld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(screenToworld.x, screenToworld.y, 0f);        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag: " + this.name);        
        transform.localScale *= 1.2f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag: " + this.name);
        transform.localScale /= 1.2f;
    }
}
