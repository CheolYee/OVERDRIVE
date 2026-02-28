using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CombatSystem.FeedBacks
{
    public class FeedbackPlayer : MonoBehaviour
    { 
        private List<Feedback> _feedbacks;

        private void Awake()
        {
            _feedbacks = GetComponents<Feedback>().ToList();
        }
        
        public void PlayAllFeedbacks()
        {
            StopAllFeedbacks();
            _feedbacks.ForEach(f => f.PlayFeedback());
        }

        private void StopAllFeedbacks()
        {
            _feedbacks.ForEach(f => f.StopFeedback());
        }
    }
}