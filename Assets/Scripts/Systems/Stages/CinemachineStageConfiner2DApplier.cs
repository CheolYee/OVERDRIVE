using Unity.Cinemachine;
using UnityEngine;

namespace Systems.Stages
{
    public interface IStageCameraBoundaryApplier
    {
        void ApplyBoundary(Collider2D boundary);
    }

    public class CinemachineStageConfiner2DApplier : MonoBehaviour, IStageCameraBoundaryApplier
    {
        [SerializeField] private CinemachineConfiner2D confiner2D;

        public void ApplyBoundary(Collider2D boundary)
        {
            if (confiner2D == null || boundary == null)
                return;

            confiner2D.BoundingShape2D = boundary;
            confiner2D.InvalidateBoundingShapeCache();
            confiner2D.InvalidateLensCache();
        }

        private void Reset()
        {
            confiner2D = GetComponent<CinemachineConfiner2D>();
        }
    }
}