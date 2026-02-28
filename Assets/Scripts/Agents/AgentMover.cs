using System;
using Agents.StatSystem;
using Modules;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents
{
    public class AgentMover : MonoBehaviour, IModule, IMover, IAfterInitModule
    {
        [Header("Agent values")] 
        [SerializeField] private StatSO moveSpeedStat;
        [SerializeField] private LayerMask whatIsGround; //차후 센서시스템으로 변경한다.
        [SerializeField] private Vector2 groundCheckSize;

        private Agent _owner;
        private Rigidbody2D _rigidbody;
        private IRenderer _renderer;
        private IStatModule _statModule;

        private float _moveSpeed;
        private float _movementX;
        private float _moveSpeedMultiplier;
        private float _originalGravityScale;
        
        public bool IsGrounded { get; private set; }
        public event Action<bool> OnGroundStatusChange;
        public event Action<Vector2> OnVelocityChange;
        public bool CanManualMovement { get; set; } = true;
        public Rigidbody2D Rigidbody2D => _rigidbody;
        
        public void Initialize(ModuleOwner owner)
        {
            _owner = owner as Agent;
            _rigidbody = owner.GetComponent<Rigidbody2D>();
            _renderer = owner.GetModule<IRenderer>();
            _statModule = owner.GetModule<IStatModule>();
            
            _originalGravityScale = _rigidbody.gravityScale;
            _moveSpeedMultiplier = 1f;
        }
        
        public void AfterInit()
        {
            _moveSpeed = _statModule.SubscribeStat(moveSpeedStat.AssetIndex, HandleMoveSpeedChange, 1f);
        }

        private void OnDestroy()
        {
            if(_statModule != null)
                _statModule.UnSubscribeStat(moveSpeedStat.AssetIndex, HandleMoveSpeedChange);
        }

        private void HandleMoveSpeedChange(StatSO stat, float current, float previous) => _moveSpeed = current;

        public void SetMoveSpeedMultiplier(float value) => _moveSpeedMultiplier = value;
        public void SetGravityScale(float value) => _rigidbody.gravityScale = _originalGravityScale * value;

        public void AddForceToAgent(Vector2 force) =>
            _rigidbody.AddForce(force, ForceMode2D.Impulse);

        public void StopImmediately(bool xAxis, bool yAxis)
        {
            if (xAxis)
            {
                _rigidbody.linearVelocityX = 0;
                _movementX = 0;
            }

            if (yAxis)
            {
                _rigidbody.linearVelocityY = 0;
            }
        }

        public void SetMovementX(float value)
        {
            _movementX = Mathf.Clamp(value, -1f, 1f);
            _renderer?.FlipController(value);
        } 

        private void FixedUpdate()
        {
            CheckGround();
            MoveCharacter();
        }

        private void CheckGround()
        {
            bool before = IsGrounded;
            IsGrounded = Physics2D.OverlapBox(transform.position, groundCheckSize, 0, whatIsGround);
            
            if(before != IsGrounded)
                OnGroundStatusChange?.Invoke(IsGrounded);
        }

        private void MoveCharacter()
        {
            if (CanManualMovement)
            {
                _rigidbody.linearVelocityX = _movementX * _moveSpeed * _moveSpeedMultiplier;
            }
            
            OnVelocityChange?.Invoke(_rigidbody.linearVelocity);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, groundCheckSize);
        }

        
    }
}