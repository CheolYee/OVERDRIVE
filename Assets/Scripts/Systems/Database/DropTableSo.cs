using Agents.Players.Skills;
using UnityEngine;

namespace Systems.Database
{
    [CreateAssetMenu(fileName = "Drop Table", menuName = "Combat/Drop Table", order = 0)]
    public class DropTableSo : ScriptableObject
    {
        public PlayerSkillDataTableSo playerSkillDataTable;
    }
}