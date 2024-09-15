using UnityEngine;

namespace WhereIsMyWife.Managers.Properties
{
    public interface IPlayerDashProperties
    {
        public float Speed { get; }
        public float Duration { get; }
        public float HangTime { get; }
        public float Cooldown { get; }
    }
    
    [CreateAssetMenu(menuName = "ScriptableObjects/PlayerProperties/Modules/DashProperties", fileName = "DashProperties")]
    public class PlayerDashProperties : ScriptableObject, IPlayerDashProperties
    {
        [field:SerializeField] public float Speed { get; private set; }
        [field:SerializeField] public float Duration { get; private set; }
        [field:SerializeField] public float HangTime { get; private set; }
        [field:SerializeField] public float Cooldown { get; private set; }
    }

}