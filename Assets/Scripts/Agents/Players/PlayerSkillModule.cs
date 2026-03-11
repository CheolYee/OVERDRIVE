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
    public class PlayerSkillModule : MonoBehaviour, IModule, IPlayerSkillModule, IAfterInitModule
    {
        public ModuleOwner Owner { get; private set; }
        public Player Player { get; private set; }
        public AbstractPlayerSkill CurrentUsingSkill { get; private set; }

        public event Action OnAttackEnd;

        [field: SerializeField] public StatSO AttackSpeedStat { get; private set; }
        [field: SerializeField] public StatSO DamageStat { get; private set; }
        [field: SerializeField] public AnimParamSO AttackSpeedParam { get; private set; }

        [field: SerializeField] public PlayerSkillDataSo[] InitSkills { get; private set; }

        private readonly Dictionary<int, AbstractPlayerSkill> _skillDict = new();
        private readonly Dictionary<SkillKey, AbstractPlayerSkill> _keyBindDict = new();

        private IRenderer _renderer;
        private IStatModule _statModule;
        private IPlayerDashLoadoutModule _dashLoadoutModule;
        private CinemachineImpulseSource _impulseSource;
        private PlayerBasicAttackModule _basicAttackModule;

        private float _currentAttackSpeed = 1f;
        private float _currentDamage = 1f;

        public void Initialize(ModuleOwner owner)
        {
            Owner = owner;
            Player = owner as Player;

            Debug.Assert(Player != null, $"{gameObject.name} is not attached to player");

            _renderer = Owner.GetModule<IRenderer>();
            _statModule = Owner.GetModule<IStatModule>();
            _dashLoadoutModule = Owner.GetModule<IPlayerDashLoadoutModule>();
            _basicAttackModule = Owner.GetModule<PlayerBasicAttackModule>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();

            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
            Debug.Assert(_dashLoadoutModule != null, $"{gameObject.name} is not attached to player");
            Debug.Assert(_basicAttackModule != null, $"{gameObject.name} has no PlayerBasicAttackModule component");

            RegisterInitialSkills();
        }

        public void AfterInit()
        {
            SubscribeStats();
            RegisterInputEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeStats();
            UnregisterInputEvents();
        }

        public void GenerateImpulse(Vector3 impulseVelocity)
        {
            _impulseSource.GenerateImpulse(impulseVelocity);
        }

        private void RegisterInitialSkills()
        {
            if (InitSkills == null)
                return;

            foreach (PlayerSkillDataSo skillData in InitSkills)
            {
                EnsureSkillRegistered(skillData, skillData.defaultKey);
            }
        }

        private void SubscribeStats()
        {
            if (_statModule == null)
                return;

            _currentAttackSpeed = _statModule.SubscribeStat(
                AttackSpeedStat.AssetIndex,
                HandleAttackSpeedChange,
                1f);

            _currentDamage = _statModule.SubscribeStat(
                DamageStat.AssetIndex,
                HandleDamageChange,
                1f);

            _renderer.SetFloat(AttackSpeedParam, _currentAttackSpeed);
        }

        private void UnsubscribeStats()
        {
            if (_statModule == null)
                return;

            _statModule.UnSubscribeStat(AttackSpeedStat.AssetIndex, HandleAttackSpeedChange);
            _statModule.UnSubscribeStat(DamageStat.AssetIndex, HandleDamageChange);
        }

        private void RegisterInputEvents()
        {
            if (Player?.PlayerInput == null)
                return;

            Player.PlayerInput.OnQKeyPressed += HandleQKeyPress;
            Player.PlayerInput.OnDashKeyPressed += HandleDashKeyPress;
            Player.PlayerInput.OnEKeyPressed += HandleEKeyPress;
            Player.PlayerInput.OnRKeyPressed += HandleRKeyPress;
        }

        private void UnregisterInputEvents()
        {
            if (Player?.PlayerInput == null)
                return;

            Player.PlayerInput.OnQKeyPressed -= HandleQKeyPress;
            Player.PlayerInput.OnDashKeyPressed -= HandleDashKeyPress;
            Player.PlayerInput.OnEKeyPressed -= HandleEKeyPress;
            Player.PlayerInput.OnRKeyPressed -= HandleRKeyPress;
        }

        public void AddSkill(PlayerSkillDataSo skillData, SkillKey bindKey = SkillKey.NONE)
        {
            if (!CanCreateSkill(skillData))
                return;

            GameObject skillObject = Instantiate(skillData.prefab, transform);
            AbstractPlayerSkill skill = skillObject.GetComponent<AbstractPlayerSkill>();

            if (skill == null)
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : {skillData.name} prefab has no {nameof(AbstractPlayerSkill)}.");
                Destroy(skillObject);
                return;
            }

            skill.InitializeSkill(this);
            skill.BindingKey = bindKey;

            _skillDict.Add(skillData.AssetIndex, skill);

            if (ShouldStoreKeyBinding(bindKey) && !_keyBindDict.TryAdd(bindKey, skill))
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : 이미 다른 스킬이 {bindKey} 에 바인딩되어 있습니다.");
            }
        }

        public void RemoveSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return;

            if (!_skillDict.TryGetValue(skillData.AssetIndex, out AbstractPlayerSkill skill))
                return;

            if (ShouldStoreKeyBinding(skill.BindingKey))
            {
                _keyBindDict.Remove(skill.BindingKey);
            }

            if (CurrentUsingSkill == skill)
            {
                CurrentUsingSkill = null;
            }

            _skillDict.Remove(skillData.AssetIndex);
            Destroy(skill.gameObject);
        }

        private static bool ShouldStoreKeyBinding(SkillKey bindKey)
        {
            return bindKey != SkillKey.NONE && bindKey != SkillKey.BASE_KEY;
        }

        private bool CanCreateSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : skillData가 null 입니다.");
                return false;
            }

            if (skillData.prefab == null)
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : {skillData.name} 의 prefab 이 비어 있습니다.");
                return false;
            }

            if (_skillDict.ContainsKey(skillData.AssetIndex))
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : 이미 등록된 스킬입니다. AssetIndex = {skillData.AssetIndex}");
                return false;
            }

            return true;
        }

        private void HandleDashKeyPress(bool isPressed)
        {
            if (isPressed)
            {
                TryUseDashFromLoadout();
                return;
            }

            EndDashSequenceIfChargeable();
        }

        private void HandleQKeyPress(bool isPressed) => HandleBoundSkillKey(SkillKey.Q_KEY, isPressed);
        private void HandleEKeyPress(bool isPressed) => HandleBoundSkillKey(SkillKey.E_KEY, isPressed);
        private void HandleRKeyPress(bool isPressed) => HandleBoundSkillKey(SkillKey.R_KEY, isPressed);

        private void HandleBoundSkillKey(SkillKey key, bool isPressed)
        {
            if (!_keyBindDict.TryGetValue(key, out AbstractPlayerSkill skill))
            {
                Debug.Log($"<color=red>Can't use skill</color> Key = {key}");
                return;
            }

            if (isPressed)
            {
                TryStartTriggeredSkill(skill, shouldMarkDashSequence: false);
                return;
            }

            if (CurrentUsingSkill == skill && skill is IChargeableSkill chargeableSkill)
            {
                chargeableSkill.ChargeEnd();
            }
        }

        private bool TryUseDashFromLoadout()
        {
            if (!_dashLoadoutModule.TryPeekNextDashSkill(out PlayerSkillDataSo skillData))
                return false;

            if (!TryStartSkillByData(skillData, shouldMarkDashSequence: true))
                return false;

            _dashLoadoutModule.AdvanceToNextDashSkill();
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

        public bool TryUseBasicAttack()
        {
            return _basicAttackModule != null && _basicAttackModule.TryUseBasicAttack();
        }

        private bool TryStartSkillByData(PlayerSkillDataSo skillData, bool shouldMarkDashSequence)
        {
            if (skillData == null)
                return false;

            if (!_skillDict.TryGetValue(skillData.AssetIndex, out AbstractPlayerSkill skill))
                return false;

            return TryStartTriggeredSkill(skill, shouldMarkDashSequence);
        }

        private bool TryStartTriggeredSkill(AbstractPlayerSkill skill, bool shouldMarkDashSequence)
        {
            if (skill == null || !skill.CanUseSkill())
                return false;

            if (!CanInterruptCurrentSkill(skill))
                return false;

            StopCurrentSkillIfNeeded();
            EnterAttackState(shouldMarkDashSequence);

            if (skill is IChargeableSkill chargeableSkill)
            {
                CurrentUsingSkill = skill;
                PlaySkillAnimation(skill);
                chargeableSkill.ChargeStart();
                return true;
            }

            ExecuteSkill(skill);
            return true;
        }

        private bool CanInterruptCurrentSkill(AbstractPlayerSkill nextSkill)
        {
            if (CurrentUsingSkill == null || !CurrentUsingSkill.IsAttacking)
                return true;

            return CurrentUsingSkill.Cancelable && nextSkill.CanInterrupt;
        }

        private void StopCurrentSkillIfNeeded()
        {
            if (CurrentUsingSkill != null && CurrentUsingSkill.IsAttacking)
            {
                CurrentUsingSkill.StopSkill();
            }
        }

        private void EnterAttackState(bool shouldMarkDashSequence)
        {
            Player.ChangeState(PlayerStateEnum.ATTACK);

            if (shouldMarkDashSequence)
                _dashLoadoutModule.MarkDashSkillStarted();
            else
                _dashLoadoutModule.ClearDashSkillStarted();
        }

        private void ExecuteSkill(AbstractPlayerSkill skill)
        {
            CurrentUsingSkill = skill;
            PlaySkillAnimation(skill);
            skill.UseSkill();
        }

        private void PlaySkillAnimation(AbstractPlayerSkill skill)
        {
            _renderer.PlayClip(skill.PlayerSkillData.animatorParam.ParamHash);
        }

        private void HandleDamageChange(StatSO stat, float current, float previous)
        {
            _currentDamage = current;
        }

        private void HandleAttackSpeedChange(StatSO stat, float current, float previous)
        {
            _currentAttackSpeed = current;
            _renderer.SetFloat(AttackSpeedParam, _currentAttackSpeed);
        }

        public bool CanUseSkill(int skillIndex, GameObject target = null)
        {
            return _skillDict.TryGetValue(skillIndex, out AbstractPlayerSkill skill) && skill.CanUseSkill(target);
        }

        public void UseSkill(int skillIndex, GameObject target = null)
        {
            if (!_skillDict.TryGetValue(skillIndex, out AbstractPlayerSkill skill))
                return;

            StopCurrentSkillIfNeeded();
            CurrentUsingSkill = skill;
            PlaySkillAnimation(skill);
            skill.UseSkill(target);
        }

        public void InvokeAttackEnd()
        {
            OnAttackEnd?.Invoke();
        }

        public float GetBaseDamage(SkillDataSO skillData)
        {
            if (skillData == null)
                return 0f;

            return _currentDamage * skillData.damageMultiplier;
        }

        public bool EnsureSkillRegistered(PlayerSkillDataSo skillData, SkillKey bindKey = SkillKey.NONE)
        {
            if (skillData == null)
                return false;

            if (_skillDict.ContainsKey(skillData.AssetIndex))
                return true;

            AddSkill(skillData, bindKey);
            return _skillDict.ContainsKey(skillData.AssetIndex);
        }

        public bool ReplaceOwnedSkill(PlayerSkillDataSo oldSkillData, PlayerSkillDataSo newSkillData)
        {
            if (newSkillData == null)
                return false;

            SkillKey bindKey = newSkillData.defaultKey;

            if (oldSkillData != null && _skillDict.TryGetValue(oldSkillData.AssetIndex, out AbstractPlayerSkill oldSkill))
            {
                bindKey = oldSkill.BindingKey;
                RemoveSkill(oldSkillData);
            }

            return EnsureSkillRegistered(newSkillData, bindKey);
        }
        
        public bool TryUseRegisteredSkill(PlayerSkillDataSo skillData, bool shouldMarkDashSequence = false)
        {
            return TryStartSkillByData(skillData, shouldMarkDashSequence);
        }
    }
}