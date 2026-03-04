using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems
{
    [CreateAssetMenu(fileName = "Player Input", menuName = "SO/Core/PlayerInput", order = 5)]
    public class PlayerInputSO : ScriptableObject, Controls.IPlayerActions
    {
        public event Action OnJumpKeyPressed;
        public event Action OnAttackKeyPressed;
        public event Action<bool> OnDashKeyPressed;
        public event Action<bool> OnQKeyPressed;
        public event Action<bool> OnEKeyPressed;
        public event Action<bool> OnRKeyPressed;
        
        public Vector2 InputDirection { get; private set; }
        
        private Controls _controls;

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls.Player.Disable();
        }

        public void SetEnable(bool isEnable)
        {
            if (isEnable)
                OnEnable();
            else
                OnDisable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            InputDirection = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if(context.performed)
                OnJumpKeyPressed?.Invoke();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnDashKeyPressed?.Invoke(true);
            if (context.canceled)
                OnDashKeyPressed?.Invoke(false);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if(context.performed)
                OnAttackKeyPressed?.Invoke();
        }

        public void OnQKey(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnQKeyPressed?.Invoke(true);
            if (context.canceled)
                OnQKeyPressed?.Invoke(false);
        }

        public void OnEKey(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnEKeyPressed?.Invoke(true);
            if (context.canceled)
                OnEKeyPressed?.Invoke(false);
        }

        public void OnRKey(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnRKeyPressed?.Invoke(true);
            if (context.canceled)
                OnRKeyPressed?.Invoke(false);
        }
    }
}