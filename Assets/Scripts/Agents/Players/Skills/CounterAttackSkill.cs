using System;
using System.Collections;
using Agents.FSM;
using CombatSystem;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class CounterAttackSkill : AbstractPlayerSkill
    {
        [SerializeField] private float counterDuration = 0.2f;
        [SerializeField] private float counterSuccessDuration = 0.35f;
        [SerializeField] private AnimParamSO counterSuccessParam;
        [SerializeField] private ContactFilter2D contactFilter;
        [SerializeField] private Transform counterSenserTrm;
        [SerializeField] private float counterRadius;
        
        [SerializeField] private Vector3 impulseForce;

        private Collider2D[] _hitResults;
        private IRenderer _renderer;
        private IMover _mover;
        private float _startTime;
        private bool _isCounterSuccess;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _hitResults = new Collider2D[10];
            _renderer = _player.GetModule<IRenderer>();
            _mover = _player.GetModule<IMover>();
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");
            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            return NormalizedCooldown >= 1f && !IsAttacking && _player.GetCurrentState() is ICanCounterState;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            _mover.StopImmediately(true, false);
            _startTime = Time.time;
            _isCounterSuccess = false;
            StartCoroutine(CounterProcessCoroutine());
        }

        private IEnumerator CounterProcessCoroutine()
        {
            while (Time.time - _startTime <= counterDuration && !_isCounterSuccess)
            {
                yield return null;

                if (SetCounterTargetInRadius(out ICounterable counterTarget) && !_isCounterSuccess)
                {
                    _isCounterSuccess = true;
                    _player.IsSuperArmor = true;
                    _renderer.PlayClip(counterSuccessParam.ParamHash);

                    Vector2 direction = _player.transform.right;
                    Vector2 knockBackDirection = SkillData.knockBackForce;
                    knockBackDirection.x *= Mathf.Sign(direction.x);
                    DamageData damageData = new DamageData
                    {
                        DamageAmount = _skillModule.GetBaseDamage(SkillData),
                        Dealer = _player,
                        DirectedKbForce = knockBackDirection,
                        IsCritical = false
                    };

                    Vector2 hitPoint = counterTarget.Collider.ClosestPoint(_player.transform.position);
                    
                    counterTarget.ApplyCounter(damageData, hitPoint, direction, -direction);
                    _skillModule.GenerateImpulse(impulseForce);
                    yield return new WaitForSeconds(counterSuccessDuration);
                    
                    _player.IsSuperArmor = false;
                    StopSkill();
                    yield break;
                }
            }
            
            StopSkill();
        }

        private bool SetCounterTargetInRadius(out ICounterable counterable)
        {
            Vector3 center = counterSenserTrm.position;
            int cnt = Physics2D.OverlapCircle(center, counterRadius, contactFilter, _hitResults);

            for (int i = 0; i < cnt; i++)
            {
                if (_hitResults[i].TryGetComponent(out counterable) && counterable.CanCounter) return true;
            }
            
            counterable = null;
            return false;
        }

        public override void StopSkill()
        {
            base.StopSkill();
            _skillModule.InvokeAttackEnd();
            StopAllCoroutines();
            _player.IsSuperArmor = false;
            _isCounterSuccess = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (counterSenserTrm != null)
                Gizmos.DrawWireSphere(counterSenserTrm.position, counterRadius);
        }
    }
}