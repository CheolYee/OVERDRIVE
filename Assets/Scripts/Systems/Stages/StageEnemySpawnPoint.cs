using UnityEngine;

namespace Systems.Stages
{
    public class StageEnemySpawnPoint : MonoBehaviour
    {
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
    }
}