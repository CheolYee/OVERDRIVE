using UnityEngine;

namespace CombatSystem.FeedBacks
{
    public abstract class Feedback : MonoBehaviour
    {
        public abstract void PlayFeedback();
        public virtual void StopFeedback() {}
    }
}