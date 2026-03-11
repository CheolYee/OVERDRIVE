using System;
using System.Collections.Generic;
using System.Linq;
using Agents.StatSystem;
using CombatSystem;
using Modules;
using UnityEngine;

namespace Agents.Enemies.Skills
{
    public class EnemySkillModule : MonoBehaviour, IModule, ISkillModule, IAfterInitModule
    {
        public ModuleOwner Owner { get; private set; }
        public event Action OnAttackEnd;

        private Dictionary<int, AbstractEnemySkill> _skilDict;
        
        private IStatModule _statModule;
        private float _currentDamage = 1f;
        
        [field: SerializeField] public StatSO DamageStat { get; private set; }
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
            _currentDamage = _statModule.SubscribeStat(DamageStat.AssetIndex, HandleDamageChange, _currentDamage);
        }
        
        private void OnDestroy()
        {
            if (_statModule != null)
            {
                _statModule.UnSubscribeStat(DamageStat.AssetIndex, HandleDamageChange);
            }
        }

        #region 스텟 헨들러
        private void HandleDamageChange(StatSO stat, float current, float previous) => _currentDamage = current;
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
            if (skillData == null)
                return 0f;

            return _currentDamage * skillData.damageMultiplier;
        }
    }
}