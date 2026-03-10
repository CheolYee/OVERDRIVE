using System;
using System.Collections.Generic;
using Agents.FSM;
using Agents.Players.Skills;
using Agents.Players.States;
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
    public class PlayerSkillModule : MonoBehaviour, IModule, IPlayerSkillModule, IPlayerSkillRuntimeRegistry, IAfterInitModule
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
        [field: SerializeField] public PlayerSkillDataSo GroundBasicAttackSkill { get; private set; }
        [field: SerializeField] public PlayerSkillDataSo AirBasicAttackSkill { get; private set; }
        
        public AbstractPlayerSkill CurrentUsingSkill { get; private set; }
        
        private IPlayerDashLoadoutModule _dashLoadoutModule;
        
        public void Initialize(ModuleOwner owner)
        {
            Owner = owner;
            Player = owner as Player;
            Debug.Assert(Player != null, $"{gameObject.name} is not attached to player");
                        
            _renderer = Owner.GetModule<IRenderer>();
            _statModule = Owner.GetModule<IStatModule>();
            _dashLoadoutModule = Owner.GetModule<IPlayerDashLoadoutModule>();
            Debug.Assert(_dashLoadoutModule != null, $"{gameObject.name} is not attached to player");
            
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
            if (skillData == null)
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : skillData가 null 입니다.");
                return;
            }

            if (skillData.prefab == null)
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : {skillData.name} 의 prefab 이 비어 있습니다.");
                return;
            }

            if (_skillDict.ContainsKey(skillData.AssetIndex))
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : 이미 등록된 스킬입니다. AssetIndex = {skillData.AssetIndex}");
                return;
            }

            GameObject skillObject = Instantiate(skillData.prefab, transform);
            AbstractPlayerSkill skill = skillObject.GetComponent<AbstractPlayerSkill>();
            skill.InitializeSkill(this);

            _skillDict.Add(skillData.AssetIndex, skill);
            skill.BindingKey = bindKey;

            if (bindKey != SkillKey.NONE && bindKey != SkillKey.BASE_KEY)
            {
                if (!_keyBindDict.TryAdd(bindKey, skill))
                {
                    Debug.LogWarning($"{nameof(PlayerSkillModule)} : 이미 다른 스킬이 {bindKey} 에 바인딩되어 있습니다.");
                }
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
        
        private void HandleDashKeyPress(bool isPressed)
        {
            if (isPressed)
            {
                TryUseDashFromLoadout();
            }
            else
            {
                EndDashSequenceIfChargeable();
            }
        }
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
                    _dashLoadoutModule.ClearDashSkillStarted();

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
                    CurrentUsingSkill.StopSkill();
                }

                CurrentUsingSkill = skill;
                _renderer.PlayClip(skill.PlayerSkillData.animatorParam.ParamHash);
                skill.UseSkill(target);
            }
        }

        public bool TryUseBasicAttack()
        {
            AgentState currentState = Player.GetCurrentState();

            if (currentState is ICanAttackState)
            {
                return TryUseConfiguredSkill(GroundBasicAttackSkill);
            }

            if (currentState is AbstractPlayerAirState)
            {
                return TryUseConfiguredSkill(AirBasicAttackSkill);
            }

            return false;
        }
        
        private bool TryUseConfiguredSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null) return false;

            if (!CanUseSkill(skillData.AssetIndex))
                return false;

            _dashLoadoutModule.ClearDashSkillStarted();
            UseSkill(skillData.AssetIndex);
            Player.ChangeState(PlayerStateEnum.ATTACK);
            return true;
        }
        
        private bool TryUseDashSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return false;

            if (!_skillDict.TryGetValue(skillData.AssetIndex, out AbstractPlayerSkill skill))
                return false;

            if (!skill.CanUseSkill())
                return false;

            if (CurrentUsingSkill != null && CurrentUsingSkill.IsAttacking
                                          && (!CurrentUsingSkill.Cancelable || !skill.CanInterrupt))
                return false;

            CurrentUsingSkill?.StopSkill();
            Player.ChangeState(PlayerStateEnum.ATTACK);
            _dashLoadoutModule.MarkDashSkillStarted();

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

            return true;
        }
        private void EndDashSequenceIfChargeable()
        {
            if (!_dashLoadoutModule.CanReleaseDashCharge())
                return;

            if (CurrentUsingSkill is not IChargeableSkill chargeableSkill)
                return;

            _dashLoadoutModule.ClearDashSkillStarted();
            chargeableSkill.ChargeEnd();
        }
        
        private bool TryUseDashFromLoadout()
        {
            if (!_dashLoadoutModule.TryPeekNextDashSkill(out PlayerSkillDataSo skillData))
                return false;

            if (!TryUseDashSkill(skillData))
                return false;

            _dashLoadoutModule.AdvanceToNextDashSkill();
            return true;
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

        public bool IsSkillRegistered(PlayerSkillDataSo skillData)
        {
            if (skillData == null || _skillDict == null)
                return false;

            return _skillDict.ContainsKey(skillData.AssetIndex);
        }

        public bool TryRegisterSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return false;

            if (_skillDict == null)
                return false;

            if (_skillDict.ContainsKey(skillData.AssetIndex))
                return true;

            AddSkill(skillData);
            return _skillDict.ContainsKey(skillData.AssetIndex);
        }

        public bool TryReplaceSkill(PlayerSkillDataSo previousSkillData, PlayerSkillDataSo newSkillData)
        {
            throw new NotImplementedException();
        }
    }
}