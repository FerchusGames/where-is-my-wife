using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using WIMW.Input;
using Zenject;

namespace WhereIsMyWife.Managers
{
    public partial class InputEventManager : IPlayerInputEvent
    {
        private readonly Subject<Unit> _jumpStartSubject = new Subject<Unit>();
        private readonly Subject<Unit> _jumpEndSubject = new Subject<Unit>();
        private readonly Subject<float> _runSubject = new Subject<float>();
        private readonly Subject<Vector2> _dashSubject = new Subject<Vector2>();
        private readonly Subject<Vector2> _useItemSubject = new Subject<Vector2>();
        private readonly Subject<Unit> _hookStartSubject = new Subject<Unit>();
        private readonly Subject<Unit> _hookEndSubject = new Subject<Unit>();

        public IObservable<Unit> JumpStartAction => _jumpStartSubject.AsObservable();
        public IObservable<Unit> JumpEndAction => _jumpEndSubject.AsObservable();
        public IObservable<float> RunAction => _runSubject.AsObservable();
        public IObservable<Vector2> DashAction => _dashSubject.AsObservable();
        public IObservable<Vector2> UseItemAction => _useItemSubject.AsObservable();
        public IObservable<Unit> HookStartAction => _hookStartSubject.AsObservable();
        public IObservable<Unit> HookEndAction => _hookEndSubject.AsObservable();
    }

    public partial class InputEventManager : IInitializable
    {
        private PlayerInputActions _playerInputActions = new PlayerInputActions();
        
        private Vector2 _moveVector = Vector2.zero;
        
        public void Initialize()
        {
            _playerInputActions.Enable();
            SubscribeToInputActions();
            
            CheckForCurrentController();
        }

        private void SubscribeToInputActions()
        {
            _playerInputActions.Normal.Jump.performed += OnJumpPerform;
            _playerInputActions.Normal.Jump.canceled += OnJumpCancel;
            _playerInputActions.Normal.Move.performed += OnMovePerform;
            _playerInputActions.Normal.Move.canceled += OnMoveCancel;
            _playerInputActions.Normal.Dash.performed += OnDash;
        }

        private void OnJumpPerform(InputAction.CallbackContext context)
        {
            _jumpStartSubject.OnNext();
        }

        private void OnJumpCancel(InputAction.CallbackContext context)
        {
            _jumpEndSubject.OnNext();
        }

        private void OnMovePerform(InputAction.CallbackContext context)
        {
            _moveVector = context.ReadValue<Vector2>();
        }

        private void OnMoveCancel(InputAction.CallbackContext context)
        {
            _moveVector = Vector2.zero;
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            _dashSubject.OnNext(_moveVector.normalized); 
        }
    }

    public partial class InputEventManager : IDisposable
    {
        public void Dispose()
        {
            _playerInputActions.Disable();
        }
    }
    
    public partial class InputEventManager : ITickable
    {
        public void Tick()
        {
            CheckForControllerTypeChange();
            
            _runSubject.OnNext(_moveVector.x);
        }

        private void CheckForControllerTypeChange()
        {
            if (Input.anyKeyDown)
            {
                if (InputWasFromJoystick())
                {
                    if (_currentControllerType == ControllerType.Keyboard)
                    {
                        CheckForCurrentController();
                    }
                }
                else if (_currentControllerType != ControllerType.Keyboard)
                {
                    ChangeControllerType(ControllerType.Keyboard);
                }                    
            }
        }

        private bool InputWasFromJoystick()
        {
            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKeyDown((KeyCode)350 + i))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public partial class InputEventManager
    {
        ControllerType _currentControllerType;
        private string[] controllers;
        
        private void CheckForCurrentController()
        {
            controllers = Input.GetJoystickNames();
            
            for (int i = 0; i < controllers.Length; i++)
            {
                Debug.Log(controllers[i]);
            }

            if (OnlyKeyboardIsConnected())
            {
                ChangeControllerType(ControllerType.Keyboard);
            }

            else
            {
                ConnectController();
            }
        }

        private void ChangeControllerType(ControllerType controllerType)
        {
            _currentControllerType = controllerType;
            Debug.Log($"ControllerType: {_currentControllerType}");
        }

        private void ConnectController()
        {
            for (int index = 0; index < controllers.Length; index++)
            {
                if (IsControllerValid(index))
                {
                    if (IsXboxConnected(index))
                    {
                        ChangeControllerType(ControllerType.Xbox);
                    }
                    else if (IsPlaystationConnected(index))
                    {
                        ChangeControllerType(ControllerType.Playstation);
                    }
                    else if (IsNintendoConnected(index))
                    {
                        ChangeControllerType(ControllerType.Nintendo);
                    }
                    else
                    {
                        ChangeControllerType(ControllerType.Xbox);
                    }
                }
            }
        }

        private bool IsControllerValid(int index)
        {
            return controllers[index] != "";
        }

        private bool IsNintendoConnected(int index)
        {
            return controllers[index].Contains("Pro") || controllers[index].Contains("Core") || controllers[index].Contains("Switch");
        }

        private bool IsPlaystationConnected(int index)
        {
            return controllers[index].Contains("playstation") || controllers[index].Contains("PS");
        }

        private bool IsXboxConnected(int index)
        {
            return controllers[index].Contains("Xbox");
        }

        private bool OnlyKeyboardIsConnected()
        {
            return controllers == null || controllers.Length == 0 || (controllers.Length == 1 && controllers[0] == "");
        }
    }
}
