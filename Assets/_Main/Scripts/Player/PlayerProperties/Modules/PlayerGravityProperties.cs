using UnityEngine;

namespace WhereIsMyWife.Managers.Properties
{
    public interface IPlayerGravityProperties
    {
        public float Scale { get; }
        public float MaxBaseFallSpeed { get; }
        public float MaxFastFallSpeed { get; }
        public float BaseFallMultiplier { get; }
        public float FastFallMultiplier { get; }
        public float JumpCutMultiplier { get; }
        public float JumpHangMultiplier { get; }
    }
    
    [CreateAssetMenu(menuName = "ScriptableObjects/PlayerProperties/Modules/GravityProperties", fileName = "GravityProperties")]
    public class PlayerGravityProperties : ScriptableObject, IPlayerGravityProperties
    {
        [field:SerializeField] public float Scale { get; private set; }
        [field:SerializeField] public float MaxBaseFallSpeed { get; private set; }
        [field:SerializeField] public float MaxFastFallSpeed { get; private set; }
        [field:SerializeField] public float BaseFallMultiplier { get; private set; }
        [field:SerializeField] public float FastFallMultiplier { get; private set; }
        [field:SerializeField] public float JumpCutMultiplier { get; private set; }
        [field:SerializeField] public float JumpHangMultiplier { get; private set; }
    }
}
