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

        public IObservable<float> WallJumpVelocity => _wallJumpVelocitySubject.AsObservable();
        
        [Inject] private IPlayerMovementProperties _movementProperties;
        [Inject] private IPlayerStateIndicator _stateIndicator;
        
        private Tween _horizontalSpeedTween;
        
        private float _horizontalSpeed = 0;
        private bool _initialFacingDirection;
        
        protected override void SubscribeToObservables()
        {
           
        }

        protected override void UnsubscribeToObservables()
        {
            
        }

        public override void EnterState()
        {
            base.EnterState();
            
            _initialFacingDirection = _stateIndicator.IsRunningRight;
            
            StartHorizontalSpeedCurve();
        }

        public override void UpdateState()
        {
            _wallJumpVelocitySubject.OnNext(0f);
        }
        
        private void StartHorizontalSpeedCurve()
        {
            _horizontalSpeed = 0;
            
            _horizontalSpeedTween = DOTween.To(() => _horizontalSpeed, x => _horizontalSpeed = x, 
                    -_movementProperties.WallSlideMaxVelocity, 
                    _movementProperties.WallSlideTimeToMaxVelocity)
                .SetEase(Ease.InOutSine);
        }

        public override void ExitState()
        {
            base.ExitState();

            KillHorizontalSpeedCurve();
        }

        private void KillHorizontalSpeedCurve()
        {
            if (_horizontalSpeedTween != null && _horizontalSpeedTween.IsActive())
            {
                _horizontalSpeedTween.Kill();
            }
        }
        

        private bool PlayerIsGoingOppositeDirectionOfWall()
        {
            return _initialFacingDirection != _stateIndicator.IsRunningRight;
        }
        
        private void EndWallHang()
        {
            NextState = PlayerStateMachine.PlayerState.Movement;
        }

        private void Dash()
        {
            NextState = PlayerStateMachine.PlayerState.Dash;
        }
    }
}