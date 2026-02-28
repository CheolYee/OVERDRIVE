using Systems.Database;
using UnityEngine;

namespace CombatSystem
{
    public enum SkillType
    {
        PHYSICAL,
        MAGIC,
        NONE_DAMAGE
    }
    
    [CreateAssetMenu(fileName = "Skill data", menuName = "Combat/Skill data", order = 15)]
    public class SkillDataSO : IndexedAsset
    {
        public SkillType skillType;
        
        public float minRange;
        public float maxRange;
        public string attackName;
        public float damageMultiplier = 1f;
        public Vector2 knockBackForce;
        public float cooldown;
    }
}