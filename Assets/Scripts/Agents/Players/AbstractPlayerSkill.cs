using Agents.Players.Skills;
using CombatSystem;
using Systems.Database;
using UnityEngine;

namespace Agents.Players
{
    public abstract class AbstractPlayerSkill : MonoBehaviour, ISkill
    {
        protected float _lastUseTime;
        protected PlayerSkillModule _skillModule;
        protected Player _player;
        protected IPlayerDashLoadoutModule _dashLoadoutModule;

        [Header("Skill Info")]
        [field: SerializeField] public SkillKey BindingKey { get; set; }
        [field: SerializeField] public bool Cancelable { get; private set; } = false;
        [field: SerializeField] public bool CanInterrupt { get; private set; } = false;
        public bool IsAttacking { get; protected set; }

        [field: SerializeField] public SkillDataSO SkillData { get; private set; }
        public PlayerSkillDataSo PlayerSkillData { get; private set; }

        public float NormalizedCooldown => Mathf.Clamp01((Time.time - _lastUseTime) / SkillData.cooldown);

        public virtual void InitializeSkill(ISkillModule skillModule)
        {
            InitializeSkill(skillModule, SkillData as PlayerSkillDataSo);
        }

        public virtual void InitializeSkill(ISkillModule skillModule, PlayerSkillDataSo runtimeSkillData)
        {
            _skillModule = skillModule as PlayerSkillModule;
            Debug.Assert(_skillModule != null, $"{gameObject.name} is not attached to PlayerSkillModule");

            _player = skillModule.Owner as Player;
            Debug.Assert(_player != null, $"{gameObject.name} is not attached to player");

            ApplyRuntimeSkillData(runtimeSkillData);

            _dashLoadoutModule = skillModule.Owner.GetModule<IPlayerDashLoadoutModule>();
            Debug.Assert(_dashLoadoutModule != null, $"{gameObject.name} is not attached to PlayerDashLoadoutModule");
        }

        public void RefreshRuntimeSkillData(PlayerSkillDataSo runtimeSkillData)
        {
            ApplyRuntimeSkillData(runtimeSkillData);
        }

        protected void ApplyRuntimeSkillData(PlayerSkillDataSo runtimeSkillData)
        {
            PlayerSkillDataSo resolvedData = runtimeSkillData ?? SkillData as PlayerSkillDataSo;

            SkillData = resolvedData;
            PlayerSkillData = resolvedData;

            Debug.Assert(PlayerSkillData != null, $"{gameObject.name} runtime skill data is null");
        }

        public abstract bool CanUseSkill(GameObject target = null);

        public virtual void UseSkill(GameObject target = null)
        {
            IsAttacking = true;
        }

        public virtual void StopSkill()
        {
            ClearSkillParams();
        }

        protected virtual void ClearSkillParams()
        {
            IsAttacking = false;
        }
    }
}