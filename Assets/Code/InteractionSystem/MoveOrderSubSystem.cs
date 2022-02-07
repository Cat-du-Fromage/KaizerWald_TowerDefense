using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class MoveOrderSubSystem : MonoBehaviour
    {
        //Design HauptSystem of the SubSystem
        [SerializeField] private InteractionSystem HauptSystem;
        //Normally retrieve from HauptSystem
        [SerializeField] private Camera PlayerCamera;
        
        private void Awake()
        {
            HauptSystem ??= GetComponent<InteractionSystem>();
        }

        private void Start()
        {
            PlayerCamera = HauptSystem.PlayerCamera;
        }

        public void OnCancelRightClick()
        {
            //Get position on terrain
            
            //Set Position to Move
        }
    }
}
