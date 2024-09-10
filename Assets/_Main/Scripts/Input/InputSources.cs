using System;
using UniRx;
using UnityEngine;

namespace WhereIsMyWife.Managers
{
    public interface IPlayerInputEvent
    {
        IObservable<Unit> JumpStartAction { get; }
        IObservable<Unit> JumpEndAction { get; }
        IObservable<float> RunAction { get; }
        IObservable<Vector2> DashAction { get; }
        IObservable<Vector2> UseItemAction { get; }
        IObservable<Unit> HookStartAction { get; }
        IObservable<Unit> HookEndAction { get; }
    }
    
    public enum ControllerType
    {
        Keyboard,
        Xbox,
        Playstation,
        Nintendo,
    }
}