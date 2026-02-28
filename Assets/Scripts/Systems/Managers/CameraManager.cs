using System;
using Gamelib.EventSystem;
using Systems.GameEvents;
using Unity.Cinemachine;
using UnityEngine;

namespace Systems.Managers
{
    public class CameraManager : MonoBehaviour
    {
        [field: SerializeField] public EventChannelSO CameraChannel { get; private set; }
        
        [SerializeField] private int activePriority = 15;
        [SerializeField] private int backgroundPriority = 5;
        [SerializeField] private CinemachineCamera currentCamera;
        
        private void Awake()
        {
            if (currentCamera == null)
            {
                GameObject startCamera = GameObject.FindGameObjectWithTag("FirstCam");
                if (startCamera != null)
                {
                    currentCamera = startCamera.GetComponent<CinemachineCamera>();
                }
            }
            
            Debug.Assert(currentCamera != null, $"시작 카메라는 반드시 셋팅되어야 합니다. {gameObject.name}");
            
            CameraChannel.AddListener<CameraSwapEvent>(HandleCameraSwap);
            ChangeCamera(currentCamera);
        }

        private void OnDestroy()
        {
            CameraChannel.RemoveListener<CameraSwapEvent>(HandleCameraSwap);
        }

        private void ChangeCamera(CinemachineCamera nextCamera)
        {
            Transform followTarget = null;
            if(currentCamera != null)
            {
                currentCamera.Priority = backgroundPriority;
                followTarget = currentCamera.Follow;
                currentCamera.Follow = null;
            }
            
            currentCamera = nextCamera;
            currentCamera.Priority = activePriority;
            currentCamera.Follow = followTarget;
        }

        private void HandleCameraSwap(CameraSwapEvent evt)
        {
            ChangeCamera(evt.NextCamera);
        }
    }
}