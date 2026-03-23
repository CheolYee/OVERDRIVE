using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Settings
{
    public class SettingPanel : AbstractPanelUI
    {
        [SerializeField] private Button exitBtn;
        [SerializeField] private string targetSceneName;
        [SerializeField] private float effectDuration = 0.3f;

        [field: SerializeField] public EventChannelSO SystemChannel { get; private set; }
        [field: SerializeField] public EventChannelSO UIChannel { get; private set; }

        private bool _isBtnClicked;

        public override void Open(bool isTween = true)
        {
            base.Open(isTween);

            _isBtnClicked = false;

            exitBtn.onClick.RemoveListener(OnExitClick);
            exitBtn.onClick.AddListener(OnExitClick);
            exitBtn.interactable = true;
        }

        public override void Close(bool isTween = true)
        {
            exitBtn.onClick.RemoveListener(OnExitClick);
            base.Close(isTween);
        }

        private void OnDestroy()
        {
            exitBtn.onClick.RemoveListener(OnExitClick);
        }

        private void OnExitClick()
        {
            if (_isBtnClicked)
                return;

            _isBtnClicked = true;
            exitBtn.interactable = false;

            if (UIChannel == null)
            {
                SystemChannel?.RaiseEvent(SystemEvents.SavePref);
                SceneManager.LoadScene(targetSceneName);
                return;
            }

            UIChannel.RaiseEvent(UIEvents.Fade.Init(false, effectDuration, () =>
            {
                SystemChannel?.RaiseEvent(SystemEvents.SavePref);
                SceneManager.LoadScene(targetSceneName);
            }));
        }
    }
}