using System;
using UnityEngine;

namespace CombatSystem
{
    public class OverlapDamageCaster : AbstractDamageCaster
    {
        public enum CastType
        {
            CIRCLE,
            BOX
        }

        [SerializeField] private CastType castType;
        [SerializeField] private float radius;
        [SerializeField] private Vector2 boxSize;
        
        public void SetRadius(float newRadius) => radius = newRadius;
        public void SetBoxSize(Vector2 newBoxSize) => boxSize = newBoxSize;

        public override bool CastDamage(float damage, Vector2 knockBackForce)
        {
            int cnt = castType switch
            {
                CastType.CIRCLE => Physics2D.OverlapCircle(transform.position, radius, contactFilter, HitResults),
                CastType.BOX => Physics2D.OverlapBox(transform.position, boxSize, 0, contactFilter, HitResults),
                _ => 0
            };

            for (int i = 0; i < cnt; i++)
            {
                if (HitResults[i].TryGetComponent(out IDamageable damageable))
                {
                    Vector2 direction = Owner.transform.right;
                    Vector2 point = HitResults[i].ClosestPoint(transform.position);
                    knockBackForce.x *= Mathf.Sign(direction.x);

                    DamageData damageData = new DamageData
                    {
                        DamageAmount = damage,
                        Dealer = Owner,
                        DirectedKbForce = knockBackForce,
                    };
                    damageable.ApplyDamage(damageData, point, direction, -direction);
                }
            }

            return cnt > 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (castType == CastType.CIRCLE)
                Gizmos.DrawWireSphere(transform.position, radius);
            else if (castType == CastType.BOX)
                Gizmos.DrawWireCube(transform.position, boxSize);
        }
    }
}