using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Managers.Properties;
using Zenject;

namespace WhereIsMyWife.Managers
{
    public partial class PlayerManager : IPlayerControllerInput
    {
        [Inject] private IPlayerInput _playerInput;

        private Subject<float> _jumpStartSubject = new Subject<float>();
        private Subject<float> _runSubject = new Subject<float>();
        private Subject<Vector2> _dashStartSubject = new Subject<Vector2>();
        private Subject<Unit> _dashEndSubject = new Subject<Unit>();

        public IObservable<float> JumpStart => _jumpStartSubject.AsObservable();
        public IObservable<float> Run => _runSubject.AsObservable();
        public IObservable<Vector2> DashStart => _dashStartSubject.AsObservable();
        public IObservable<Unit> DashEnd => _dashEndSubject.AsObservable();
        
        private void ExecuteJumpStartEvent()
        {
            _jumpStartSubject.OnNext(_playerProperties.Jump.ForceMagnitude);
        }

        private void ExecuteRunEvent(float runDirection)
        {
            _runSubject.OnNext(GetRunAcceleration(runDirection));
        }

        private void ExecuteDashStartEvent(Vector2 dashDirection)
        {
            Dash(dashDirection).Forget();
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
            _lastOnGroundTime -= Time.deltaTime;
        }
    }
    
    public partial class PlayerManager
    {
        [Inject] private IPlayerProperties _playerProperties;
        
        private float _lastOnGroundTime = 0;

        private bool _isJumping = false;
        private bool _isJumpFalling = false;
        private float _accelerationRate;
        private float _targetSpeed;

        private async UniTaskVoid Dash(Vector2 dashDirection)
        {
            _dashStartSubject.OnNext(dashDirection * _playerProperties.Dash.Speed);

            await UniTask.Delay(TimeSpan.FromSeconds(_playerProperties.Dash.Duration), ignoreTimeScale: false);
            
            _dashEndSubject.OnNext();
        }

        private float GetRunAcceleration(float runDirection)
        {
            _targetSpeed = runDirection * _playerProperties.Movement.RunMaxSpeed;
            
            UpdateAccelerationRate();
            
            return GetTargetAndCurrentSpeedDifference() * _accelerationRate;
        }

        private float GetTargetAndCurrentSpeedDifference()
        {
            return _targetSpeed - _playerControllerData.RigidbodyVelocity.x;
        }

        private void UpdateAccelerationRate()
        {
            UpdateBaseAccelerationRate();
            AddJumpHangMultipliers();
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

        private float GetGroundAccelerationRate()
        {
            if (IsAccelerating())
            {
                return _playerProperties.Movement.RunAccelerationRate;
            }

            return _playerProperties.Movement.RunDecelerationRate;
        }

        private bool IsAccelerating()
        {
            return Mathf.Abs(_targetSpeed) > 0.01f;
        }

        private bool IsOnGround()
        {
            return _lastOnGroundTime > 0;
        }
    }
}
