using UnityEngine;

namespace Systems.Stages
{
    public class StageChestSpawnPoint : MonoBehaviour
    {
        [field: SerializeField] public bool isFlipChest;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
    }
}