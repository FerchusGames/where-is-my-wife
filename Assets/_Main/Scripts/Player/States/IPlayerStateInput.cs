using System;
using UniRx;
using UnityEngine;

namespace WhereIsMyWife.Player.State
{
    public interface IPlayerStateInput
    {
        IObservable<float> JumpStart { get; }
        IObservable<float> Run { get; }
        IObservable<Vector2> DashStart { get; }
        IObservable<float> GravityScale { get; }
        IObservable<float> FallSpeedCap { get; }
    }
}