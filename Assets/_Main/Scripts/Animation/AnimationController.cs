using System;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Managers;
using Zenject;

namespace WhereIsMyWife.Controllers
{
    public class AnimationController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        [Inject] private IMovementStateEvents _movementStateEvents;
        [Inject] private IWallHangStateEvents _wallHangStateEvents;
        [Inject] private IDashStateEvents _dashStateEvents;
        [Inject] private IPlayerStateIndicator _stateIndicator;

        private Rigidbody2D _rigidbody2D;
        
        private const string RUN_ANIMATION_STATE = "walk";
        private const string IDLE_ANIMATION_STATE = "idle";
        private const string FALL_ANIMATION_STATE = "fall";
        private const string JUMP_ANIMATION_STATE = "jump";
        private const string CROUCH_ANIMATION_STATE = "crouch";
        private const string WALL_SLIDE_ANIMATION_STATE = "wall_slide";
        private const string LAND_ANIMATION_STATE = "land";
        private const string WALL_HIT_ANIMATION_STATE = "wall_hit";

        private string _currentAnimationState = "";

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _movementStateEvents.JumpStart.AsUnitObservable().Subscribe(Jump);
            _movementStateEvents.Run.AsUnitObservable().Subscribe(Run);

            _wallHangStateEvents.StartWallHang.Subscribe(StartWallHang);
        }

        private void Jump()
        {
            PlayAnimationState(JUMP_ANIMATION_STATE);
        }

        private void Run()
        {
            if (!_stateIndicator.IsJumping)
            {
                if (_stateIndicator.IsRunFalling)
                {
                    PlayAnimationState(FALL_ANIMATION_STATE);
                }
                
                else if (Mathf.Abs(_rigidbody2D.velocity.x) >= 1f)
                {
                    PlayAnimationState(RUN_ANIMATION_STATE);
                }
                else
                {
                    PlayAnimationState(IDLE_ANIMATION_STATE);
                }
                
            }
        }

        private void StartWallHang()
        {
            PlayAnimationState(WALL_HIT_ANIMATION_STATE);
        }
        
        private void PlayAnimationState(string newState)
        {
            if (_currentAnimationState == newState) return;
            
            _animator.Play(newState);

            _currentAnimationState = newState;
        }
    }
}



