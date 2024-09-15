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
            WallHang,
            WallJump,
        }
        [Inject] private IMovementState _movementState;
        [Inject] private IDashState _dashState;
        [Inject] private IWallHangState _wallHangState;
        
        private void Awake()
        {
            States[PlayerState.Movement] = _movementState;
            States[PlayerState.Dash] = _dashState;
            States[PlayerState.WallHang] = _wallHangState;
            
            CurrentState = States[PlayerState.Movement];
        }
    }
}
