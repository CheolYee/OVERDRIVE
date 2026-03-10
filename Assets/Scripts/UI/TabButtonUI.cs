using System;
using DG.Tweening;
using Gamelib.SoundSystem;
using Systems.Database;
using Systems.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class TabButtonUI : MonoBehaviour
    {
        [field: SerializeField] public UIDataSo UIData { get; private set; }
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Image selectionImage;
        
        private Button _button;

        public event Action<UIDataSo> OnTabButtonClick;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleTabButtonClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleTabButtonClick);
        }

        private void HandleTabButtonClick()
        {
            OnTabButtonClick?.Invoke(UIData);
            SoundPlayManager.Instance.PlaySfx(SfxSounds.NORMAL_HIT, transform.position);
        }
        
        

        public void SetSelected(bool isSelected)
        {
            float sizeX = isSelected ? 1f : 0f;
            selectionImage.transform.DOComplete();
            selectionImage.transform.DOScaleX(sizeX, 0.2f).SetUpdate(true);
        }

        private void OnValidate()
        {
            if (UIData == null) return;
            gameObject.name = $"TabButtonUI - [{UIData.hierarchyName}]";
            
            if (buttonText == null) return;
                buttonText.SetText(UIData.displayName);
        }
    }
}