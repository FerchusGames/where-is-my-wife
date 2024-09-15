using System;
using UniRx;
using UnityEngine;

namespace WhereIsMyWife.Controllers
{
    public interface IPlayerControllerInput
    {
        IObservable<float> JumpStart { get; }
        IObservable<Unit> JumpEnd { get; }
        IObservable<float> Run { get; }
        IObservable<Vector2> DashStart { get; }
        IObservable<Unit> DashEnd { get; }
        IObservable<Unit> Turn { get; }
        IObservable<float> GravityScale { get; }
        IObservable<float> FallSpeedCap { get; }
    }
}