using TMPro;
using UnityEngine;

namespace UI.SkillReward
{
    public class ChestSkillRewardPanelView : AbstractPanelUI
    {
        [SerializeField] private TextMeshProUGUI chestIdText;
        [SerializeField] private TextMeshProUGUI chestPointText;

        public void BindDebug(int chestInstanceId, Vector3 chestWorldPosition)
        {
            chestIdText.text = $"Chest ID: {chestInstanceId}";
            chestPointText.text = $"Chest Position: {chestWorldPosition}";
        }
}
}