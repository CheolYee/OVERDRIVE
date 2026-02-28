using System;
using Gamelib.EventSystem;
using Systems;
using Systems.GameEvents;
using Unity.Cinemachine;
using UnityEngine;

namespace Environments
{
    public class CameraSwapTrigger : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera leftCamera;
        [SerializeField] private CinemachineCamera rightCamera;
        
        [SerializeField] private CinemachineCamera topCamera;
        [SerializeField] private CinemachineCamera bottomCamera;
        
        [field:SerializeField] public EventChannelSO CameraChannel { get; private set; }

        [SerializeField] private bool isHorizontalSwap = true; //가로 스왑이냐?

        private void Awake()
        {
            if(isHorizontalSwap && (leftCamera == null || rightCamera == null))
                Debug.LogError($"수평 스왑 설정이 올바르지 않습니다. {gameObject.name}");
            else if(!isHorizontalSwap && (topCamera == null || bottomCamera == null))
                Debug.LogError($"수직 스왑 설정이 올바르지 않습니다. {gameObject.name}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Vector2 exitDirection = (other.transform.position - transform.position).normalized;

                if (isHorizontalSwap)
                {
                    CameraChannel.RaiseEvent(CameraEvents.CameraSwap.Init(exitDirection.x > 0 ? rightCamera : leftCamera));
                }
                else
                {
                    CameraChannel.RaiseEvent(CameraEvents.CameraSwap.Init(exitDirection.y < 0 ? bottomCamera : topCamera));
                }
            }
        }
    }
}