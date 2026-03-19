using Agents.Enemies.BT.Events;
using CombatSystem;
using Gamelib.ObjectPool.Runtime;
using Gamelib.SoundSystem;
using Systems.AnimationSystems;
using Systems.Managers;
using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies
{
    public abstract class AbstractEnemy : Agent, IPoolable
    {
        [field: SerializeField] public AttackConfigSO AttackConfig { get; private set; }
        [SerializeField] private PoolManagerSo poolManager;
        [field: SerializeField] public PoolItemSo PoolItem { get; set; }
        public BehaviorGraphAgent BtAgent { get; private set; }
        public IMover Mover { get; private set; }
        public IRenderer Renderer { get; private set; }
        public ISkillModule SkillModule { get; private set; }
        
        private BlackboardVariable<StateChannel> _stateChannel;
        
        public GameObject GameObject => gameObject;
        public void ResetItem()
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            IsDead = false;
            HealthModule.ResetHealth();
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            BtAgent = GetComponent<BehaviorGraphAgent>();
            Debug.Assert(BtAgent != null, $"{gameObject.name} is not attached to BTAgent");

            Mover = GetModule<IMover>();
            Renderer = GetModule<IRenderer>();
            SkillModule = GetModule<ISkillModule>();
            
            Debug.Assert(Mover != null, $"{gameObject.name} is not attached to mover");
            Debug.Assert(Renderer != null, $"{gameObject.name} is not attached to renderer");
            Debug.Assert(Renderer != null, $"{gameObject.name} is not attached to Skill Module");
        }
        protected override void Start()
        {
            base.Start();
            SetVariableValue(BtVar.Enemy, this);
            if (!GetVariableValue(BtVar.StateChannel, out _stateChannel))
            {
                Debug.LogError($"{gameObject.name} Blackboard variable {BtVar.StateChannel} not found");
            }
        }

        protected override void HandleHealthChange(float before, float current, float max)
        {
            if (current <= 0 && !IsDead)
            {
                _stateChannel.Value.SendEventMessage(EnemyState.DEAD);
                IsDead = true;
                onDeath?.Invoke();
            }
        }

        public override void ApplyDamage(DamageData damageData, Vector2 hitPoint, Vector2 hitDirection, Vector2 hitNormal)
        {
            base.ApplyDamage(damageData, hitPoint, hitDirection, hitNormal);
            if (!IsSuperArmor && !IsDead)
            {
                _stateChannel.Value.SendEventMessage(EnemyState.HIT);
            }
        }

        public void SetDead()
        {
            SoundPlayManager.Instance.PlaySfx(SfxSounds.ENEMY_DEAD, transform.position);
            gameObject.layer = LayerMask.NameToLayer("DeadBody");
            poolManager.Push(this);
        }

        public void SetVariableValue<T>(string variableName, T value)
        {
            Debug.Assert(!string.IsNullOrEmpty(variableName), $"Variable name is empty");

            if (BtAgent.GetVariable(variableName, out BlackboardVariable<T> variable))
            {
                variable.Value = value;
            }
            else
            {
                Debug.LogError($"Variable {variableName} not found");
            }
        }

        public bool GetVariableValue<T>(string variableName, out BlackboardVariable<T> variable)
        {
            Debug.Assert(!string.IsNullOrEmpty(variableName), "Variable name is empty");
            
            return BtAgent.GetVariable(variableName, out variable);
        }

        private void OnDrawGizmosSelected()
        {
            if (AttackConfig != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, AttackConfig.DetectRange);
            }
        }
    }
}