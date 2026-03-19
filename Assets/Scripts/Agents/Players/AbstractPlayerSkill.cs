using Agents.Players.Skills;
using CombatSystem;
using Systems.Database;
using UnityEngine;

namespace Agents.Players
{
    public abstract class AbstractPlayerSkill : MonoBehaviour, ISkill, IRuntimeSkillDataSetter
    {
        protected float _lastUseTime;
        protected Player _player;
        protected PlayerSkillModule _skillModule;
        protected IPlayerDashLoadoutModule _dashLoadoutModule;

        [Header("Skill Info")]
        [field: SerializeField] public SkillKey BindingKey { get; set; }
        [field: SerializeField] public bool Cancelable { get; private set; } //다른스킬이 이거 캔슬 가능?
        [field: SerializeField] public bool CanInterrupt { get; private set; } //다른 스킬을 캔슬하고 이거 사용 가능?
        public bool IsAttacking { get; protected set; }

        [field: SerializeField] public SkillDataSO SkillData { get; private set; }
        public PlayerSkillDataSo PlayerSkillData { get; private set; }

        private bool _isInitialized;

        public float NormalizedCooldown
        {
            get
            {
                if (SkillData == null)
                    return 0f;

                float cooldown = Mathf.Max(0.0001f, SkillData.cooldown);
                return Mathf.Clamp01((Time.time - _lastUseTime) / cooldown);
            }
        }

        public void InitializeSkill(ISkillModule skillModule)
        {
            CacheSkillContext(skillModule);
            ApplySkillData(PlayerSkillData);

            if (_isInitialized == false)
            {
                _isInitialized = true;
                OnInitialized();
            }

            OnSkillDataChanged();
        }

        public void SetSkillData(SkillDataSO runtimeSkillData)
        {
            ApplySkillData(runtimeSkillData);
            OnSkillDataChanged();
        }

        private void CacheSkillContext(ISkillModule skillModule)
        {
            _skillModule = skillModule as PlayerSkillModule;
            Debug.Assert(_skillModule != null, $"{gameObject.name} is not attached to PlayerSkillModule");

            _player = skillModule.Owner as Player;
            Debug.Assert(_player != null, $"{gameObject.name} is not attached to Player");

            _dashLoadoutModule = skillModule.Owner.GetModule<IPlayerDashLoadoutModule>();
            Debug.Assert(_dashLoadoutModule != null, $"{gameObject.name} is not attached to PlayerDashLoadoutModule");
        }

        private void ApplySkillData(SkillDataSO runtimeSkillData)
        {
            SkillDataSO resolvedData = runtimeSkillData ?? SkillData;
            SkillData = resolvedData;

            PlayerSkillData = resolvedData as PlayerSkillDataSo;
            Debug.Assert(PlayerSkillData != null, $"{gameObject.name} runtime skill data is null");
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnSkillDataChanged()
        {
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