using CombatSystem;
using Modules;
using UnityEngine;
using UnityEngine.Events;

namespace Agents
{
    public abstract class Agent : ModuleOwner, IDamageable
    {
        [field: SerializeField] public bool IsSuperArmor { get; set; }
        [field: SerializeField] public bool IsDead { get; set; }
        public AgentSensor Sensor { get; private set; }
        public HealthModule HealthModule { get; private set; }
        public ActionDataModule ActionData { get; private set; }

        public UnityEvent onHit;
        public UnityEvent onDeath;

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            Sensor = GetModule<AgentSensor>();
            HealthModule = GetModule<HealthModule>();
            ActionData = GetModule<ActionDataModule>();
            
            Debug.Assert(Sensor != null, $"{gameObject.name} 에 센서가 없습니다.");
            Debug.Assert(HealthModule != null, $"{gameObject.name} 에 체력모듈이 없습니다.");
            Debug.Assert(ActionData != null, $"{gameObject.name} 에 엑션데이터모듈이 없습니다.");
        }

        protected override void AfterInitComponents()
        {
            base.AfterInitComponents();
            HealthModule.OnHealthChange += HandleHealthChange;
        }

        protected virtual void OnDestroy()
        {
            HealthModule.OnHealthChange -= HandleHealthChange;
        }

        protected abstract void HandleHealthChange(float before, float current, float max);

        protected virtual void Start()
        {
            
        }

        public virtual void ApplyDamage(DamageData damageData, Vector2 hitPoint, Vector2 hitDirection, Vector2 hitNormal)
        {
            if (IsDead) return;
            
            ActionData.LastHitDirection = hitDirection;
            ActionData.LastHitPoint = hitPoint;
            ActionData.LastHitNormal = hitNormal;
            ActionData.LastKnockBackDirection = damageData.DirectedKbForce;
            HealthModule.ApplyDamage(damageData.DamageAmount);
            
            onHit?.Invoke();
        }
    }
}