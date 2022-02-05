using System;
using UnityEngine;

namespace TowerDefense
{
    public enum SelectionEvent
    {
        Selection,
        Clear,
    }
    
    public class SelectionSubSystem : MonoBehaviour, IInteractionSubSystem<SelectionEvent>
    {
        [SerializeField] private Camera PlayerCamera;
        public IBaseSystem MainSystem { get; set; }
        
        public SelectionEvent EventType { get; private set; }
        //Need Camera Component
        
        private RaycastHit SingleHit;
        private readonly RaycastHit[] Hits = new RaycastHit[4]; //when mouse click we cast a ray

        private Ray SingleRay;
        private readonly Ray[] BoxRays = new Ray[4];
        
        //UI RECTANGLE
        private readonly Vector2[] UiCorners = new Vector2[4] {Vector2.down, Vector2.one, Vector2.up ,Vector2.right};

        private void Start()
        {
            
        }


        
    }
}