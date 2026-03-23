using Systems.AnimationSystems;
using UnityEngine;

namespace Systems.Stages
{
    public class StageDoorAnimatorView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimParamSO openParam;

        public void SetOpen(bool isOpen)
        {
            if (animator == null || openParam == null)
                return;

            animator.SetBool(openParam.ParamHash, isOpen);
        }

        private void Reset()
        {
            animator = GetComponent<Animator>();
        }
    }
}