using System;
using System.Collections.Generic;
using System.Linq;
using Agents.FSM;
using Agents.Players;
using Agents.Players.States;
using Agents.StatSystem;
using CombatSystem;
using Modules;
using Systems.Database;
using UnityEngine;

namespace Agents.Enemies.Skills
{
    public class EnemySkillModule : MonoBehaviour, IModule, ISkillModule, IAfterInitModule
    {
        public ModuleOwner Owner { get; private set; }
        public event Action OnAttackEnd;

        private Dictionary<int, AbstractEnemySkill> _skilDict;
        
        private IStatModule _statModule;
        private float _currentPhysicalDamage = 1f;
        private float _currentInt = 1f;
        private float _currentStr = 1f;
        
        [field: SerializeField] public StatSO PhysicalDamageStat { get; private set; }
        [field: SerializeField] public StatSO IntStat { get; private set; }
        [field: SerializeField] public StatSO StrStat { get; private set; }
        public void Initialize(ModuleOwner owner)
        {
            Owner = owner;
            _skilDict = GetComponentsInChildren<AbstractEnemySkill>()
                .ToDictionary(skill => skill.SkillData.AssetIndex);

            foreach (AbstractEnemySkill skill in _skilDict.Values)
            {
                skill.InitializeSkill(this);
            }

            _statModule = Owner.GetModule<IStatModule>();
        }
        
        public void AfterInit()
        {
            _currentPhysicalDamage = _statModule.SubscribeStat(PhysicalDamageStat.AssetIndex, HandleDamageChange, _currentPhysicalDamage);
            _currentInt = _statModule.SubscribeStat(IntStat.AssetIndex, HandleIntChange, _currentInt);
            _currentStr = _statModule.SubscribeStat(StrStat.AssetIndex, HandleStrChange, _currentStr);
        }
        
        private void OnDestroy()
        {
            if (_statModule != null)
            {
                _statModule.UnSubscribeStat(PhysicalDamageStat.AssetIndex, HandleDamageChange);
                _statModule.UnSubscribeStat(IntStat.AssetIndex, HandleIntChange);
                _statModule.UnSubscribeStat(StrStat.AssetIndex, HandleStrChange);
            }
        }

        #region 스텟 헨들러
        private void HandleDamageChange(StatSO stat, float current, float previous) => _currentPhysicalDamage = current;
        private void HandleIntChange(StatSO stat, float current, float previous) => _currentInt = current;
        private void HandleStrChange(StatSO stat, float current, float previous) => _currentStr = current;
        #endregion
        
        public bool CanUseSkill(int skillIndex, GameObject target = null)
        {
            if (_skilDict.TryGetValue(skillIndex, out AbstractEnemySkill skill))
            {
                return skill.CanUseSkill(target);
            }
            
            return false;
        }

        public void UseSkill(int skillIndex, GameObject target = null)
        {
            if (_skilDict.TryGetValue(skillIndex, out AbstractEnemySkill skill))
            {
                skill.UseSkill(target);
            }
        }

        public void InvokeAttackEnd()
        {
            OnAttackEnd?.Invoke();
        }

        public float GetBaseDamage(SkillDataSO skillData)
        {
            return skillData.skillType switch
            {
                SkillType.PHYSICAL => _currentPhysicalDamage * (1f + _currentStr / 100) * skillData.damageMultiplier,
                SkillType.MAGIC => _currentPhysicalDamage * (1f + _currentInt / 100) * skillData.damageMultiplier,
                SkillType.NONE_DAMAGE => 0,
                _ => 0
            };
        }
    }
}