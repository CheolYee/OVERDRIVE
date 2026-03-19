using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.Stages
{
    public class StageExitDoor : MonoBehaviour
    {
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private StageDoorAnimatorView doorView;

        private EventChannelSO _stageEventChannel;
        private int _runId = -1;
        private int _roomId = -1;
        private bool _isOpen;

        public void Bind(EventChannelSO stageEventChannel, int runId, int roomId)
        {
            _stageEventChannel = stageEventChannel;
            _runId = runId;
            _roomId = roomId;

            SetOpen(false);
        }

        public void ClearBinding()
        {
            _stageEventChannel = null;
            _runId = -1;
            _roomId = -1;
            _isOpen = false;
        }

        public void SetOpen(bool isOpen)
        {
            _isOpen = isOpen;

            if (triggerCollider != null)
                triggerCollider.enabled = isOpen;

            doorView?.SetOpen(isOpen);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isOpen || _stageEventChannel == null)
                return;

            if (((1 << other.gameObject.layer) & playerLayer.value) == 0)
                return;

            SetOpen(false);

            _stageEventChannel.RaiseEvent(
                StageEvents.NextRoomRequested.Init(_runId, _roomId));
        }

        private void OnValidate()
        {
            if (triggerCollider == null)
                triggerCollider = GetComponent<Collider2D>();
        }
    }
}