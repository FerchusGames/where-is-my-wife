using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Managers;
using WhereIsMyWife.Managers.Properties;
using WhereIsMyWife.Player.StateMachine;
using Zenject;

namespace WhereIsMyWife.Player.State
{
    public class PlayerWallJumpState : PlayerState, IWallJumpState, IWallJumpStateEvents
    {
        public PlayerWallJumpState() : base(PlayerStateMachine.PlayerState.WallJump) { }

        private Subject<float> _wallJumpVelocitySubject = new Subject<float>();
        private Subject<float> _gravityScaleSubject = new Subject<float>();
        private Subject<float> _fallSpeedCapSubject = new Subject<float>();
        
        public IObservable<float> WallJumpVelocity => _wallJumpVelocitySubject.AsObservable();
        public IObservable<float> GravityScale => _gravityScaleSubject.AsObservable();
        public IObservable<float> FallSpeedCap => _fallSpeedCapSubject.AsObservable();
        
        private IDisposable _gravityScaleSubscription;
        private IDisposable _fallSpeedCapSubscription;
        private IDisposable _landSubscription;
        private IDisposable _wallHangStartSubscription;
        
        [Inject] private IPlayerMovementProperties _movementProperties;
        [Inject] private IPlayerStateIndicator _stateIndicator;
        
        private Tween _horizontalSpeedTween;
        
        private float _horizontalSpeed = 0;
        private int _directionMultiplier = 1;
        private bool _isLookingRightAtStart;

        private float _timer = 0;
        private float _minWallJumpDuration = 0.2f;
        private float _wallJumpSpeed = 15f;
        private float _timeToNormalSpeed = 0.23f;
        private float _timeToZeroSpeed = 0.5f;

        private bool _minTimeHasPassed = false;
        
        protected override void SubscribeToObservables()
        {
            _gravityScaleSubscription = _playerStateInput.GravityScale.Subscribe(_gravityScaleSubject.OnNext);
            _fallSpeedCapSubscription = _playerStateInput.FallSpeedCap.Subscribe(_fallSpeedCapSubject.OnNext);
            _landSubscription = _playerStateInput.Land.Subscribe(EndWallJump);
            _wallHangStartSubscription = _playerStateInput.WallHangStart.Subscribe(WallHang);
        }

        protected override void UnsubscribeToObservables()
        {
            _gravityScaleSubscription?.Dispose();
            _fallSpeedCapSubscription?.Dispose();
            _landSubscription?.Dispose();
            _wallHangStartSubscription?.Dispose();
        }

        public override void EnterState()
        {
            base.EnterState();

            _timer = 0;
            _minTimeHasPassed = false;
            _isLookingRightAtStart = _stateIndicator.IsLookingRight;
            _directionMultiplier = _stateIndicator.IsLookingRight ? 1 : -1;
            StartJumpSpeedCurve();
        }

        public override void ExitState()
        {
            base.ExitState();
            _horizontalSpeedTween.Kill();
        }

        public override void UpdateState()
        {
            if (_stateIndicator.IsAccelerating && _minTimeHasPassed)
            {
                EndWallJump();
            }
        }

        public override void FixedUpdateState()
        {
            _timer += Time.fixedDeltaTime;
            
            _wallJumpVelocitySubject.OnNext(_horizontalSpeed);
        }

        private void StartJumpSpeedCurve()
        {
            _horizontalSpeed = _wallJumpSpeed * _directionMultiplier;

            _horizontalSpeedTween = DOTween.To(() => _horizontalSpeed, x => _horizontalSpeed = x, 
                    _movementProperties.RunMaxSpeed * _directionMultiplier, 
                    _timeToNormalSpeed)
                .SetEase(Ease.InOutSine)
                .OnComplete(StartDecelerationCurve);
        }
        
        private void StartDecelerationCurve()
        {
            _minTimeHasPassed = true;
            _horizontalSpeedTween = DOTween.To(() => _horizontalSpeed, x => _horizontalSpeed = x, 
                    0, 
                    _timeToZeroSpeed)
                .SetEase(Ease.InSine)
                .OnComplete(StartDecelerationCurve);
        }
        
        private void EndWallJump()
        {
            NextState = PlayerStateMachine.PlayerState.Movement;
        }

        private void WallHang()
        {
            NextState = PlayerStateMachine.PlayerState.WallHang;
        }
        
        private void Dash()
        {
            NextState = PlayerStateMachine.PlayerState.Dash;
        }
    }
}