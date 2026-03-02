using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class DashSkill : AbstractPlayerSkill
    {
        private IMover _mover;
        private AbstractDamageCaster _damageCaster;
        
        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _mover = skillModule.Owner.GetModule<IMover>();
            
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");
            
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} is not attached to damage caster");
            _damageCaster.InitCaster(_player);
        }
        
        public override bool CanUseSkill(GameObject target = null)
        {
            AgentState current = _player.GetCurrentState();
            return NormalizedCooldown >= 1f && !IsAttacking
                                            && (current is ICanAttackState || current is PlayerAttackState);
        }
        
        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            _mover.StopImmediately(true, false);
        }
    }
}