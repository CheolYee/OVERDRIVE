using Agents;
using Agents.Enemies;
using CombatSystem;
using UnityEngine;

namespace Test
{
    public class EnemyFov2D : Agent
    {
        [Header("Target")]
        [SerializeField] private Transform player;

        [Header("View Settings")] [Min(0f)] 
        [SerializeField] private float viewDistance = 6f;

        [Range(0f, 180f)] 
        [SerializeField] private float viewAngle = 90f;

        [Header("Check")] 
        [Min(0.01f)] [SerializeField]
        private float checkInterval = 0.05f;
        
        protected AgentMover Mover {get; private set;}

        private float _timer;

        protected override void Awake()
        {
            base.Awake();
            Debug.Assert(player != null, "플레이어가 없습니다.");

            Mover = GetModule<AgentMover>();
        }

        private void Update()
        {
            if (player == null) return;

            _timer += Time.deltaTime;
            if (_timer < checkInterval) return;
            _timer = 0f;

            bool inFov = IsInFOV(player.position);

            if (inFov)
            {
                Debug.Log($"플레이어 감지! Enemy: {name}");
                float dir = player.position.x - transform.position.x;
                Mover.SetMovementX(dir);
                
            }
            else
            {
                Debug.Log($"플레이어 놓침. Enemy: {name}");
                Mover.StopImmediately(true, false);
            }
        }

        private bool IsInFOV(Vector3 targetWorldPos)
        {
            Vector2 enemyPos = transform.position;
            Vector2 toTarget = (Vector2)targetWorldPos - enemyPos;

            //거리 체크
            float distSq = toTarget.sqrMagnitude;
            float viewDistSq = viewDistance * viewDistance;
            if (distSq > viewDistSq) return false;

            //플레이어와 너무 겹치지 않았는지 예외처리
            if (distSq < 0.0001f) return true;

            //내적으로 각도 체크
            Vector2 forward = GetFacingDirection(); //적이 바라보는 방향
            Vector2 dir = toTarget.normalized;

            float dot = Vector2.Dot(forward, dir); // cos(theta)
            float halfAngle = viewAngle * 0.5f;
            float threshold = Mathf.Cos(halfAngle * Mathf.Deg2Rad);

            return dot >= threshold;
        }
        
        private Vector2 GetFacingDirection()
        {
            //localScale.x가 음수면 왼쪽을 보고 있다고 가정
            return transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Vector3 pos = transform.position;
            Gizmos.DrawWireSphere(pos, viewDistance);

            Vector2 forward = GetFacingDirection();
            float halfAngle = viewAngle * 0.5f;

            Vector2 leftDir = Rotate2D(forward, +halfAngle);
            Vector2 rightDir = Rotate2D(forward, -halfAngle);

            Gizmos.DrawLine(pos, pos + (Vector3)(leftDir * viewDistance));
            Gizmos.DrawLine(pos, pos + (Vector3)(rightDir * viewDistance));

            int segments = 20;
            Vector3 prev = pos + (Vector3)(rightDir * viewDistance);
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(-halfAngle, +halfAngle, t);
                Vector2 d = Rotate2D(forward, angle);
                Vector3 next = pos + (Vector3)(d * viewDistance);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        private static Vector2 Rotate2D(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            ).normalized;
        }
#endif
        protected override void HandleHealthChange(float before, float current, float max)
        {
            if (current <= 0 && !IsDead)
            {
                IsDead = true;
                onDeath?.Invoke();
            }    
        }
        
        public override void ApplyDamage(DamageData damageData, Vector2 hitPoint, Vector2 hitDirection, Vector2 hitNormal)
        {
            base.ApplyDamage(damageData, hitPoint, hitDirection, hitNormal);
            if (!IsSuperArmor && !IsDead)
            {
                Debug.Log("적이 피격당함!");
            }
        }
    }
}