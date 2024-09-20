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
    public class PlayerWallHangState : PlayerState, IWallHangState, IWallHangStateEvents
    {
        public PlayerWallHangState() : base(PlayerStateMachine.PlayerState.WallHang) { }
        
        private Subject<Unit> _startWallHang = new Subject<Unit>();
        private Subject<float> _wallHangGravitySubject = new Subject<float>();
        private Subject<Unit> _turnSubject = new Subject<Unit>();
        private Subject<float> _wallJumpStartSubject = new Subject<float>();
        public IObservable<Unit> StartWallHang => _startWallHang.AsObservable();
        public IObservable<float> WallHangVelocity => _wallHangGravitySubject.AsObservable();
        public IObservable<float> WallJumpStart => _wallJumpStartSubject.AsObservable();
        public IObservable<Unit> Turn => _turnSubject.AsObservable();

        private IDisposable _dashStartSubscription;
        private IDisposable _landSubscription;
        private IDisposable _wallHangEndSubscription;
        private IDisposable _wallJumpStartSubscription;
        
        [Inject] private IPlayerMovementProperties _movementProperties;
        [Inject] private IPlayerStateIndicator _stateIndicator;
        
        private Tween _slideTween;
        
        private float _slideSpeed = 0;
        private float _fastSlideSpeed = 5f;
        private bool _isLookingRightAtStart;
        
        protected override void SubscribeToObservables()
        {
            _dashStartSubscription = _playerStateInput.DashStart.AsUnitObservable().Subscribe(Dash);
            _wallJumpStartSubscription = _playerStateInput.JumpStart.Subscribe(Jump);
            _landSubscription = _playerStateInput.Land.Subscribe(TurnAndCancelWallHang);
            _wallHangEndSubscription = _playerStateInput.WallHangEnd.Subscribe(TurnAndCancelWallHang);
        }

        protected override void UnsubscribeToObservables()
        {
            _dashStartSubscription?.Dispose();
            _wallJumpStartSubscription?.Dispose();
            _landSubscription?.Dispose();
            _wallHangEndSubscription?.Dispose();
        }

        public override void EnterState()
        {
            base.EnterState();
            
            _isLookingRightAtStart = _stateIndicator.IsLookingRight;
            _startWallHang.OnNext();
            
            StartSlideSpeedCurve();
        }

        private void StartSlideSpeedCurve()
        {
            _slideSpeed = 0;
            
            _slideTween = DOTween.To(() => _slideSpeed, x => _slideSpeed = x, 
                    -_movementProperties.WallSlideMaxVelocity, 
                    _movementProperties.WallSlideTimeToMaxVelocity)
                .SetEase(Ease.InOutSine);
        }

        public override void ExitState()
        {
            base.ExitState();

            KillSlideSpeedCurve();
        }

        private void KillSlideSpeedCurve()
        {
            if (_slideTween != null && _slideTween.IsActive())
            {
                _slideTween.Kill();
            }
        }

        public override void UpdateState()
        {
            if (PlayerIsGoingOppositeDirectionOfWall())
            {
                TurnAndCancelWallHang();
            }

            if (_stateIndicator.IsLookingDown)
            {
                _wallHangGravitySubject.OnNext(-_fastSlideSpeed);
            }
            else
            {
                _wallHangGravitySubject.OnNext(_slideSpeed);
            }
        }

        private bool PlayerIsGoingOppositeDirectionOfWall()
        {
            if (!_stateIndicator.IsAccelerating)
            {
                return false;
            }
            
            return _isLookingRightAtStart != _stateIndicator.IsRunningRight;
        }

        private void TurnAndCancelWallHang()
        {
            _turnSubject.OnNext();
            CancelWallHang();
        }

        private void CancelWallHang()
        {
            NextState = PlayerStateMachine.PlayerState.Movement;
        }

        private void Dash()
        {
            _turnSubject.OnNext();
            NextState = PlayerStateMachine.PlayerState.Dash;
        }

        private void Jump(float jumpForce)
        {
            _turnSubject.OnNext();
            _wallJumpStartSubject.OnNext(jumpForce / 1.5f);
            NextState = PlayerStateMachine.PlayerState.WallJump;
        }
    }
}