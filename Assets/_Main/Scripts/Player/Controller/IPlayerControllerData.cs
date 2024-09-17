using UnityEngine;

namespace WhereIsMyWife.Controllers
{
    public interface IPlayerControllerData
    {
        public Vector2 RigidbodyVelocity { get; }
        public Vector2 GroundCheckPosition { get; }
        public Vector2 WallHangCheckUpPosition { get; }
        public Vector2 WallHangCheckDownPosition { get; }
        public float HorizontalScale { get; }
    }
}