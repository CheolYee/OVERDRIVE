using System;

namespace Systems.AnimationSystems
{
    public interface IAnimatorTrigger
    {
        event Action OnAnimationEnd;
        event Action OnAttackTrigger;
        
        event Action<bool> OnCounterStateChange;
    }
}