using Systems.AnimationSystems;

namespace Agents.FSM
{
    public abstract class AgentState
    {
        protected Agent _owner;
        protected int _clipHash;
        protected bool _isTriggerCall;

        protected IRenderer _renderer;

        public AgentState(Agent owner, AnimParamSO stateParam)
        {
            _owner = owner;
            _clipHash = stateParam != null ? stateParam.ParamHash : 0;
            _renderer = owner.GetModule<IRenderer>();
        }
        
        public virtual void Update() {}

        public virtual void Enter()
        {
            _renderer.PlayClip(_clipHash);
            _isTriggerCall = false;
        }

        public virtual void Exit() { }
        
        public virtual void AnimationEndTrigger() => _isTriggerCall = true; 
    }
}