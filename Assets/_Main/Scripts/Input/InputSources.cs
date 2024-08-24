using System;
using UniRx;
using UnityEngine;

namespace WhereIsMyWife.Managers
{
    public interface IPlayerInput
    {
        IObservable<Unit> JumpStartAction { get; }
        IObservable<float> RunAction { get; }
        IObservable<Vector2> DashStartAction { get; }
    }

}