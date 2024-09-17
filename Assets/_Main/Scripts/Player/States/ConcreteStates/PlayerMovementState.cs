using System;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Player.StateMachine;

namespace WhereIsMyWife.Player.State
{
    public class PlayerMovementState : PlayerState, IMovementState, IMovementStateEvents
    {
        public PlayerMovementState() : base(PlayerStateMachine.PlayerState.Movement) { }
        
        private Subject<float> _jumpStartSubject = new Subject<float>();
        private Subject<float> _runSubject = new Subject<float>();
        private Subject<float> _gravityScaleSubject = new Subject<float>();
        private Subject<float> _fallSpeedCapSubject = new Subject<float>();

        public IObservable<float> JumpStart => _jumpStartSubject.AsObservable();
        public IObservable<float> Run => _runSubject.AsObservable();
        public IObservable<float> GravityScale => _gravityScaleSubject.AsObservable();
        public IObservable<float> FallSpeedCap => _fallSpeedCapSubject.AsObservable();
        
        private IDisposable _jumpStartSubscription;
        private IDisposable _runSubscription;
        private IDisposable _gravityScaleSubscription;
        private IDisposable _fallSpeedCapSubscription;
        private IDisposable _wallHangStartSubscription;
        private IDisposable _dashStartSubscription;
        
        private float _runAcceleration;
        
        protected override void SubscribeToObservables()
        {
            _jumpStartSubscription = _playerStateInput.JumpStart.Subscribe(_jumpStartSubject.OnNext);
            _runSubscription = _playerStateInput.Run.Subscribe(_runSubject.OnNext);
            _gravityScaleSubscription = _playerStateInput.GravityScale.Subscribe(_gravityScaleSubject.OnNext);
            _fallSpeedCapSubscription = _playerStateInput.FallSpeedCap.Subscribe(_fallSpeedCapSubject.OnNext);
            _wallHangStartSubscription = _playerStateInput.WallHangStart.Subscribe(WallHang);
            _dashStartSubscription = _playerStateInput.DashStart.AsUnitObservable().Subscribe(Dash);
        }

        protected override void UnsubscribeToObservables()
        {
            _jumpStartSubscription?.Dispose();
            _runSubscription?.Dispose();
            _gravityScaleSubscription?.Dispose();
            _fallSpeedCapSubscription?.Dispose();
            _wallHangStartSubscription?.Dispose();
            _dashStartSubscription?.Dispose();
        }

        private void Dash()
        {
            NextState = PlayerStateMachine.PlayerState.Dash;
        }

        private void WallHang()
        {
            NextState = PlayerStateMachine.PlayerState.WallHang;
        }
    }
}