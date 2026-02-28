using Agents.FSM;
using CombatSystem;
using Systems.AnimationSystems;
using Systems.Database;
using UnityEngine;

namespace Agents.Players.Skills
{
    [CreateAssetMenu(fileName = "Player Skill data", menuName = "Combat/Player Skill data", order = 15)]
    public class PlayerSkillDataSo : SkillDataSO
    {
        public AnimParamSO animatorParam;
        public PlayerStateEnum nextState;
        public SkillKey defaultKey;
        public GameObject prefab;
    }
}