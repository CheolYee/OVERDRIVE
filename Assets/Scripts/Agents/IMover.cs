using System;
using UnityEngine;

namespace Agents
{
    public interface IMover
    {
        bool IsGrounded { get; }
        bool CanManualMovement { get; set; }
        Rigidbody2D Rigidbody2D { get; }
        event Action<bool> OnGroundStatusChange;
        event Action<Vector2> OnVelocityChange;
        void SetMoveSpeedMultiplier(float value);
        void SetGravityScale(float value);
        void AddForceToAgent(Vector2 force);
        void StopImmediately(bool xAxis, bool yAxis);
        void SetMovementX(float value);
    }
}