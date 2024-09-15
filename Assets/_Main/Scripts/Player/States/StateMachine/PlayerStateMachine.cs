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
        [Inject] private IMovementState _movementState;
        [Inject] private IDashState _dashState;
        
        private void Awake()
        {
            States[PlayerState.Movement] = _movementState;
            States[PlayerState.Dash] = _dashState;
            
            CurrentState = States[PlayerState.Movement];
        }
    }
}
