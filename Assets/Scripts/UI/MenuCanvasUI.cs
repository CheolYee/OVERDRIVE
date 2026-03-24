using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gamelib.EventSystem;
using Gamelib.SoundSystem;
using Systems.Database;
using Systems.GameEvents;
using Systems.Managers;
using UnityEngine;

namespace UI
{
    public class MenuCanvasUI : MonoBehaviour
    {
        [SerializeField] private EventChannelSO uiChannel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float transitionTime = 0.3f;
        [SerializeField] private UIDataSo defaultPanelData;

        public UIWindowStatus WindowStatus { get; private set; } = UIWindowStatus.CLOSED;

        private Dictionary<int, AbstractPanelUI> _panelDict;
        private Dictionary<int, TabButtonUI> _tabButtonDict;

        private AbstractPanelUI _currentPanel;
        private TabButtonUI _currentTabButton;
        private Tween _fadeTween;

        private void Awake()
        {
            _panelDict = GetComponentsInChildren<AbstractPanelUI>(true)
                .ToDictionary(panel => panel.UIData.hashValue);

            _tabButtonDict = GetComponentsInChildren<TabButtonUI>(true)
                .ToDictionary(tabButton => tabButton.UIData.hashValue);

            foreach (TabButtonUI tabButton in _tabButtonDict.Values)
            {
                tabButton.OnTabButtonClick += HandleTabButtonClick;
                tabButton.SetSelected(false);
            }

            SetWindowImmediate(false);
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();

            if (_tabButtonDict != null)
            {
                foreach (TabButtonUI tabButton in _tabButtonDict.Values)
                {
                    tabButton.OnTabButtonClick -= HandleTabButtonClick;
                }
            }
        }

        private void TabClick(int hashValue)
        {
            if (_tabButtonDict.ContainsKey(hashValue))
                HandleTabButtonClick(_tabButtonDict[hashValue].UIData);
        }

        private void HandleTabButtonClick(UIDataSo targetUIData)
        {
            _currentTabButton?.SetSelected(false);
            _currentTabButton = _tabButtonDict.GetValueOrDefault(targetUIData.hashValue);
            Debug.Assert(_currentTabButton != null, $"Tab button is null : {targetUIData.uiName}");

            _currentTabButton.SetSelected(true);
            OpenPanel(targetUIData.hashValue);
        }

        public void OpenPanel(int hashValue)
        {
            _currentPanel?.Close();
            _currentPanel = _panelDict.GetValueOrDefault(hashValue);
            Debug.Assert(_currentPanel != null, $"Panel is null : {hashValue}");
            _currentPanel.Open();
        }

        public void OpenWindow(int panelHash = 0, Action endCallback = null)
        {
            if (WindowStatus == UIWindowStatus.OPENED || WindowStatus == UIWindowStatus.OPENING)
                return;

            WindowStatus = UIWindowStatus.OPENING;

            SoundPlayManager.Instance.PlaySfx(SfxSounds.CARD_PANEL_OPEN, transform.position);

            uiChannel.RaiseEvent(UIEvents.BlurPanel.Init(true, () =>
            {
                SetWindow(true, true, () =>
                {
                    WindowStatus = UIWindowStatus.OPENED;
                    endCallback?.Invoke();
                });
            }));

            TabClick(panelHash == 0 ? defaultPanelData.hashValue : panelHash);
        }

        public void CloseWindow(Action endCallback = null)
        {
            if (WindowStatus != UIWindowStatus.OPENED)
                return;

            WindowStatus = UIWindowStatus.CLOSING;

            SetWindow(false, true, () =>
            {
                _currentPanel?.Close();
                _currentPanel = null;

                _currentTabButton?.SetSelected(false);
                _currentTabButton = null;

                uiChannel.RaiseEvent(UIEvents.BlurPanel.Init(false, () =>
                {
                    WindowStatus = UIWindowStatus.CLOSED;
                    endCallback?.Invoke();
                }));
            });
        }

        private void SetWindow(bool isOpen, bool isTween, Action endCallback = null)
        {
            float alpha = isOpen ? 1f : 0f;

            _fadeTween?.Kill();

            canvasGroup.blocksRaycasts = isOpen;
            canvasGroup.interactable = isOpen;

            if (isTween)
            {
                _fadeTween = canvasGroup.DOFade(alpha, transitionTime)
                    .SetUpdate(true)
                    .OnComplete(() => endCallback?.Invoke());
            }
            else
            {
                canvasGroup.alpha = alpha;
                endCallback?.Invoke();
            }
        }

        private void SetWindowImmediate(bool isOpen)
        {
            _fadeTween?.Kill();
            canvasGroup.alpha = isOpen ? 1f : 0f;
            canvasGroup.blocksRaycasts = isOpen;
            canvasGroup.interactable = isOpen;
        }

        [ContextMenu("OpenWindow")]
        private void OpenWindowTest() => SetWindow(true, false);

        [ContextMenu("CloseWindow")]
        private void CloseWindowTest() => SetWindow(false, false);
    }
}