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
    
//    [SerializeField] GameObject BoardSpaces;
    public List<List<BoardSpace>> BoardSpaces = new List<List<BoardSpace>>(NUM_ROWS_COLS);
    [SerializeField] GameObject PushedOffRocks;   
   // int BoardSpaceLayer; 

    // Start is called before the first frame update
    void Start()
    {
        Shobu = FindObjectOfType<Shobu>();          
    }

    private void Awake() 
    {
        List<BoardSpace> boardSpaces = GetComponentsInChildren<BoardSpace>().ToList();
        boardSpaces = boardSpaces.OrderBy(x => x.name).ToList();
        for(int i = 0; i<NUM_ROWS_COLS; i++)
        {
            BoardSpaces.Add(new List<BoardSpace>(NUM_ROWS_COLS));            
            for(int j=0; j<NUM_ROWS_COLS; j++)
            {                
                BoardSpaces[i].Add(boardSpaces[(i*NUM_ROWS_COLS) + j]);
            }
        }
      //  BoardSpaceLayer = LayerMask.NameToLayer("Board Space");
    }

    public void ResetBoard()
    {                   
        for(int i = 0; i < NUM_ROWS_COLS; i++)
        {
            for(int j = 0; j < NUM_ROWS_COLS; j++)
            {
                BoardSpaces[i][j].GetComponent<SpriteRenderer>().enabled = false;
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
            rocks[i].transform.parent = BoardSpaces[0][i].transform;
            rocks[i].transform.localPosition = Vector3.zero;
           
            rocks[i+NUM_ROWS_COLS].transform.parent = BoardSpaces[3][i].transform;
            rocks[i+NUM_ROWS_COLS].transform.localPosition = Vector3.zero;
        }
    }

    public void PutRockOnPushedList(Rock rock)
    {
        rock.transform.parent = PushedOffRocks.transform;
        rock.gameObject.SetActive(false);
    }
}
