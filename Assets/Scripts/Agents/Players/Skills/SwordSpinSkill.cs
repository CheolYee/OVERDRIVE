using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using Gamelib.SoundSystem;
using Systems.AnimationSystems;
using Systems.Managers;
using Unity.Cinemachine;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class SwordSpinSkill : AbstractPlayerSkill
    {
        [Header("SoundEffect")]
        [SerializeField] private SfxSounds sfx;
        
        [Header("Impulse")]
        [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
        [SerializeField] private Vector3 impulseForce;
        
        private IMover _mover;
        private IAnimatorTrigger _trigger;
        private AbstractDamageCaster _damageCaster;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _trigger = _player.GetModule<IAnimatorTrigger>();
            _mover = _player.GetModule<IMover>();
            Debug.Assert(_trigger != null, $"{gameObject.name} 소유자가 애니메이션 트리거를 가지고 있지 않습니다.");
            
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} 에 데미지 캐스터가 없습니다.");
            _damageCaster.InitCaster(_player);
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            AgentState current = _player.GetCurrentState();
            return NormalizedCooldown >= 1f && !IsAttacking
                                            && (current is ICanAttackState || current is PlayerAttackState
                                            || current is AbstractPlayerAirState);
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);

            _mover.StopImmediately(true, false);
            _mover.CanManualMovement = false;
            
            _trigger.OnAnimationEnd += HandleAnimationEnd;
            _trigger.OnAttackTrigger += HandleAttackTrigger;
        }

        public override void StopSkill()
        {
            base.StopSkill();
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            _trigger.OnAttackTrigger -= HandleAttackTrigger;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
        }
        private void HandleAttackTrigger()
        {
            cinemachineImpulseSource.GenerateImpulse(impulseForce);
            SoundPlayManager.Instance.PlaySfx(sfx, transform.position);
            _damageCaster.CastDamage(_skillModule.GetBaseDamage(SkillData), SkillData.knockBackForce);
        }

        private void HandleAnimationEnd() => StopSkill();
    }
}