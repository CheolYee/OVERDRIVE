using System;
using Agents.StatSystem;
using Gamelib.SoundSystem;
using Modules;
using Systems.Managers;
using UnityEngine;

namespace CombatSystem
{
    public class HealthModule : MonoBehaviour, IModule, IAfterInitModule
    {
        [SerializeField] private StatSO maxHealthStat;
        
        public delegate void HealthChange(float before, float current, float max);
        public event HealthChange OnHealthChange;
        
        private float _currentHealth;
        private IStatModule _statModule;
        private ModuleOwner _owner;

        [field: SerializeField] public float MaxHealth { get; private set; } = 30f;

        public float CurrentHealth
        {
            get => _currentHealth;
            set
            {
                float before = _currentHealth;
                _currentHealth = Mathf.Clamp(value, 0f, MaxHealth);
                if (!Mathf.Approximately(before, _currentHealth))
                {
                    OnHealthChange?.Invoke(before, _currentHealth, MaxHealth);
                }
            }
        }
        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            _statModule = owner.GetModule<IStatModule>();
        }


        public void AfterInit()
        {
            if (_statModule != null)
            {
                MaxHealth = _statModule.SubscribeStat(maxHealthStat.AssetIndex, HandleMaxHealthChange, MaxHealth);
            }
            
            CurrentHealth = MaxHealth;
        }

        private void OnDestroy()
        {
            _statModule.UnSubscribeStat(maxHealthStat.AssetIndex, HandleMaxHealthChange);
        }

        private void HandleMaxHealthChange(StatSO stat, float current, float previous)
        {
            float healthDifference = current - previous;
            CurrentHealth += healthDifference;
            MaxHealth = current;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
        }
        
        public void ApplyDamage(float damage)
        {
            SoundPlayManager.Instance.PlaySfx(SfxSounds.NORMAL_HIT, transform.position);
            CurrentHealth -= damage;
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
        }
    }
}