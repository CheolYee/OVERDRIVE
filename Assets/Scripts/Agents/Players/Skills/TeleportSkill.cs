using Agents.FSM;
using Agents.Players.States;
using Systems.AnimationSystems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agents.Players.Skills
{
    public class TeleportSkill : AbstractPlayerSkill
    {
        [Header("Teleport Data")]
        [SerializeField] private ContactFilter2D targetLayer;
        [SerializeField] private float teleportOffset;
        [SerializeField] private int maxEnemyCount = 10;
        
        private Vector2 _teleportRange;
        private IMover _mover;
        private IRenderer _renderer;
        private IAnimatorTrigger _trigger;
        private Collider2D[] _results;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _results = new Collider2D[maxEnemyCount];
            _trigger = _player.GetModule<IAnimatorTrigger>();
            _mover = _player.GetModule<IMover>();
            _renderer = _player.GetModule<IRenderer>();
            Debug.Assert(_trigger != null, 
                $"{gameObject.name} 소유자가 애니메이션 트리거를 가지고 있지 않습니다.");
            Debug.Assert(_renderer != null, 
                $"{gameObject.name} 소유자가 렌더러를 가지고 있지 않습니다.");
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            AgentState current = _player.GetCurrentState();
            return NormalizedCooldown >= 1f && !IsAttacking
                                            && (current is ICanAttackState || current is PlayerAttackState
                                            || current is AbstractPlayerAirState);
        }

        protected override void OnSkillDataChanged()
        {
            base.OnSkillDataChanged();
            _teleportRange = Vector2.one * PlayerSkillData.maxRange;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);

            _mover.StopImmediately(true, true);
            _mover.SetGravityScale(0);
            _mover.CanManualMovement = false;
            _trigger.OnAnimationEnd += HandleAnimationEnd;
            _trigger.OnAttackTrigger += HandleAttackTrigger;
        }

        public override void StopSkill()
        {
            base.StopSkill();
            _player.IsSuperArmor = false;
            _mover.SetGravityScale(1);
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            _trigger.OnAttackTrigger -= HandleAttackTrigger;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
        }
        private void HandleAttackTrigger()
        {
            Teleport();
        }
        
        private void Teleport()
        {
            int count = Physics2D.OverlapBox(transform.position,
                _teleportRange, 0, targetLayer, _results);
            
            if (count <= 0)
                return;
            
            int rand = Random.Range(0, count);
            Collider2D target = _results[rand];
            
            if (target == null)
                return;
            
            Vector2 offset = new Vector2(-_renderer.FacingDirection * teleportOffset, 0f);
            _player.transform.position = (Vector2)target.transform.position + offset;
        }

        private void HandleAnimationEnd() => StopSkill();
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, _teleportRange);
        }

#endif
    }
}
