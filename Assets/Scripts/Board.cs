using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static int NUM_ROWS_COLS = 4;

    Shobu Shobu;

    public enum eBoardColor {DARK, LIGHT};
    [field: SerializeField] public eBoardColor BoardColor {get; set;} 
    
    [SerializeField] GameObject BoardSpaces;
    [SerializeField] GameObject PushedOffRocks;   
    int BoardSpaceLayer; 

    // Start is called before the first frame update
    void Start()
    {
        Shobu = FindObjectOfType<Shobu>();          
    }

    private void Awake() 
    {
        BoardSpaceLayer = LayerMask.NameToLayer("Board Space");
    }

    public void ResetBoard()
    {                   
        List<Transform> children = GetComponentsInChildren<Transform>().ToList();
        foreach(Transform child in children)
        {            
            int childlayer = child.gameObject.layer;            
            if(childlayer == BoardSpaceLayer)
            {                                
                child.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        foreach(Transform t in PushedOffRocks.transform)
        {
            t.gameObject.SetActive(true);
        }       
        List<Rock> rocks = GetComponentsInChildren<Rock>().ToList();
        rocks = rocks.OrderBy(x => x.name).ToList();

        for(int i=0; i<NUM_ROWS_COLS; i++)
        {  
            rocks[i].gameObject.SetActive(true);
            rocks[i].transform.parent = BoardSpaces.transform.GetChild(NUM_ROWS_COLS-1).GetChild(i);
            rocks[i].transform.localPosition = Vector3.zero;

            rocks[i+NUM_ROWS_COLS].gameObject.SetActive(true);
            rocks[i+NUM_ROWS_COLS].transform.parent = BoardSpaces.transform.GetChild(0).GetChild(i);
            rocks[i+NUM_ROWS_COLS].transform.localPosition = Vector3.zero;
        }
    }

    public void PutRockOnPushedList(Rock rock)
    {
        rock.transform.parent = PushedOffRocks.transform;
        rock.gameObject.SetActive(false);
    }
}
