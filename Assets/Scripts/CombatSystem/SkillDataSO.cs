using Systems.Database;
using UnityEngine;

namespace CombatSystem
{
    [CreateAssetMenu(fileName = "Skill data", menuName = "Combat/Skill data", order = 15)]
    public class SkillDataSO : IndexedAsset
    {
        public float minRange;
        public float maxRange;
        public string attackName;
        public string idName;
        public float damageMultiplier = 1f;
        public Vector2 knockBackForce;
        public float cooldown;
    }
}