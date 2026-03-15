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
        [field: SerializeField] public EventChannelSO SystemChannel { get; private set; }
        
        private bool _isBtnClicked;

        public override void Open(bool isTween = true)
        {
            base.Open(isTween);
            exitBtn.onClick.AddListener(OnExitClick);
        }

        private void OnExitClick()
        {
            if (_isBtnClicked)
                return;
            
            SystemChannel?.RaiseEvent(SystemEvents.SavePref);
            SceneManager.LoadScene(targetSceneName);
        }
    }
}