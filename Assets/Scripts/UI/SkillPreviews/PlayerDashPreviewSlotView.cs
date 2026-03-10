using Agents.Players.Skills;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SkillPreviews
{
    public class PlayerDashPreviewSlotView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private Image skillIconImage;

        [Header("Motion")]
        [SerializeField] private float moveDistance = 24f;
        [SerializeField] private float moveDuration = 0.2f;
        [SerializeField] private float scaleDuration = 0.2f;

        private Vector2 _baseAnchoredPosition;

        private void Awake()
        {
            if (root == null)
                root = transform as RectTransform;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (root != null)
                _baseAnchoredPosition = root.anchoredPosition;
        }

        public void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }

        public void Bind(PlayerSkillDataSo skillData)
        {
            bool hasSkill = skillData != null;

            if (skillNameText != null)
                skillNameText.text = hasSkill ? skillData.attackName : "None";

            if (skillIconImage != null)
            {
                skillIconImage.sprite = hasSkill ? skillData.skillIcon : null;
                skillIconImage.enabled = hasSkill;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = hasSkill ? 1f : 0.35f;
        }

        public void PlayCurrentMotion()
        {
            if (root == null)
                return;

            root.DOKill();
            transform.DOKill();

            root.anchoredPosition = _baseAnchoredPosition;
            transform.localScale = Vector3.one * 0.9f;
            transform.DOScale(1f, scaleDuration).SetEase(Ease.OutBack);
        }

        public void PlayNextMotion()
        {
            if (root == null)
                return;

            root.DOKill();
            if (canvasGroup != null)
                canvasGroup.DOKill();

            root.anchoredPosition = _baseAnchoredPosition + Vector2.right * moveDistance;

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            root.DOAnchorPos(_baseAnchoredPosition, moveDuration).SetEase(Ease.OutCubic);

            if (canvasGroup != null)
                canvasGroup.DOFade(1f, moveDuration);
        }
    }
}