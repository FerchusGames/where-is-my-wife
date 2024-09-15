using WhereIsMyWife.Player.StateMachine;

public interface IMovementState : IBaseState<PlayerStateMachine.PlayerState> { }
public interface IDashState : IBaseState<PlayerStateMachine.PlayerState> { }
public interface IHookState : IBaseState<PlayerStateMachine.PlayerState> { }
