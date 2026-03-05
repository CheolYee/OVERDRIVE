using Modules;
using UnityEngine;

namespace CombatSystem
{
    public abstract class AbstractDamageCaster : MonoBehaviour
    {
        public ModuleOwner Owner {get; private set;}

        [SerializeField] protected int maxHitCount = 5;
        [SerializeField] protected ContactFilter2D contactFilter;

        protected Collider2D[] HitResults;

        public virtual void InitCaster(ModuleOwner owner)
        {
            Owner = owner;
            HitResults = new Collider2D[maxHitCount];
        }

        public abstract bool CastDamage(float damage, Vector2 knockBackForce);
        public abstract bool CastDamage(float damage, Vector2 knockBackForce, Collider2D hitCollider);
        public abstract void SetRadius(float newRadius);
        public abstract void SetBoxSize(Vector2 newBoxSize);
    }
}