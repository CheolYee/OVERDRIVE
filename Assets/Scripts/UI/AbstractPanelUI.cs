using System;
using DG.Tweening;
using Systems.Database;
using UnityEngine;

namespace UI
{
    public abstract class AbstractPanelUI : MonoBehaviour
    {
        [field: SerializeField] public UIDataSo UIData { get; protected set; }

        [SerializeField] protected float initHeight = 20f;
        [SerializeField] protected float hideHeight = -900f;
        [SerializeField] protected float transitionDuration = 0.3f;
        [SerializeField] protected RectTransform rectTrm;

        private void Start()
        {
            Close();
        }

        public virtual void Open(bool isTween = true)
        {
            if (isTween)
            {
                rectTrm.DOAnchorPosY(initHeight, transitionDuration).SetUpdate(true);
            }
            else
            {
                rectTrm.anchoredPosition = new Vector2(rectTrm.anchoredPosition.x, initHeight);
            }
        }

        public virtual void Close(bool isTween = true)
        {
            if (isTween)
            {
                rectTrm.DOAnchorPosY(hideHeight, transitionDuration).SetUpdate(true);
            }
            else
            {
                rectTrm.anchoredPosition = new Vector2(rectTrm.anchoredPosition.x, hideHeight);
            }
        }
        
        [ContextMenu("OpenPanel")]
        private void OpenPanelTest() => Open(false);
        
        [ContextMenu("Close Panel")]
        private void ClosePanelTest() => Close(false);
    }
}