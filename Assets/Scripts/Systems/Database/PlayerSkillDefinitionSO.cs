using UnityEngine;

namespace Systems.Database
{
    [CreateAssetMenu(fileName = "Player Skill Definition", menuName = "Combat/Player Skill Definition", order = 14)]
    public class PlayerSkillDefinitionSO : IndexedAsset
    {
        public PlayerSkill skillId;
        public string displayName;
        public Sprite skillIcon;
        public SkillKey defaultKey;
    }
}