using WhereIsMyWife.Player.State;

namespace WhereIsMyWife.Player.StateMachine
{
    public class PlayerStateMachine : StateManager<PlayerStateMachine.PlayerState>
    {
        public enum PlayerState
        {
            Movement,
            Dash,
            Hook,
            Dead,
            Cinematic
        }

        private void Awake()
        {
            States[PlayerState.Movement] = new PlayerMovementState();
            
            CurrentState = States[PlayerState.Movement];
        }
    }
}
