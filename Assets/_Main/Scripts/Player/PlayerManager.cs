using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Managers.Properties;
using Zenject;

namespace WhereIsMyWife.Managers
{
    public interface IPlayerStateIndicator
    {
        public bool IsDead { get; }
        public bool IsFacingRight { get; }
        public bool IsJumping { get; }
        public bool IsJumpCut { get; }
        public bool IsJumpFalling { get; }
        public bool IsOnJumpInputBuffer();
        public bool IsFastFalling();
        public bool IsOnGround();
        public bool IsInJumpHang();
        public bool IsIdling();
        public bool CanJump();
        public bool CanJumpCut();
    }
    
    public partial class PlayerManager 
    {
        [Inject] private IPlayerProperties _properties;

        [Inject] private IRunningMethods _runningMethods;
        [Inject] private IJumpingMethods _jumpingMethods;
        
        // Movement
        private float _accelerationRate = 0;
        private float _targetSpeed = 0;
        private bool _isGoingDown = false;
        
        // Timers
        private float _lastOnGroundTime = 0;
        private float _lastPressedJumpTime = 0;
    }

    public partial class PlayerManager : IAnimationControllerInput
    {
        private const string DASH_ANIMATION_STATE = "dash";
        private const string WALK_ANIMATION_STATE = "walk";
        private const string IDLE_ANIMATION_STATE = "idle";
        private const string FALL_ANIMATION_STATE = "fall";
        private const string JUMP_ANIMATION_STATE = "jump";
        private const string CROUCH_ANIMATION_STATE = "crouch";
        private const string WALL_SLIDE_ANIMATION_STATE = "wall_slide";
        private const string LAND_ANIMATION_STATE = "land";
        private const string WALL_HIT_ANIMATION_STATE = "wall_hit";

        private string _currentAnimationState = "";

        private ISubject<string> _playAnimationSubject = new Subject<string>();

        public IObservable<string> PlayAnimationStateAction => _playAnimationSubject.AsObservable();

        private void PlayAnimationState(string newState)
        {
            if (_currentAnimationState == newState) return;
            
            _playAnimationSubject.OnNext(newState);

            _currentAnimationState = newState;
        }
    }
    
    public partial class PlayerManager : IPlayerStateIndicator
    {
        public bool IsDead { get; private set; } = false;
        public bool IsFacingRight { get; private set; } = true;
        public bool IsJumping { get; private set; } = false;
        public bool IsJumpCut { get; private set; } = false;
        public bool IsJumpFalling { get; private set; } = false;
        
        public bool IsOnJumpInputBuffer()
        {
            return _lastPressedJumpTime >= 0;
        }

        public bool IsOnGround()
        {
            return _lastOnGroundTime >= 0;
        }

        public bool IsFastFalling()
        {
            return _controllerData.RigidbodyVelocity.y < 0 && _isGoingDown;
        }
        
        public bool IsInJumpHang()
        {
            return (IsJumping || IsJumpFalling) 
                   && Mathf.Abs(_controllerData.RigidbodyVelocity.y) < _properties.Jump.HangTimeThreshold;
        }

        public bool IsIdling()
        {
            return (Mathf.Abs(_controllerData.RigidbodyVelocity.x) < 0.1f 
                    && Mathf.Abs(_controllerData.RigidbodyVelocity.y) < 0.1f);
        }

        public bool CanJump()
        {
            return _lastOnGroundTime > 0 && !IsJumping;
        }

        public bool CanJumpCut()
        {
            return IsJumping && _controllerData.RigidbodyVelocity.y > 0;
        }
    }
    
    public partial class PlayerManager : IPlayerControllerInput
    {
        [Inject] private IPlayerInputEvent _playerInputEvent;

        private Subject<float> _jumpStartSubject = new Subject<float>();
        private Subject<Unit> _jumpEndSubject = new Subject<Unit>();
        private Subject<float> _runSubject = new Subject<float>();
        private Subject<Vector2> _dashStartSubject = new Subject<Vector2>();
        private Subject<Unit> _dashEndSubject = new Subject<Unit>();
        private Subject<float> _gravityScaleSubject = new Subject<float>();
        private Subject<float> _fallSpeedCapSubject = new Subject<float>();
        private Subject<Unit> _turnSubject = new Subject<Unit>();

        public IObservable<float> JumpStart => _jumpStartSubject.AsObservable();
        public IObservable<Unit> JumpEnd => _jumpEndSubject.AsObservable();
        public IObservable<float> Run => _runSubject.AsObservable();
        public IObservable<Vector2> DashStart => _dashStartSubject.AsObservable();
        public IObservable<Unit> DashEnd => _dashEndSubject.AsObservable();
        public IObservable<float> GravityScale => _gravityScaleSubject.AsObservable();
        public IObservable<float> FallSpeedCap => _fallSpeedCapSubject.AsObservable();
        public IObservable<Unit> Turn => _turnSubject.AsObservable();
        
        private void ExecuteJumpStartEvent()
        {
            _lastPressedJumpTime = _properties.Jump.InputBufferTime;
        }

        private void ExecuteJumpEndEvent()
        {
            if (CanJumpCut())
            {
                IsJumpCut = true;
            }
        }
        
        private void ExecuteRunEvent(float runDirection)
        {
            if (runDirection != 0)
            {
                bool isMovingRight = runDirection > 0;
                CheckDirectionToFace(isMovingRight);
                PlayAnimationState(WALK_ANIMATION_STATE);   
            }
            
            _runSubject.OnNext(_runningMethods.GetRunAcceleration(runDirection, _controllerData.RigidbodyVelocity.x));
        }

        private void CheckDirectionToFace(bool isMovingRight)
        {
            if (isMovingRight != IsFacingRight)
            {
                _turnSubject.OnNext();
                IsFacingRight = !IsFacingRight;
            }
        }

        private void ExecuteDashStartEvent(Vector2 dashDirection)
        {
            Dash(dashDirection).Forget();
        }

        private void ExecuteGoDownEvent()
        {
            _isGoingDown = true;
        }
        
        private async UniTaskVoid Dash(Vector2 dashDirection)
        {
            _dashStartSubject.OnNext(dashDirection * _properties.Dash.Speed);

            await UniTask.Delay(TimeSpan.FromSeconds(_properties.Dash.Duration), ignoreTimeScale: false);
            
            _dashEndSubject.OnNext();
        }
    }

    public partial class PlayerManager : IPlayerControllerEvent
    {
        private IPlayerControllerData _controllerData;
        
        public void SetPlayerControllerData(IPlayerControllerData playerControllerData)
        {
            _controllerData = playerControllerData;
        }
    }

    public partial class PlayerManager : IInitializable
    {
        public void Initialize()
        {
            SubscribeToObservables();

            _gravityScaleSubject.OnNext(_properties.Gravity.Scale);
        }

        private void SubscribeToObservables()
        {
            _playerInputEvent.JumpStartAction.Subscribe(ExecuteJumpStartEvent);
            _playerInputEvent.JumpEndAction.Subscribe(ExecuteJumpEndEvent);
            _playerInputEvent.RunAction.Subscribe(ExecuteRunEvent);
            _playerInputEvent.DashAction.Subscribe(ExecuteDashStartEvent);
            _playerInputEvent.GoDownAction.Subscribe(ExecuteGoDownEvent);
        }
    }

    public partial class PlayerManager : ITickable
    {
        public void Tick()
        {
            TickTimers();
            GroundCheck();
            JumpChecks();
            GravityShifts();
            Idle();
        }

        private void Idle()
        {
            if (IsIdling())
            {
                PlayAnimationState(IDLE_ANIMATION_STATE);
            }
        }

        private void TickTimers()
        {
            _lastOnGroundTime -= Time.deltaTime;
            _lastPressedJumpTime -= Time.deltaTime;
        }
        
        private void GroundCheck()
        {
            if (GetGroundCheckOverlapBox() && !IsJumping)
            {
                _lastOnGroundTime = _properties.Jump.CoyoteTime;
            }
        }

        private Collider2D GetGroundCheckOverlapBox()
        {
            return Physics2D.OverlapBox(_controllerData.GroundCheckPosition, _properties.Check.GroundCheckSize, 0,
                _properties.Check.GroundLayer);
        }
        
        private void JumpChecks()
        {
            JumpingCheck();
            JumpStopCheck();

            if (CanJump() && _lastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsJumpCut = false;
                IsJumpFalling = false;
                
                Jump();
            }
        }

        private void JumpingCheck()
        {
            if (IsJumping && _controllerData.RigidbodyVelocity.y < 0)
            {
                IsJumping = false;
                IsJumpFalling = true;
            }
        }

        private void JumpStopCheck()
        {
            if (_lastOnGroundTime > 0 && !IsJumping)
            {
                IsJumpCut = false;
                IsJumpFalling = false;
            }
        }
        
        private void Jump()
        {
            ResetJumpTimers();
            
            _jumpStartSubject.OnNext(_jumpingMethods.GetJumpForce(_controllerData.RigidbodyVelocity.y));
            PlayAnimationState(JUMP_ANIMATION_STATE);
        }

        private void ResetJumpTimers()
        {
            _lastPressedJumpTime = 0;
            _lastOnGroundTime = 0;
        }
        
        private void GravityShifts()
        {
            // Make player fall faster if holding down 
            if (IsFastFalling())
            {
                SetGravityScale(_properties.Gravity.Scale * _properties.Gravity.FastFallMultiplier);
                SetFallSpeedCap(_properties.Gravity.MaxFastFallSpeed);
            }
            
            // Scale gravity up if jump button released
            else if (IsJumpCut)
            {
                SetGravityScale(_properties.Gravity.Scale  * _properties.Gravity.JumpCutMultiplier);
                SetFallSpeedCap(_properties.Gravity.MaxBaseFallSpeed);
            }

            // Higher gravity when near jump height apex
            else if (IsInJumpHang())
            {
                SetGravityScale(_properties.Gravity.Scale  * _properties.Gravity.JumpHangMultiplier);
            }

            // Higher gravity if falling
            else if (IsJumpFalling)
            {
                SetGravityScale(_properties.Gravity.Scale  * _properties.Gravity.BaseFallMultiplier);
                SetFallSpeedCap(_properties.Gravity.MaxBaseFallSpeed);
            }

            // Reset gravity
            else
            {
                SetGravityScale(_properties.Gravity.Scale);
            }

            _isGoingDown = false;
        }

        private void SetFallSpeedCap(float fallSpeedCap)
        {
            _fallSpeedCapSubject.OnNext(fallSpeedCap);
        }

        private void SetGravityScale(float gravityScale)
        {
            _gravityScaleSubject.OnNext(gravityScale);
        }
    }
    
    
    public partial class PlayerManager : IRespawn
    {
        private Vector3 _respawnPoint;
     
        private ISubject<Vector3> _respawnSubject = new Subject<Vector3>();
        
        public IObservable<Vector3> RespawnAction => _respawnSubject.AsObservable();
        
        public void SetRespawnPoint(Vector3 respawnPoint)
        {
            _respawnPoint = respawnPoint;
        }

        public void TriggerRespawn()
        {
           _respawnSubject.OnNext(_respawnPoint);
        }
    }
}
