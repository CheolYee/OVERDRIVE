using UnityEngine;

namespace CombatSystem
{
    public class OverlapDamageCaster : AbstractDamageCaster
    {
        public enum CastType { CIRCLE, BOX }

        [SerializeField] private CastType castType;
        [SerializeField] private float radius = 0.6f;
        [SerializeField] private Vector2 boxSize = new Vector2(1.2f, 0.9f);

        public override void SetRadius(float newRadius) => radius = newRadius;
        public override void SetBoxSize(Vector2 newBoxSize) => boxSize = newBoxSize;

        public override bool CastDamage(float damage, Vector2 knockBackForce)
        {
            int cnt = castType switch
            {
                CastType.CIRCLE => Physics2D.OverlapCircle(transform.position, radius, contactFilter, HitResults),
                CastType.BOX => Physics2D.OverlapBox(transform.position, boxSize, 0, contactFilter, HitResults),
                _ => 0
            };

            if (cnt <= 0) return false;

            Vector2 direction = GetFacingDirection();
            bool anyHit = false;

            for (int i = 0; i < cnt; i++)
            {
                anyHit |= TryApplyDamage(HitResults[i], damage, knockBackForce, direction);
            }

            return anyHit;
        }

        //특정 콜라이더 단일 타격용
        public override bool CastDamage(float damage, Vector2 knockBackForce, Collider2D hitCollider)
        {
            if (hitCollider == null) return false;

            Vector2 direction = GetFacingDirection();
            return TryApplyDamage(hitCollider, damage, knockBackForce, direction);
        }

        private Vector2 GetFacingDirection()
        {
            float x = Owner != null ? Owner.transform.right.x : 1f;
            x = Mathf.Sign(Mathf.Abs(x) < 0.0001f ? 1f : x);
            return new Vector2(x, 0f);
        }

        private bool TryApplyDamage(Collider2D col, float damage, Vector2 knockBackForce, Vector2 direction)
        {
            if (col == null) return false;
            if (!col.TryGetComponent(out IDamageable damageable)) return false;

            Vector2 kb = knockBackForce;
            kb.x *= Mathf.Sign(direction.x);

            Vector2 point = col.ClosestPoint(transform.position);

            DamageData damageData = new DamageData
            {
                DamageAmount = damage,
                Dealer = Owner,
                DirectedKbForce = kb,
            };

            damageable.ApplyDamage(damageData, point, direction, -direction);
            return true;
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