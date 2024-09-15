using WhereIsMyWife.Player.State;
using Zenject;

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

        [Inject] private IBaseState<PlayerStateMachine.PlayerState> _movementState;
        
        private void Awake()
        {
            States[PlayerState.Movement] = _movementState;
            
            CurrentState = States[PlayerState.Movement];
        }
    }
}
