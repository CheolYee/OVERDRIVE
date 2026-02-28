using UnityEngine;

namespace Agents.FSM
{
    [CreateAssetMenu(fileName = "FSM state manager", menuName = "FSM/State list", order = 10)]
    public class StateListSO : ScriptableObject
    {
        public string enumName;
        public StateSO[] states;
    }
}