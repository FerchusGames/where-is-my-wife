using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Managers.Properties;
using WhereIsMyWife.Player.StateMachine;
using Zenject;

namespace WhereIsMyWife.Player.State
{
    public class PlayerWallHangState : PlayerState, IWallHangState, IWallHangStateEvents
    {
        public PlayerWallHangState() : base(PlayerStateMachine.PlayerState.Movement) { }
        
        private ISubject<float> _wallHangGravitySubject = new Subject<float>();

        public IObservable<float> WallHangVelocity => _wallHangGravitySubject.AsObservable();

        [Inject] private IPlayerMovementProperties _movementProperties;
        
        private float _slideSpeed = 0;
        
        protected override void SubscribeToObservables()
        {
            _playerStateInput.DashStart.AsUnitObservable().Subscribe(Dash);
            _playerStateInput.JumpStart.AsUnitObservable().Subscribe(Jump);
        }

        protected override void UnsubscribeToObservables()
        {
            
        }

        public override void EnterState()
        {
            base.EnterState();
            _slideSpeed = 0;
            DOTween.To(() => _slideSpeed, x => _slideSpeed = x, 
                -_movementProperties.WallSlideMaxVelocity, 
                _movementProperties.WallSlideTimeToMaxVelocity)
                .SetEase(Ease.InExpo);
        }

        public override void UpdateState()
        {
            _wallHangGravitySubject.OnNext(_slideSpeed);
        }

        private void Dash()
        {
            NextState = PlayerStateMachine.PlayerState.Dash;
        }

        private void Jump()
        {
            NextState = PlayerStateMachine.PlayerState.WallJump;
        }

    }
}