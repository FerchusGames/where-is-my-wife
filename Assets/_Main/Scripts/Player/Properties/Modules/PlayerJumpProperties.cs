using UnityEngine;

namespace WhereIsMyWife.Managers.Properties
{
    public interface IPlayerJumpProperties
    {
        public float ForceMagnitude { get; }
        public float CoyoteTime { get; }
        public float InputBufferTime { get; }
        public float HangTimeThreshold { get; }
        public float HangAccelerationMultiplier { get; }
        public float HangMaxSpeedMultiplier { get; }
    }

    [CreateAssetMenu(menuName = "ScriptableObjects/PlayerProperties/Modules/JumpProperties", 
        fileName = "JumpProperties")]
    public class PlayerJumpProperties : ScriptableObject, IPlayerJumpProperties
    {
        [field:SerializeField] public float ForceMagnitude { get; private set; }
        [field:SerializeField] public float CoyoteTime { get; private set; }
        [field:SerializeField] public float InputBufferTime { get; private set; }
        [field:SerializeField] public float HangTimeThreshold { get; private set; }
        [field:SerializeField] public float HangAccelerationMultiplier { get; private set; }
        [field:SerializeField] public float HangMaxSpeedMultiplier { get; private set; }
    }
}
