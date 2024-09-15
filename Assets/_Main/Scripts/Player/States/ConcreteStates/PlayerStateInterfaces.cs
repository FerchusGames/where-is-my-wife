using WhereIsMyWife.Player.StateMachine;

public interface IMovementState : IBaseState<PlayerStateMachine.PlayerState> { }
public interface IDashState : IBaseState<PlayerStateMachine.PlayerState> { }
public interface IHookState : IBaseState<PlayerStateMachine.PlayerState> { }
public interface IWallHangState : IBaseState<PlayerStateMachine.PlayerState> { }
public interface IWallJumpState : IBaseState<PlayerStateMachine.PlayerState> { }
