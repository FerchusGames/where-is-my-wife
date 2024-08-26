using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Managers.Properties;
using Zenject;

namespace WhereIsMyWife.Managers
{
    public partial class PlayerManager
    {
        [Inject] private IPlayerProperties _playerProperties;
        
        // State control
        private bool _isFacingRight = true;
        private bool _isJumping = false;
        private bool _isJumpCut = false;
        private bool _isJumpFalling = false;
        
        // Movement
        private float _accelerationRate = 0;
        private float _targetSpeed = 0;
        
        // Timers
        private float _lastOnGroundTime = 0;
        private float _lastPressedJumpTime = 0;
    }
    
    public partial class PlayerManager : IPlayerControllerInput
    {
        [Inject] private IPlayerInput _playerInput;

        private Subject<float> _jumpStartSubject = new Subject<float>();
        private Subject<Unit> _jumpEndSubject = new Subject<Unit>();
        private Subject<float> _runSubject = new Subject<float>();
        private Subject<Vector2> _dashStartSubject = new Subject<Vector2>();
        private Subject<Unit> _dashEndSubject = new Subject<Unit>();

        public IObservable<float> JumpStart => _jumpStartSubject.AsObservable();
        public IObservable<Unit> JumpEnd => _jumpEndSubject.AsObservable();
        public IObservable<float> Run => _runSubject.AsObservable();
        public IObservable<Vector2> DashStart => _dashStartSubject.AsObservable();
        public IObservable<Unit> DashEnd => _dashEndSubject.AsObservable();
        
        private void ExecuteJumpStartEvent()
        {
            _jumpStartSubject.OnNext(GetJumpForce());
        }
        
        private float GetJumpForce()
        {
            float force = _playerProperties.Jump.ForceMagnitude;

            if (_playerControllerData.RigidbodyVelocity.y < 0)
            {
                force -= _playerControllerData.RigidbodyVelocity.y; // To always jump the same amount.
            }

            return force;
        }

        private void ExecuteRunEvent(float runDirection)
        {
            _runSubject.OnNext(GetRunAcceleration(runDirection));
        }
        
        private float GetRunAcceleration(float runDirection)
        {
            _targetSpeed = runDirection * _playerProperties.Movement.RunMaxSpeed;
            
            UpdateAccelerationRate();
            
            return GetTargetAndCurrentSpeedDifference() * _accelerationRate;
        }
        
        private void UpdateAccelerationRate()
        {
            UpdateBaseAccelerationRate();
            AddJumpHangMultipliers();
        }
        
        private void UpdateBaseAccelerationRate()
        {
            if (IsOnGround())
            {
                _accelerationRate = GetGroundAccelerationRate();
            }

            else
            {
                _accelerationRate = GetAirAccelerationRate();
            }
        }
        
        private bool IsOnGround()
        {
            return _lastOnGroundTime > 0;
        }
        
        private float GetGroundAccelerationRate()
        {
            if (IsAccelerating())
            {
                return _playerProperties.Movement.RunAccelerationRate;
            }

            return _playerProperties.Movement.RunDecelerationRate;
        }
        
        private float GetAirAccelerationRate()
        {
            if (IsAccelerating())
            {
                return _playerProperties.Movement.RunAccelerationRate 
                       * _playerProperties.Movement.AirAccelerationMultiplier;
            }
            
            return _playerProperties.Movement.RunDecelerationRate 
                   * _playerProperties.Movement.AirDecelerationMultiplier;
        }

        private bool IsAccelerating()
        {
            return Mathf.Abs(_targetSpeed) > 0.01f;
        }
        
        private void AddJumpHangMultipliers()
        {
            if (IsInJumpHang())
            {
                _accelerationRate *= _playerProperties.Jump.HangAccelerationMultiplier;
                _targetSpeed *= _playerProperties.Jump.HangMaxSpeedMultiplier;
            }
        }
        
        private bool IsInJumpHang()
        {
            return (_isJumping || _isJumpFalling) 
                   && Mathf.Abs(_playerControllerData.RigidbodyVelocity.y) < _playerProperties.Jump.HangTimeThreshold;
        }
        
        private float GetTargetAndCurrentSpeedDifference()
        {
            return _targetSpeed - _playerControllerData.RigidbodyVelocity.x;
        }

        private void ExecuteDashStartEvent(Vector2 dashDirection)
        {
            Dash(dashDirection).Forget();
        }
        
        private async UniTaskVoid Dash(Vector2 dashDirection)
        {
            _dashStartSubject.OnNext(dashDirection * _playerProperties.Dash.Speed);

            await UniTask.Delay(TimeSpan.FromSeconds(_playerProperties.Dash.Duration), ignoreTimeScale: false);
            
            _dashEndSubject.OnNext();
        }
    }

    public partial class PlayerManager : IPlayerControllerEvent
    {
        private IPlayerControllerData _playerControllerData;
        
        public void SetPlayerControllerData(IPlayerControllerData playerControllerData)
        {
            _playerControllerData = playerControllerData;
        }
    }

    public partial class PlayerManager : IInitializable
    {
        public void Initialize()
        {
            SubscribeToObservables();
        }

        private void SubscribeToObservables()
        {
            _playerInput.JumpStartAction.Subscribe(ExecuteJumpStartEvent);
            _playerInput.RunAction.Subscribe(ExecuteRunEvent);
            _playerInput.DashStartAction.Subscribe(ExecuteDashStartEvent);
        }
    }

    public partial class PlayerManager : ITickable
    {
        public void Tick()
        {
            TickTimers();
        }

        private void TickTimers()
        {
            _lastOnGroundTime -= Time.deltaTime;
            _lastPressedJumpTime -= Time.deltaTime;
        }
    }
}
