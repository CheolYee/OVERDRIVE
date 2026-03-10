using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Systems.Database;
using UnityEngine;

namespace UI
{
    public class MenuCanvasUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float transitionTime = 0.3f;
        [SerializeField] private UIDataSo defaultPanelData;

        public UIWindowStatus WindowStatus = UIWindowStatus.CLOSED;

        private Dictionary<int, AbstractPanelUI> _panelDict;
        
        private AbstractPanelUI _currentPanel;
        private Dictionary<int, TabButtonUI> _tabButtonDict;
        private TabButtonUI _currentTabButton;

        private void Awake()
        {
            _panelDict = GetComponentsInChildren<AbstractPanelUI>().ToDictionary(panel => panel.UIData.hashValue);
            _tabButtonDict = GetComponentsInChildren<TabButtonUI>()
                .ToDictionary(tabButton => tabButton.UIData.hashValue);
                
            foreach (TabButtonUI tabButton in _tabButtonDict.Values)
            {
                tabButton.OnTabButtonClick += HandleTabButtonClick;
                tabButton.SetSelected(false);
            }
        }

        private void OnDestroy()
        {
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
            if( _tabButtonDict.ContainsKey(hashValue))
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
 
        public void OpenWindow(int panelHash = 0)
        {
            WindowStatus = UIWindowStatus.OPENING;
            Time.timeScale = 0f;
            SetWindow(true, true, () => WindowStatus = UIWindowStatus.OPENED);
            TabClick(panelHash == 0 ? defaultPanelData.hashValue : panelHash);
        }

        public void CloseWindow()
        {
            WindowStatus = UIWindowStatus.CLOSING;
            SetWindow(false, true, () =>
            {
                WindowStatus = UIWindowStatus.CLOSED;
                _currentPanel?.Close();
                _currentPanel = null;
                _currentTabButton?.SetSelected(false);
                _currentTabButton = null;
                Time.timeScale = 1f;
            });
        }

        private void SetWindow(bool isOpen, bool isTween, Action endCallback = null)
        {
            float alpha = isOpen ? 1f : 0;
            if (isTween)
            {
                canvasGroup.DOFade(alpha, transitionTime)
                    .SetUpdate(true)
                    .OnComplete(() => endCallback?.Invoke());
            }
            else
            {
                canvasGroup.alpha = alpha;
            }
            canvasGroup.blocksRaycasts = isOpen;
            canvasGroup.interactable = isOpen;
        }
        
        [ContextMenu("OpenWindow")]
        private void OpenWindowTest() => SetWindow(true, false);
        
        [ContextMenu("CloseWindow")]
        private void CloseWindowTest() => SetWindow(false, false);
    }
}