using Agents.FSM;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public abstract class AbstractPlayerState : AgentState
    {
        protected IMover _mover;
        protected Player _player;
        
        public AbstractPlayerState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
            _player = owner as Player;
            Debug.Assert(_player != null, $"{this} is not attached to player");
            _mover = owner.GetModule<IMover>();
        }
    }
}