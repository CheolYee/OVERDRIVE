using Agents.Players.Skills;
using CombatSystem;
using Systems.Database;
using UnityEngine;

namespace Agents.Players
{
    public abstract class AbstractPlayerSkill : MonoBehaviour, ISkill
    {
        protected float _lastUseTime;
        protected PlayerSkillModule _skillModule; //이건 나중에 변경
        protected Player _player;
        
        [Header("Skill Info")]
        [field: SerializeField] public SkillKey BindingKey { get; set; }
        [field: SerializeField] public bool Cancelable { get; private set; } = false; //다른 공격이 이 공격 취소 여부
        [field: SerializeField] public bool CanInterrupt { get; private set; } = false; //다른공격 취소가능여부
        public bool IsAttacking { get; protected set; }
        
        [field: SerializeField] public SkillDataSO SkillData { get; private set; }
        public PlayerSkillDataSo PlayerSkillData { get; private set; }
        public float NormalizedCooldown => Mathf.Clamp01((Time.time - _lastUseTime) / SkillData.cooldown);
        
        public virtual void InitializeSkill(ISkillModule skillModule)
        {
            _skillModule = skillModule as PlayerSkillModule;
            Debug.Assert(_skillModule != null, $"{gameObject.name} is not attached to PlayerSkillModule");
            _player = skillModule.Owner as Player;
            Debug.Assert(_player != null, $"{gameObject.name} is not attached to player");
            PlayerSkillData = SkillData as PlayerSkillDataSo;
            Debug.Assert(PlayerSkillData != null, $"{SkillData.name} skill data is not PlayerSkillDataSo");
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