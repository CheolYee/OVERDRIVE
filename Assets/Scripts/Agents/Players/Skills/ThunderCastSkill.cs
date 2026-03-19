using System.Collections;
using Agents.FSM;
using Agents.Players.States;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class ThunderCastSkill : AbstractPlayerSkill, IChargeableSkill
    {
        [SerializeField] private ParticleSystem chargingParticles;
        [SerializeField] private GameObject thunderStrikePrefab;
        [SerializeField] private int maxStrikeCount = 5;
        [SerializeField] private float durationPerStrike = 0.3f;
        [SerializeField] private float chargeDuration = 0.75f;
        [SerializeField] private float offsetValue = 1.5f;

        private int _currentChargingCount;
        private bool _isCharging;
        private float _chargingTime;
        private IMover _mover;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            _mover = _player.GetModule<IMover>();
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            AgentState currentState = _player.GetCurrentState();
            return NormalizedCooldown >= 1f && !IsAttacking
                                            && (currentState is ICanAttackState || currentState is PlayerAttackState);
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            StartCoroutine(GenerateThunderStrike());
        }

        private IEnumerator GenerateThunderStrike()
        {
            float offset = offsetValue * _player.transform.right.x; //전방 1.5위치부터 떨구기 시작
            for (int i = 0; i < _currentChargingCount; i++)
            {
                GameObject instance = Instantiate(thunderStrikePrefab, _player.transform.position, Quaternion.identity);
                ThunderStrike strike = instance.GetComponent<ThunderStrike>();
                Vector3 position = _player.transform.position + new Vector3(offset * (i + 1), 0); //전방으로 조금씩 앞으로
                float damage = _skillModule.GetBaseDamage(PlayerSkillData);
                strike.CastEffect(PlayerSkillData, position, damage * SkillData.damageMultiplier, _player);
                
                yield return new WaitForSeconds(durationPerStrike);
            }
            StopSkill();
        }

        public override void StopSkill()
        {
            base.StopSkill();
            StopAllCoroutines(); //이거 반드시 추가해야해.
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
        }

        public void ChargeStart()
        {
            _mover.StopImmediately(true, false);
            _currentChargingCount = 0;
            _isCharging = true;
            _chargingTime = 0;
            
            IsAttacking = true;
            chargingParticles?.Play();
        }

        public void ChargeEnd()
        {
            chargingParticles?.Stop();
            _isCharging = false;
            UseSkill();
        }

        public void ChargeCancel()
        {
            chargingParticles?.Stop();
            _isCharging = false;
            StopSkill();
        }

        private void Update()
        {
            if (!_isCharging) return;
            
            _chargingTime += Time.deltaTime;
            if (_chargingTime >= chargeDuration && _currentChargingCount < maxStrikeCount)
            {
                _currentChargingCount++;
                _chargingTime -= chargeDuration;
                Debug.Log($"Charge {_currentChargingCount}");
            }
        }
        
    }
}