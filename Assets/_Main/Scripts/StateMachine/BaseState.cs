using System;

public abstract class BaseState<EState> where EState : Enum
{
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract EState GetNextState();
    public EState StateKey { get; private set; }
    
    public BaseState(EState key)
    {
        StateKey = key;
    }
}
