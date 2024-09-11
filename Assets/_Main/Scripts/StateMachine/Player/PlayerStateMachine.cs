using System;

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
        CurrentState = States[PlayerState.Movement];
    }
}
