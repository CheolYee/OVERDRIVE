using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HpBar
{
    public class FillBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI fillText;

        [SerializeField] private bool displayAsInt;

        private const string TextFormat = "{0} / {1}";

        public void SetFill(float value, float maxValue)
        {
            fillImage.fillAmount = value / maxValue;

            fillText.SetText(displayAsInt
                ? string.Format(TextFormat, Mathf.RoundToInt(value), Mathf.RoundToInt(maxValue))
                : string.Format(TextFormat, value.ToString("F2"), maxValue.ToString("F2")));
        }
    }
}