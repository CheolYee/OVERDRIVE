using System;
using Coffee.UIExtensions;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace UI.Util
{
    public class BackGroundBlurPanel : MonoBehaviour
    {
        [SerializeField] private EventChannelSO uiChannel;
        [SerializeField] private UIEffectSnapshotPanel uiEffectSnapshotPanel;

        private void Awake()
        {
            uiChannel.AddListener<BlurPanelEvent>(SetBlurPanel);
        }

        private void OnDestroy()
        {
            uiChannel.RemoveListener<BlurPanelEvent>(SetBlurPanel);
        }

        private void SetBlurPanel(BlurPanelEvent evt)
        {
            if (evt.IsOpen)
                ShowBlurPanel(evt.OnBlurEnd);
            else
                HideBlurPanel(evt.OnBlurEnd);
        }

        private bool IsVisible { get; set; }

        private void ShowBlurPanel(Action endCallback = null)
        {
            if (uiEffectSnapshotPanel == null)
            {
                endCallback?.Invoke();
                return;
            }

            IsVisible = true;

            uiEffectSnapshotPanel.Show(() =>
            {
                Time.timeScale = 0f;
                endCallback?.Invoke();
            });
        }

        private void HideBlurPanel(Action endCallback = null)
        {
            if (uiEffectSnapshotPanel == null)
            {
                endCallback?.Invoke();
                return;
            }

            if (!IsVisible)
            {
                endCallback?.Invoke();
                return;
            }

            IsVisible = false;

            Time.timeScale = 1f;

            uiEffectSnapshotPanel.Hide(() =>
            {
                endCallback?.Invoke();
            });
        }
    }
}