using UnityEngine;

namespace CombatSystem
{
    public interface ICounterable
    {
        Collider2D Collider { get; }
        bool CanCounter { get; }
        void ApplyCounter(DamageData damageData, Vector2 hitPoint, Vector2 hitDirection, Vector2 hitNormal);
    }
}