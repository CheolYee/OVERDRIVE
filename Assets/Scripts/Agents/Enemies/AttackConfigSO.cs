using UnityEngine;

namespace Agents.Enemies
{
    [CreateAssetMenu(fileName = "Attack Config", menuName = "Enemy/Attack config", order = 10)]
    public class AttackConfigSO : ScriptableObject
    {
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float DetectRange { get; private set; }
        [field: SerializeField] public float StoppingDistance { get; private set; }
    }
}