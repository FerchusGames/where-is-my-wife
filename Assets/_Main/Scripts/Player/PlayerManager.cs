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
        public bool IsFacingRight();
        public bool IsJumping();
        public bool IsJumpCut();
        public bool IsJumpFalling();
        public bool IsOnJumpInputBuffer();
        public bool IsOnGround();
        public bool IsInJumpHang();
    }
    
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
        public bool IsFacingRight()
        {
            return false;
        }

        public bool IsJumping()
        {
            return false;
        }

        public bool IsJumpCut()
        {
            return false;
        }

        public bool IsJumpFalling()
        {
            return false;
        }

        public bool IsOnJumpInputBuffer()
        {
            return _lastPressedJumpTime >= 0;
        }

        public bool IsOnGround()
        {
            return _lastOnGroundTime >= 0;
        }
        
        public bool IsInJumpHang()
        {
            return (IsJumping() || IsJumpFalling()) 
                   && Mathf.Abs(_controllerData.RigidbodyVelocity.y) < _properties.Jump.HangTimeThreshold;
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

        public IObservable<float> JumpStart => _jumpStartSubject.AsObservable();
        public IObservable<Unit> JumpEnd => _jumpEndSubject.AsObservable();
        public IObservable<float> Run => _runSubject.AsObservable();
        public IObservable<Vector2> DashStart => _dashStartSubject.AsObservable();
        public IObservable<Unit> DashEnd => _dashEndSubject.AsObservable();
        
        private void ExecuteJumpStartEvent()
        {
            _jumpStartSubject.OnNext(_jumpingMethods.GetJumpForce(_controllerData.RigidbodyVelocity.y));
        }

        private void ExecuteRunEvent(float runDirection)
        {
            _runSubject.OnNext(_runningMethods.GetRunAcceleration(runDirection, _controllerData.RigidbodyVelocity.x));
        }

        private void ExecuteDashStartEvent(Vector2 dashDirection)
        {
            Dash(dashDirection).Forget();
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
        }

        private void SubscribeToObservables()
        {
            _playerInputEvent.JumpStartAction.Subscribe(ExecuteJumpStartEvent);
            _playerInputEvent.RunAction.Subscribe(ExecuteRunEvent);
            _playerInputEvent.DashAction.Subscribe(ExecuteDashStartEvent);
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
