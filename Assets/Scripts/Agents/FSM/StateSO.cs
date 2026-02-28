using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.FSM
{
    [CreateAssetMenu(fileName = "State data", menuName = "FSM/State data", order = 0)]
    public class StateSO : ScriptableObject
    {
        public string stateName;
        public string className;
        public int stateIndex;
        public AnimParamSO stateParam;
    }
}