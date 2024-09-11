using System;
using System.Collections.Generic;
using Zenject;

public abstract class StateManager<EState> : IInitializable, ITickable where EState : Enum
{
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    protected BaseState<EState> CurrentState;
    protected bool IsTransitioningState = false;
    
    public void Initialize()
    {
        CurrentState.EnterState();
    }

    public void Tick()
    {
        EState nextStateKey = CurrentState.GetNextState();

        if (!IsTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        }

        else if (!IsTransitioningState)
        {
            TransitionToState(nextStateKey);
        }
    }

    private void TransitionToState(EState nextStateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState();
        CurrentState = States[nextStateKey];
        CurrentState.EnterState();
        IsTransitioningState = false;
    }
}
