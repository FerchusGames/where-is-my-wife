using System;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Managers.Properties;
using WhereIsMyWife.Player.State;
using WhereIsMyWife.Player.StateMachine;
using Zenject;

public class PlayerDashState : PlayerState, IDashState, IDashStateEvents
{
    public PlayerDashState() : base(PlayerStateMachine.PlayerState.Dash) { }

    private ISubject<Vector2> _dashSubject = new Subject<Vector2>();
    
    public IObservable<Vector2> Dash { get; }

    private IDisposable _dashSubscription;

    [Inject] private IPlayerDashProperties _properties;
    
    private float _timer;
    
    protected override void SubscribeToObservables()
    {
        _dashSubscription = _playerStateInput.DashStart.Subscribe(_dashSubject.OnNext);
    }

    protected override void UnsubscribeToObservables()
    {
        _dashSubscription?.Dispose();
    }

    public override void EnterState()
    {
        base.EnterState();
        _timer = 0;
    }

    public override void FixedUpdateState()
    {
        _timer += Time.fixedDeltaTime;

        if (_timer >= _properties.Duration)
        {
            NextState = PlayerStateMachine.PlayerState.Movement;
        }
    }

    public override void ExitState()
    {
    }
}
