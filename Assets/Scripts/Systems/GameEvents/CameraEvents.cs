using Gamelib.EventSystem;
using Unity.Cinemachine;

namespace Systems.GameEvents
{
    public static class CameraEvents
    {
        public static readonly CameraSwapEvent CameraSwap = new CameraSwapEvent();
    }

    public class CameraSwapEvent : GameEvent
    {
        public CinemachineCamera NextCamera { get; private set; }
        public CameraSwapEvent Init(CinemachineCamera nextCamera)
        {
            NextCamera = nextCamera;
            return this;
        }
    }
}