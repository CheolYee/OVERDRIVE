using System;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace UI.SkillReward
{
    public class ChestSkillRewardModel : MonoBehaviour
    {
        [SerializeField] private EventChannelSO uiEventChannel;
        
        public event Action OnPendingRequestReceived; //보상 요청이 들어왔을 때 발생하는 이벤트
        
        public bool HasPendingRequest { get; private set; } //보상 요청이 대기 중인지 여부
        public int LastChestInstanceId { get; private set; } //마지막으로 요청된 보상 상자의 인스턴스 ID
        public Vector3 LastChestWorldPosition { get; private set; } //마지막으로 요청된 보상 상자의 월드 위치


        private void Awake()
        {
            Debug.Assert(uiEventChannel != null, "[ChestSkillRewardModel] : uiEventChannel != null");
        }

        private void OnEnable()
        {
            uiEventChannel.AddListener<OpenChestSkillRewardEvent>(HandleOpenChestSkillRewardEvent);
        }

        private void OnDisable()
        {
            uiEventChannel.RemoveListener<OpenChestSkillRewardEvent>(HandleOpenChestSkillRewardEvent);
        }

        private void HandleOpenChestSkillRewardEvent(OpenChestSkillRewardEvent obj)
        {
            HasPendingRequest = true;
            LastChestInstanceId = obj.ChestInstanceId;
            LastChestWorldPosition = obj.ChestWorldPosition;
            
            OnPendingRequestReceived?.Invoke();
        }
        
        public void ClearPendingRequest()
        {
            HasPendingRequest = false;
        }
    }
}