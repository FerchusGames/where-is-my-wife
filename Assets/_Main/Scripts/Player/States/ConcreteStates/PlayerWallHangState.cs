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

        public IObservable<Unit> StartWallHang => _startWallHang.AsObservable();
        public IObservable<float> WallHangVelocity => _wallHangGravitySubject.AsObservable();
        public IObservable<Unit> Turn => _turnSubject.AsObservable();

        [Inject] private IPlayerMovementProperties _movementProperties;
        [Inject] private IPlayerStateIndicator _stateIndicator;
        
        private Tween _slideTween;
        
        private float _slideSpeed = 0;
        private bool _initialFacingDirection;
        
        protected override void SubscribeToObservables()
        {
            _playerStateInput.DashStart.AsUnitObservable().Subscribe(Dash);
            _playerStateInput.JumpStart.AsUnitObservable().Subscribe(Jump);
            _playerStateInput.Land.Subscribe(TurnAndCancelWallHang);
            _playerStateInput.WallHangEnd.Subscribe(CancelWallHang);
        }

        protected override void UnsubscribeToObservables()
        {
            
        }

        public override void EnterState()
        {
            base.EnterState();
            
            _initialFacingDirection = _stateIndicator.IsRunningRight;
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
            
            _wallHangGravitySubject.OnNext(_slideSpeed);
        }

        private bool PlayerIsGoingOppositeDirectionOfWall()
        {
            return _initialFacingDirection != _stateIndicator.IsRunningRight;
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

        private void Jump()
        {
            _turnSubject.OnNext();
            NextState = PlayerStateMachine.PlayerState.WallJump;
        }

    }
}