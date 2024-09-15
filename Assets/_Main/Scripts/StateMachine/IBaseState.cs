using System;
using UnityEngine;

public interface IBaseState<EState> where EState : Enum
{
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public EState NextState { get;}
    public EState StateKey { get; }
    public abstract void OnTriggerEnter2D(Collider2D other);
    public abstract void OnTriggerStay2D(Collider2D other);
    public abstract void OnTriggerExit2D(Collider2D other);
}