using System;
using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.Physics;

namespace TowerDefense
{
    public class SelectionSubSystem : MonoBehaviour
    {
        //Design HauptSystem of the SubSystem
        [SerializeField] private InteractionSystem HauptSystem;
        //Normally retrieve from HauptSystem
        [SerializeField] private Camera PlayerCamera;
        
        //private SelectionInputController control;

        private RaycastHit singleHit;
        private readonly RaycastHit[] hits = new RaycastHit[4]; //when mouse click we cast a ray

        private Ray singleRay;
        private readonly Ray[] boxRays = new Ray[4];
        
        //UI RECTANGLE
        private readonly Vector2[] uiCorners = new Vector2[4] {Vector2.down, Vector2.one, Vector2.up ,Vector2.right};

        private const int unitLayer = 1 << 9;

        private void Awake()
        {
            HauptSystem ??= GetComponent<InteractionSystem>();
        }

        private void Start()
        {
            PlayerCamera = HauptSystem.PlayerCamera;
            
            HauptSystem.InputsControl.SelectionEvents.canceled += OnLeftClickRelease;
        }

        private void OnLeftClickRelease(InputAction.CallbackContext ctx)
        {
            singleRay = PlayerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            SphereCast(singleRay,0.5f, out singleHit, Mathf.Infinity, unitLayer);
            //select if not null, deselect otherwise
            HauptSystem.SelectionNotification(singleHit.transform);
        }

    }
}