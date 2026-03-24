using System;
using System.Collections.Generic;
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
    public class PlayerSkillModule : MonoBehaviour, IModule, IPlayerSkillModule, IAfterInitModule
    {
        public ModuleOwner Owner { get; private set; }
        public AbstractPlayerSkill CurrentUsingSkill { get; private set; }
        
        private Player _player;

        public event Action OnAttackEnd;

        [field: SerializeField] public StatSO AttackSpeedStat { get; private set; }
        [field: SerializeField] public StatSO DamageStat { get; private set; }
        [field: SerializeField] public AnimParamSO AttackSpeedParam { get; private set; }

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
            _player = owner as Player;

            Debug.Assert(_player != null, $"{gameObject.name} is not attached to player");

            _renderer = Owner.GetModule<IRenderer>();
            _statModule = Owner.GetModule<IStatModule>();
            _dashLoadoutModule = Owner.GetModule<IPlayerDashLoadoutModule>();
            _basicAttackModule = Owner.GetModule<PlayerBasicAttackModule>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();

            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
            Debug.Assert(_dashLoadoutModule != null, $"{gameObject.name} is not attached to player");
            Debug.Assert(_basicAttackModule != null, $"{gameObject.name} has no PlayerBasicAttackModule component");
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
            if (_player?.PlayerInput == null)
                return;

            _player.PlayerInput.OnQKeyPressed += HandleQKeyPress;
            _player.PlayerInput.OnDashKeyPressed += HandleDashKeyPress;
            _player.PlayerInput.OnEKeyPressed += HandleEKeyPress;
            _player.PlayerInput.OnRKeyPressed += HandleRKeyPress;
        }

        private void UnregisterInputEvents()
        {
            if (_player?.PlayerInput == null)
                return;

            _player.PlayerInput.OnQKeyPressed -= HandleQKeyPress;
            _player.PlayerInput.OnDashKeyPressed -= HandleDashKeyPress;
            _player.PlayerInput.OnEKeyPressed -= HandleEKeyPress;
            _player.PlayerInput.OnRKeyPressed -= HandleRKeyPress;
        }

        private void AddSkill(PlayerSkillDataSo skillData, SkillKey bindKey = SkillKey.NONE)
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

            SkillKey resolvedBindKey = bindKey != SkillKey.NONE ? bindKey : skillData.defaultKey;
            skill.BindingKey = resolvedBindKey;

            _skillDict.Add(skillData.AssetIndex, skill);

            if (ShouldStoreKeyBinding(resolvedBindKey))
            {
                _keyBindDict[resolvedBindKey] = skill;
            }
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

            if (TryGetRegisteredSkill(skillData.skillId, out _, out _))
            {
                Debug.LogWarning($"{nameof(PlayerSkillModule)} : 이미 같은 skillId 스킬이 등록되어 있습니다. skillId = {skillData.skillId}");
                return false;
            }

            return true;
        }

        private bool TryGetRegisteredSkill(PlayerSkill skillId, out int registeredAssetIndex, out AbstractPlayerSkill registeredSkill)
        {
            registeredAssetIndex = -1;
            registeredSkill = null;

            foreach (KeyValuePair<int, AbstractPlayerSkill> pair in _skillDict)
            {
                AbstractPlayerSkill skill = pair.Value;
                if (skill == null || skill.PlayerSkillData == null)
                    continue;

                if (skill.PlayerSkillData.skillId != skillId)
                    continue;

                registeredAssetIndex = pair.Key;
                registeredSkill = skill;
                return true;
            }

            return false;
        }

        private void ApplyRegisteredSkillData(
            int registeredAssetIndex,
            AbstractPlayerSkill registeredSkill,
            PlayerSkillDataSo newSkillData,
            SkillKey bindKey)
        {
            if (registeredSkill == null || newSkillData == null)
                return;

            SkillKey previousBindKey = registeredSkill.BindingKey;

            if (ShouldStoreKeyBinding(previousBindKey) && previousBindKey != bindKey)
            {
                _keyBindDict.Remove(previousBindKey);
            }

            if (registeredAssetIndex != newSkillData.AssetIndex)
            {
                _skillDict.Remove(registeredAssetIndex);
                _skillDict[newSkillData.AssetIndex] = registeredSkill;
            }

            registeredSkill.SetSkillData(newSkillData);
            registeredSkill.BindingKey = bindKey;

            if (ShouldStoreKeyBinding(bindKey))
            {
                _keyBindDict[bindKey] = registeredSkill;
            }
        }

        private void HandleDashKeyPress(bool isPressed)
        {
            if (isPressed)
            {
                Debug.Log("일단 스킬 키 눌림");
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

        private void TryUseDashFromLoadout()
        {
            if (!_dashLoadoutModule.TryPeekNextDashSkill(out PlayerSkillDataSo skillData)) return;
            Debug.Log("스킬 찾는 중");

            if (!TryStartSkillByData(skillData, shouldMarkDashSequence: true)) return;
            Debug.Log("스킬 실행");

            _dashLoadoutModule.AdvanceToNextDashSkill();
            Debug.Log("다음 스킬로 변경됨");
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
            _player.ChangeState(PlayerStateEnum.ATTACK);

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

            if (TryGetRegisteredSkill(skillData.skillId, out int registeredAssetIndex, out AbstractPlayerSkill registeredSkill))
            {
                SkillKey resolvedBindKey = bindKey != SkillKey.NONE ? bindKey : registeredSkill.BindingKey;
                if (resolvedBindKey == SkillKey.NONE)
                    resolvedBindKey = skillData.defaultKey;

                ApplyRegisteredSkillData(registeredAssetIndex, registeredSkill, skillData, resolvedBindKey);
                return true;
            }

            AddSkill(skillData, bindKey);
            return _skillDict.ContainsKey(skillData.AssetIndex);
        }

        public bool ReplaceOwnedSkill(PlayerSkillDataSo oldSkillData, PlayerSkillDataSo newSkillData)
        {
            if (newSkillData == null)
                return false;

            if (TryGetRegisteredSkill(newSkillData.skillId, out int registeredAssetIndex, out AbstractPlayerSkill registeredSkill))
            {
                SkillKey bindKey = registeredSkill.BindingKey != SkillKey.NONE
                    ? registeredSkill.BindingKey
                    : newSkillData.defaultKey;

                ApplyRegisteredSkillData(registeredAssetIndex, registeredSkill, newSkillData, bindKey);
                return true;
            }

            return EnsureSkillRegistered(newSkillData, newSkillData.defaultKey);
        }

        public bool TryUseRegisteredSkill(PlayerSkillDataSo skillData, bool shouldMarkDashSequence = false)
        {
            return TryStartSkillByData(skillData, shouldMarkDashSequence);
        }
    }
}