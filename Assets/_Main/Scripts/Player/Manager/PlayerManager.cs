using System;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Managers.Properties;
using WhereIsMyWife.Player.State;
using Zenject;

namespace WhereIsMyWife.Managers
{
    public partial class PlayerManager 
    {
        [Inject] private IPlayerProperties _properties;

        [Inject] private IRunningMethods _runningMethods;
        [Inject] private IJumpingMethods _jumpingMethods;
        
        // Movement
        private float _accelerationRate = 0;
        private float _targetSpeed = 0;
        
        // Timers
        private float _lastOnGroundTime = 0;
        private float _lastPressedJumpTime = 0;
    }
    
    public partial class PlayerManager : IPlayerStateIndicator
    {
        public bool IsDead { get; private set; } = false;
        public bool IsAccelerating => _runningMethods.GetIsAccelerating();
        public bool IsRunningRight { get; private set; } = true;
        public bool IsLookingRight => _controllerData.HorizontalScale > 0;
        public bool IsLookingDown { get; private set; }
        public bool IsJumping { get; private set; } = false;
        public bool IsJumpCut { get; private set; } = false;
        public bool IsJumpFalling { get; private set; } = false;
        public bool IsOnWallHang { get; private set; } = false;
        public bool IsRunFalling { get; private set; } = false;

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
            return _controllerData.RigidbodyVelocity.y < 0 && IsLookingDown;
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
            return (_lastOnGroundTime > 0 && !IsJumping) || IsOnWallHang;
        }

        public bool CanJumpCut()
        {
            return IsJumping && _controllerData.RigidbodyVelocity.y > 0;
        }
    }
    
    public partial class PlayerManager : IPlayerStateInput
    {
        [Inject] private IPlayerInputEvent _playerInputEvent;

        private Subject<float> _jumpStartSubject = new Subject<float>();
        private Subject<Unit> _jumpEndSubject = new Subject<Unit>();
        private Subject<float> _runSubject = new Subject<float>();
        private Subject<Vector2> _dashStartSubject = new Subject<Vector2>();
        private Subject<Unit> _wallHangStartSubject = new Subject<Unit>();
        private Subject<Unit> _wallHangEndSubject = new Subject<Unit>();
        private Subject<float> _gravityScaleSubject = new Subject<float>();
        private Subject<float> _fallSpeedCapSubject = new Subject<float>();
        private Subject<Unit> _landSubject = new Subject<Unit>();

        public IObservable<float> JumpStart => _jumpStartSubject.AsObservable();
        public IObservable<Unit> JumpEnd => _jumpEndSubject.AsObservable();
        public IObservable<float> Run => _runSubject.AsObservable();
        public IObservable<Unit> WallHangStart => _wallHangStartSubject.AsObservable();
        public IObservable<Unit> WallHangEnd => _wallHangEndSubject.AsObservable();
        public IObservable<Vector2> DashStart => _dashStartSubject.AsObservable();
        public IObservable<float> GravityScale => _gravityScaleSubject.AsObservable();
        public IObservable<float> FallSpeedCap => _fallSpeedCapSubject.AsObservable();
        public IObservable<Unit> Land => _landSubject.AsUnitObservable();

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
            UpdateIsRunningRight(runDirection);
            _runSubject.OnNext(_runningMethods.GetRunAcceleration(runDirection, _controllerData.RigidbodyVelocity.x));
        }

        private void ExecuteDashStartEvent(Vector2 dashDirection)
        {
            _dashStartSubject.OnNext(dashDirection * _properties.Dash.Speed);
        }

        private void ExecuteLookDownEvent(bool isLookingDown)
        {
            IsLookingDown = isLookingDown;
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
            _playerInputEvent.LookDownAction.Subscribe(ExecuteLookDownEvent);
        }
    }

    public partial class PlayerManager : ITickable
    {
        public void Tick()
        {
            TickTimers();
            GroundCheck();
            WallCheck();
            JumpChecks();
            GravityShifts();
        }

        private void UpdateIsRunningRight(float runDirection)
        {
            if (runDirection > 0)
            {
                IsRunningRight = true;
            }
            
            else if (runDirection < 0)
            {
                IsRunningRight = false;
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
                IsRunFalling = false;
            }

            if (!IsJumping && _lastOnGroundTime < _properties.Jump.CoyoteTime)
            {
                IsRunFalling = true;
            }
        }

        private Collider2D GetGroundCheckOverlapBox()
        {
            return Physics2D.OverlapBox(_controllerData.GroundCheckPosition, _properties.Check.GroundCheckSize, 0,
                _properties.Check.GroundLayer);
        }

        private void WallCheck()
        {
            if (GetWallHangCheck())
            {
                if ((IsJumping || IsRunFalling))
                {
                    IsOnWallHang = true;
                    _wallHangStartSubject.OnNext();
                }
            }

            else
            {
                IsOnWallHang = false;
                _wallHangEndSubject.OnNext();
            }
        }

        private bool GetWallHangCheck()
        {
            return (Physics2D.OverlapBox(_controllerData.WallHangCheckUpPosition, _properties.Check.WallHangCheckSize,
                0, _properties.Check.GroundLayer)
                &&
                Physics2D.OverlapBox(_controllerData.WallHangCheckDownPosition, _properties.Check.WallHangCheckSize,
                    0, _properties.Check.GroundLayer)
                &&
                IsAccelerating
                );
        }

        private void JumpChecks()
        {
            JumpingCheck();
            LandCheck();

            if (CanJump() && _lastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsJumpCut = false;
                IsJumpFalling = false;
                IsRunFalling = false;
                
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

        private void LandCheck()
        {
            if (_lastOnGroundTime > 0 && !IsJumping)
            {
                IsJumpCut = false;
                IsJumpFalling = false;
                _landSubject.OnNext();
            }
        }
        
        private void Jump()
        {
            ResetJumpTimers();
            
            _jumpStartSubject.OnNext(_jumpingMethods.GetJumpForce(_controllerData.RigidbodyVelocity.y));
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
