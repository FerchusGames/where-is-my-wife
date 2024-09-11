using System;
using UnityEngine;

public abstract class BaseState<EState> where EState : Enum
{
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract EState GetNextState();
    public EState StateKey { get; private set; }
    public abstract void OnTriggerEnter2D(Collider2D other);
    public abstract void OnTriggerStay2D(Collider2D other);
    public abstract void OnTriggerExit2D(Collider2D other);
    
    public BaseState(EState key)
    {
        StateKey = key;
    }
}
