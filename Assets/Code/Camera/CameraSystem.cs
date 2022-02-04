using System;
using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWaldCode.RTTCamera
{
    public class CameraSystem : MonoBehaviour
    {
        [SerializeField] private CameraData cameraData;
        private const float StartCameraAngle = 45;
        
        //CAMERA INFO
        private Transform CameraTransform;
        private Vector3 CameraPosition => CameraTransform.position;
        
        //MOVEMENT
        private Vector2 MouseStartPosition, MouseEndPosition;
        private Vector2 MoveAxis;

        //ROTATION
        public float CurrentVerticalAngle;
        
        //UPDATED MOVE SPEED
        private int MoveSpeed;

        //INPUT ACTIONS
        private Controls CameraControls;
        private InputAction ZoomCameraAction => CameraControls.CameraControl.Zoom;
        private InputAction MoveAction => CameraControls.CameraControl.Mouvement;
        private InputAction RotationAction => CameraControls.CameraControl.Rotation;
        private InputAction SprintAction => CameraControls.CameraControl.Faster;
        
        private void Awake()
        {
            CameraControls ??= new Controls();
            CameraControls.Enable();
            
            CameraTransform = transform;
            MoveSpeed = cameraData.BaseMoveSpeed;
            
            CameraTransform.rotation = Quaternion.identity;
            CameraTransform.Rotate(StartCameraAngle,0,0,Space.Self);
            CurrentVerticalAngle = -StartCameraAngle;
        }

        private void Start()
        {
            SprintAction.EnableStartCancelEvent(StartSprint, CancelSprint);
            MoveAction.EnablePerformCancelEvent(PerformMove, CancelMove);
            
            ZoomCameraAction.EnablePerformEvent(PerformZoom);
            RotationAction.EnableStartPerformEvent(StartRotation, PerformRotation);
        }

        private void OnDestroy()
        {
            SprintAction.DisableStartCancelEvent(StartSprint, CancelSprint);
            MoveAction.DisablePerformCancelEvent(PerformMove, CancelMove);
            
            ZoomCameraAction.DisablePerformEvent(PerformZoom);
            RotationAction.DisableStartPerformEvent(StartRotation, PerformRotation);
        }
        

        private void Update()
        {
            if (MoveAxis == Vector2.zero) return;
            MoveCamera(CameraTransform.right);
        }

        private void MoveCamera(in Vector3 cameraRight)
        {
            //real forward of the camera (aware of the rotation)
            Vector3 cameraForwardXZ = CameraTransform.forward.Flat();

            Vector3 xAxisMove = Vector3.zero;
            Vector3 zAxisMove = Vector3.zero;
            
            if (MoveAxis.x != 0)
                xAxisMove = MoveAxis.x > 0 ? -cameraRight : cameraRight;

            if (MoveAxis.y != 0)
                zAxisMove = MoveAxis.y > 0 ? cameraForwardXZ : -cameraForwardXZ;
            
            float heightMultiplier = Mathf.Max(1f, CameraPosition.y);
            CameraTransform.position += (xAxisMove + zAxisMove) * (MoveSpeed * heightMultiplier * Time.deltaTime);
        }
        
        private void CameraZoom(float deltaMouse)
        {
            float zoomValue = deltaMouse * cameraData.ZoomSpeed * Time.deltaTime;
            if (CameraPosition.y + zoomValue <= cameraData.MinZoom) return;
            
            CameraTransform.position += Vector3.up * (MathF.Log(CameraPosition.y) * zoomValue);
        }
        
        private void CameraRotation(in Vector2 currentMousePosition)
        {
            MouseEndPosition = currentMousePosition;
            
            //Distances mouse on camera view (XY because we got them from camera view!)
            //2D:X = 3D:Y
            //2D:Y = 3D:X
            float distanceX = (MouseEndPosition - MouseStartPosition).x * cameraData.RotationSpeed;
            float distanceY = (MouseEndPosition - MouseStartPosition).y * cameraData.RotationSpeed;
            
            //We keep track of the X axis rotation so we can clamp it! (quaternion make it difficult.. long story)
            float deltaVertical = distanceY * Time.deltaTime;
            CurrentVerticalAngle = Mathf.Clamp(CurrentVerticalAngle + deltaVertical, cameraData.MinRotation, cameraData.MaxRotation);
        
            //Get rotation from CurrentVerticalAngle on X axis
            Quaternion verticalRotation = Quaternion.AngleAxis(CurrentVerticalAngle, Vector3.right);
        
            //Get Angle from 0->CameraXRotation (Mathf.Sign(CurrentVerticalAngle) so we have negative value when we go down)
            float angleX = Quaternion.Angle(Quaternion.identity, verticalRotation) * Mathf.Sign(CurrentVerticalAngle);
        
            //Check if resulting Angle is still in bounds
            bool isInRange = angleX >= cameraData.MinRotation && angleX <= cameraData.MaxRotation;
            float angleToRotate = isInRange ? deltaVertical : 0.0f;
        
            //don't forget to Minus X value so we have the right angle
            CameraTransform.Rotate(-angleToRotate, 0f, 0f, Space.Self);
            CameraTransform.Rotate(0f, distanceX * Time.deltaTime, 0f, Space.World);
                
            MouseStartPosition = MouseEndPosition;
        }

//======================================================================================================================
//EVENTS CALLBACK
//======================================================================================================================
        
        //Rotation
        //====================
        private void StartRotation(InputAction.CallbackContext ctx) => MouseStartPosition = ctx.ReadValue<Vector2>();
        private void PerformRotation(InputAction.CallbackContext ctx) => CameraRotation(ctx.ReadValue<Vector2>());

        //Sprint
        //====================
        private void StartSprint(InputAction.CallbackContext ctx) => MoveSpeed = cameraData.SprintSpeed;
        private void CancelSprint(InputAction.CallbackContext ctx) => MoveSpeed = cameraData.BaseMoveSpeed;

        //Move
        //====================
        private void PerformMove(InputAction.CallbackContext ctx) => MoveAxis = ctx.ReadValue<Vector2>();
        private void CancelMove(InputAction.CallbackContext ctx) => MoveAxis = Vector2.zero;

        //Zoom
        //====================
        private void PerformZoom(InputAction.CallbackContext ctx) => CameraZoom(ctx.ReadValue<float>());
    }
}
