using UnityEngine;

namespace WhereIsMyWife.Managers.Properties
{
    public interface IPlayerCheckProperties
    {
        public Vector2 GroundCheckSize { get; }
        public LayerMask GroundLayer { get; }
    }

    [CreateAssetMenu(menuName = "ScriptableObjects/PlayerProperties/Modules/CheckProperties", 
        fileName = "CheckProperties")]
    public class PlayerCheckProperties : ScriptableObject, IPlayerCheckProperties
    {
        [field:SerializeField] public Vector2 GroundCheckSize { get; private set; }
        [field:SerializeField] public LayerMask GroundLayer { get; private set; }
    }
}
