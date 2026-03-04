using System;
using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using Gamelib.EventSystem;
using Systems;
using Systems.AnimationSystems;
using Systems.Database;
using Systems.GameEvents;
using UnityEngine;

namespace Agents.Players
{
    public class Player : Agent
    {
        [Header("Player values")] 
        [field: SerializeField] public float JumpForce { get; private set; } = 12f; //나중에 변경함
        [field: SerializeField] public int JumpCount { get; private set; } = 2; //나중에 변경함
        [field: SerializeField] public AnimParamSO YVelocityParam { get; private set; }
        [field: SerializeField] public EventChannelSO PlayerEventChannel { get; private set; }

        #region 임시 코드 영역
        
        [field: SerializeField] public AnimParamSO AttackSpeedParam { get; private set; }

        private ISkillModule _skillModule;
        
        #endregion
        
        [field: SerializeField] public PlayerInputSO PlayerInput { get; private set; }
        [SerializeField] private StateListSO stateList;
        
        
        private AgentStateMachine _stateMachine;
        private int _currentJumpCount;

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            
            _skillModule = GetModule<ISkillModule>(); //나중에 변경.
            _stateMachine = new AgentStateMachine(this, stateList.states);
        }

        protected override void AfterInitComponents()
        {
            base.AfterInitComponents();
            PlayerInput.OnJumpKeyPressed += HandleJumpKeyPressed;
            PlayerInput.OnAttackKeyPressed += HandleAttackKeyPressed;
            _currentJumpCount = JumpCount; //요건 나중에 삭제한다.
            
            PlayerEventChannel.AddListener<ActivePlayerEvent>(HandleActivePlayerEvent);
        }

        private void HandleActivePlayerEvent(ActivePlayerEvent obj)
        {
            if (obj.IsActive == false)
            {
                ChangeState(PlayerStateEnum.IDLE);
            }
            enabled = obj.IsActive;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            PlayerInput.OnJumpKeyPressed -= HandleJumpKeyPressed;
            PlayerInput.OnAttackKeyPressed -= HandleAttackKeyPressed;
            
            PlayerEventChannel.RemoveListener<ActivePlayerEvent>(HandleActivePlayerEvent);
        }

        protected override void HandleHealthChange(float before, float current, float max)
        {
        }

        protected override void Start()
        {
            base.Start();
            ChangeState(PlayerStateEnum.IDLE);
        }
        
        private void HandleAttackKeyPressed()
        {
            if (_stateMachine.CurrentState is ICanAttackState && _skillModule.CanUseSkill((int)PlayerSkill.NORMAL_COMBO))
            {
                _skillModule.UseSkill((int)PlayerSkill.NORMAL_COMBO);
                ChangeState(PlayerStateEnum.ATTACK);
            }
            else if (_stateMachine.CurrentState is AbstractPlayerAirState && _skillModule.CanUseSkill((int)PlayerSkill.JUMP_ATTACK))
            {
                _skillModule.UseSkill((int)PlayerSkill.JUMP_ATTACK);
                ChangeState(PlayerStateEnum.ATTACK);
            }
        }
        private void HandleJumpKeyPressed()
        {
            if (_stateMachine.CurrentState is ICanJumpState && _currentJumpCount > 0)
            {
                _currentJumpCount--;
                ChangeState(PlayerStateEnum.JUMP);
            }
        }
        
        public void ResetJumpCount() => _currentJumpCount = JumpCount;


        private void Update()
        {
            _stateMachine.UpdateMachine();
        }

        public void ChangeState(PlayerStateEnum nextState) => _stateMachine.ChangeState((int)nextState);
        public AgentState GetCurrentState() => _stateMachine.CurrentState;
    }
}