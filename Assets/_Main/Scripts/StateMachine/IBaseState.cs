using System;
using UnityEngine;

public interface IBaseState<EState> where EState : Enum
{
    public void EnterState();
    public void ExitState();
    public void UpdateState();
    public void FixedUpdateState();
    public EState NextState { get;}
    public EState StateKey { get; }
    public void OnTriggerEnter2D(Collider2D other);
    public void OnTriggerStay2D(Collider2D other);
    public void OnTriggerExit2D(Collider2D other);
}