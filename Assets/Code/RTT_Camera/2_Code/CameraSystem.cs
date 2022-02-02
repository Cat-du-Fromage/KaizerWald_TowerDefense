using System;
using KWUtils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTCamera
{
    public class CameraSystem : MonoBehaviour
    {
        [SerializeField]private CameraInputData cameraData;
        
        private Transform CameraTransform;
        private Controls CameraControls;
        
        private bool IsSprinting;
        private Vector2 MouseStartPosition, MouseEndPosition;
        private Vector2 MoveAxis;

        private float TargetYaw;
        private float TargetPitch;
        
        //INPUT ACTIONS
        private InputAction ZoomCameraAction => CameraControls.CameraControl.Zoom;
        private InputAction MoveAction => CameraControls.CameraControl.Mouvement;
        private InputAction RotationAction => CameraControls.CameraControl.Rotation;
        private InputAction SprintAction => CameraControls.CameraControl.Faster;
        
        //UPDATED MOVE SPEED
        private int MoveSpeed => IsSprinting ? cameraData.SprintSpeed : cameraData.baseMoveSpeed;
        private void Awake()
        {
            CameraControls ??= new Controls();
            CameraControls.Enable();
            
            CameraTransform = transform;
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
            //No was to get hold Pressed
            if (MoveAxis == Vector2.zero) return;
            MoveCamera(CameraTransform.position, CameraTransform.forward, CameraTransform.right);
        }

        private void MoveCamera(in Vector3 cameraPosition, in Vector3 cameraForward, in Vector3 cameraRight)
        {
            //real forward of the camera (aware of the rotation)
            Vector3 cameraForwardXZ = cameraForward.Flat();
            
            Vector3 xAxisRotation = MoveAxis.x != 0 ? (MoveAxis.x > 0 ? -cameraRight : cameraRight) : Vector3.zero;
            Vector3 zAxisRotation = MoveAxis.y != 0 ? (MoveAxis.y > 0 ? cameraForwardXZ : -cameraForwardXZ) : Vector3.zero;
            
            CameraTransform.position += (xAxisRotation + zAxisRotation) * (Mathf.Max(1f,cameraPosition.y) * MoveSpeed * Time.deltaTime);
        }
        
        private void SetCameraRotation()
        {
            if (MouseEndPosition != MouseStartPosition)
            {
                float distanceX = (MouseEndPosition - MouseStartPosition).x * cameraData.rotationSpeed;
                float distanceY = (MouseEndPosition - MouseStartPosition).y * cameraData.rotationSpeed;

                float deltaTime = Time.deltaTime;
                
                CameraTransform.Rotate(0f,distanceX * deltaTime, 0f, Space.World);
                
                CameraTransform.Rotate(-distanceY * deltaTime, 0f, 0f, Space.Self);
                /*
                float rotationX = CameraTransform.localEulerAngles.x;
                
                if (rotationX > 300f)
                    rotationX -= 360f;
                
                
                float test = ClampAngle(-distanceY * Time.deltaTime);
                Quaternion rotU = Quaternion.AngleAxis(test, Vector3.right);
                CameraTransform.rotation = CameraTransform.rotation * rotU;
                Debug.Log(rotU);
                */
                
                //CameraTransform.rotation = clamp(CameraTransform.rotation * rotU)
                
                //Debug.Log(degrees((CameraTransform.rotation * rotU).x));
                //CameraTransform.rotation = CameraTransform.rotation * rotU;
                    
                //Debug.Log(CameraTransform.rotation.x);
                
                MouseStartPosition = MouseEndPosition;
            }
        }
        
        public static float ClampAngle(float _Angle)
        {
            float ReturnAngle = _Angle;
 
            if (_Angle < 0f)
                ReturnAngle = (_Angle + (360f * ((_Angle / 360f) + 1)));
 
            else if (_Angle > 360f)
                ReturnAngle = (_Angle - (360f * (_Angle / 360f)));
 
            else if (ReturnAngle == 360) //Never use 360, only go from 0 to 359
                ReturnAngle = 0;
 
            return ReturnAngle;
        }
        
        public static float ComputeXAngle(Quaternion q)
        {
            float sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
            float cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
            return atan2(sinr_cosp, cosr_cosp);
        }
        //EVENTS CALLBACK
        //==============================================================================================================
        
        //Rotation
        //====================
        private void StartRotation(InputAction.CallbackContext ctx)
        {
            MouseStartPosition = ctx.ReadValue<Vector2>();
        }
        
        private void PerformRotation(InputAction.CallbackContext ctx)
        {
            MouseEndPosition = ctx.ReadValue<Vector2>();
            SetCameraRotation();
        }

        //Sprint
        //====================
        private void StartSprint(InputAction.CallbackContext ctx) => IsSprinting = true;
        private void CancelSprint(InputAction.CallbackContext ctx) => IsSprinting = false;

        //Move
        //====================
        private void PerformMove(InputAction.CallbackContext ctx) => MoveAxis = ctx.ReadValue<Vector2>();
        private void CancelMove(InputAction.CallbackContext ctx) => MoveAxis = Vector2.zero;

        //Zoom
        //====================
        private void PerformZoom(InputAction.CallbackContext ctx) => CameraTransform.position += Vector3.up * ctx.ReadValue<float>();
    }
}
