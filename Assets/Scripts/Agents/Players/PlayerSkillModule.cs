﻿using System;
using System.Collections.Generic;
using System.Linq;
using Agents.FSM;
using Agents.Players.Skills;
using Agents.StatSystem;
using CombatSystem;
using Modules;
using Systems.AnimationSystems;
using Systems.Database;
using Unity.Cinemachine;
using UnityEngine;

namespace Agents.Players
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class PlayerSkillModule : MonoBehaviour, IModule, ISkillModule, IAfterInitModule
    {
        public ModuleOwner Owner { get; private set; }
        public Player Player { get; private set; }
        public event Action OnAttackEnd;

        private Dictionary<int, AbstractPlayerSkill> _skillDict;
        private Dictionary<SkillKey, AbstractPlayerSkill> _keyBindDict;
        private IRenderer _renderer;
        private IStatModule _statModule; //스탯기반해서 공속 조절
        private float _currentAttackSpeed;
        private float _currentPhysicalDamage = 1f;
        private float _currentInt = 1f;
        private float _currentStr = 1f;
        
        private CinemachineImpulseSource _impulseSource;
        
        [field: SerializeField] public StatSO AttackSpeedStat { get; private set; }
        [field: SerializeField] public StatSO PhysicalDamageStat { get; private set; }
        [field: SerializeField] public StatSO IntStat { get; private set; }
        [field: SerializeField] public StatSO StrStat { get; private set; }
        [field: SerializeField] public AnimParamSO AttackSpeedParam { get; private set; }

        [field: SerializeField] public PlayerSkillDataSo[] InitSkills { get; private set; }
        
        public AbstractPlayerSkill CurrentUsingSkill { get; private set; } = null;
        
        
        public void Initialize(ModuleOwner owner)
        {
            Owner = owner;
            Player = owner as Player;
            Debug.Assert(Player != null, $"{gameObject.name} is not attached to player");
                        
            _renderer = Owner.GetModule<IRenderer>();
            _statModule = Owner.GetModule<IStatModule>();
            
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            _skillDict = new Dictionary<int, AbstractPlayerSkill>();
            _keyBindDict = new Dictionary<SkillKey, AbstractPlayerSkill>();
            foreach (PlayerSkillDataSo skillData in InitSkills)
            {
                AddSkill(skillData, skillData.defaultKey);
            }
        }
        
        public void AfterInit()
        {
            _currentAttackSpeed = _statModule.SubscribeStat(AttackSpeedStat.AssetIndex, HandleAttackSpeedChange, 1f);
            _currentPhysicalDamage = _statModule.SubscribeStat(PhysicalDamageStat.AssetIndex, HandleDamageChange,
                _currentPhysicalDamage);
            _currentInt = _statModule.SubscribeStat(IntStat.AssetIndex, HandleIntChange, _currentInt);
            _currentStr = _statModule.SubscribeStat(StrStat.AssetIndex, HandleStrChange, _currentStr);
            
            _renderer.SetFloat(AttackSpeedParam, _currentAttackSpeed);

            Player.PlayerInput.OnQKeyPressed += HandleQKeyPress;
            Player.PlayerInput.OnDashKeyPressed += HandleDashKeyPress;
            Player.PlayerInput.OnEKeyPressed += HandleEKeyPress;
            Player.PlayerInput.OnRKeyPressed += HandleRKeyPress;
        }

        private void OnDestroy()
        {
            if (_statModule != null)
            {
                _statModule.UnSubscribeStat(AttackSpeedStat.AssetIndex, HandleAttackSpeedChange);
                _statModule.UnSubscribeStat(PhysicalDamageStat.AssetIndex, HandleDamageChange);
                _statModule.UnSubscribeStat(IntStat.AssetIndex, HandleIntChange);
                _statModule.UnSubscribeStat(StrStat.AssetIndex, HandleStrChange);
            }

            if (Player != null && Player.PlayerInput != null)
            {
                Player.PlayerInput.OnQKeyPressed -= HandleQKeyPress;
                Player.PlayerInput.OnDashKeyPressed -= HandleDashKeyPress;
                Player.PlayerInput.OnEKeyPressed -= HandleEKeyPress;
                Player.PlayerInput.OnRKeyPressed -= HandleRKeyPress;
            }
        }

        public void GenerateImpulse(Vector3 impulseVelocity) => _impulseSource.GenerateImpulse(impulseVelocity);

        #region 스킬 추가 및 삭제 로직
        
        public void AddSkill(PlayerSkillDataSo skillData, SkillKey bindKey = SkillKey.NONE)
        {
            GameObject skillObject = Instantiate(skillData.prefab, transform);
            AbstractPlayerSkill skill = skillObject.GetComponent<AbstractPlayerSkill>();
            skill.InitializeSkill(this);
            _skillDict.Add(skillData.AssetIndex, skill);
            skill.BindingKey = bindKey;

            if (bindKey != SkillKey.NONE && bindKey != SkillKey.BASE_KEY)
            {
                _keyBindDict.Add(bindKey, skill);
                //나중에 키바인드 변경시마다 이벤트 발행할 필요 있음.
            }
        }

        public void RemoveSkill(PlayerSkillDataSo skillData)
        {
            if (_skillDict.TryGetValue(skillData.AssetIndex, out AbstractPlayerSkill skill))
            {
                if(skill.BindingKey != SkillKey.NONE && skill.BindingKey != SkillKey.BASE_KEY)
                    _keyBindDict.Remove(skill.BindingKey);
                
                _skillDict.Remove(skillData.AssetIndex);
                Destroy(skill.gameObject);
            }
        }

        #endregion
        
        
        #region 스킬 키 바인딩 로직
        
        private void HandleDashKeyPress(bool isPressed) => SkillKeyPressed(SkillKey.DASH_KEY, isPressed);
        private void HandleQKeyPress(bool isPressed) => SkillKeyPressed(SkillKey.Q_KEY, isPressed);
        private void HandleEKeyPress(bool isPressed) => SkillKeyPressed(SkillKey.E_KEY, isPressed);
        private void HandleRKeyPress(bool isPressed) => SkillKeyPressed(SkillKey.R_KEY, isPressed);
        private void SkillKeyPressed(SkillKey key, bool isPressed)
        {
            if (_keyBindDict.TryGetValue(key, out AbstractPlayerSkill skill))
            {
                if (isPressed && skill.CanUseSkill())
                {
                    if (CurrentUsingSkill != null && CurrentUsingSkill.IsAttacking
                                                  && (!CurrentUsingSkill.Cancelable || !skill.CanInterrupt)) return;
                    CurrentUsingSkill?.StopSkill();
                    Player.ChangeState(PlayerStateEnum.ATTACK);
                    if (skill is IChargeableSkill chargeableSkill)
                    {
                        CurrentUsingSkill = skill;
                        _renderer.PlayClip(skill.PlayerSkillData.animatorParam.ParamHash);
                        chargeableSkill.ChargeStart();
                    }
                    else
                    {
                        UseSkill(skill.SkillData.AssetIndex);
                    }
                }
                else if (!isPressed && skill is IChargeableSkill chargeableSkill && CurrentUsingSkill == skill)
                {
                    chargeableSkill.ChargeEnd();
                }
                //현재 스킬이 존재하고 사용중일 때 검사해봐라. 
                //현재 스킬이 캔슬 가능하고, 지금 사용하려는 스킬이 인터럽터블 한지를 검사해서. 둘다 만족하지 않는다면 리턴.

                
            }
            else
            {
                Debug.Log($"<color=red>Can't use skill</color> Key = {key}");
            }
        }

        #endregion


        #region 스탯 이벤트 핸들러

        private void HandleDamageChange(StatSO stat, float current, float previous) => _currentPhysicalDamage = current;
        private void HandleIntChange(StatSO stat, float current, float previous)=> _currentInt = current;
        private void HandleStrChange(StatSO stat, float current, float previous)=> _currentStr = current;

        private void HandleAttackSpeedChange(StatSO stat, float current, float previous)
        {
            _currentAttackSpeed = current;
            _renderer.SetFloat(AttackSpeedParam, _currentAttackSpeed);
        }

        #endregion

        public bool CanUseSkill(int skillIndex, GameObject target = null)
        {
            if (_skillDict.TryGetValue(skillIndex, out AbstractPlayerSkill skill))
            {
                return skill.CanUseSkill(target);
            }

            return false;
        }

        public void UseSkill(int skillIndex, GameObject target = null)
        {
            if(_skillDict.TryGetValue(skillIndex, out AbstractPlayerSkill skill))
            {
                if (CurrentUsingSkill != null && CurrentUsingSkill.IsAttacking)
                {
                    //여기서 현재 스킬이 캔슬가능한지 따져봐야 해. 하지만 그건 다다음주에 한다.
                    CurrentUsingSkill.StopSkill(); //강제 종료
                }
                
                CurrentUsingSkill = skill;
                _renderer.PlayClip(skill.PlayerSkillData.animatorParam.ParamHash);
                skill.UseSkill(target);
            }
        }
        
        //OnAttackEnd 는 event이기 때문에 이 클래스에서만 실행이 가능하다. 근데 공격을 끝내는건 자식인 AbstractPlayerAttack이 끝낸다.
        //따라서 자식이 공격 끝냈음을 발행할 수 있도록 하는 매서드를 만들어야 한다.
        public void InvokeAttackEnd() => OnAttackEnd?.Invoke();

        public float GetBaseDamage(SkillDataSO skillData)
        {
            return skillData.skillType switch
            {
                SkillType.PHYSICAL => _currentPhysicalDamage * ( 1f + _currentStr / 100) * skillData.damageMultiplier,
                SkillType.MAGIC => _currentPhysicalDamage * ( 1f + _currentInt / 100) * skillData.damageMultiplier,
                SkillType.NONE_DAMAGE => 0,
                _ => 0
            };
        }
    }
}