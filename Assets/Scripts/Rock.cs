using UnityEngine;

namespace Radient
{
    /// <summary>
    /// Class to hold the necessary stuff for one of the rock pieces on the board
    /// </summary>
    public class Rock : MonoBehaviour
    {            
        [field: SerializeField] public Shobu.eRockColors RockColor {get; set;} // Color of the rock        
        public Board MyBoard {get; private set;} // The board this rock belongs to
        public Vector2Int PushedCoords {get; set;}  // the coordinates this rock will get pushed to during an Aggressive move

        // Start is called before the first frame update
        private void Start() 
        {
            MyBoard = GetComponentInParent<Board>();        
        }

        /// <summary>
        /// The rock to rock physics is disabled most of the time, but during
        /// an Aggressive move it's turned back on in case a rock is getting pushed.        
        /// </summary>
        /// <param name="other">Other collider this rock is colliding with</param>
        private void OnTriggerEnter(Collider other) 
        {            
            // Check with Shobu if we're a pushed rock getting hit by the moving selected rock
            FindObjectOfType<Shobu>().CheckPushedRockCollision(this);
        }
    }
}