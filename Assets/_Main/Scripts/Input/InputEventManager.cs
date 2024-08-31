using System;
using UniRx;
using UnityEngine;
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
        private readonly Subject<Unit> _lookUpSubject = new Subject<Unit>();
        private readonly Subject<Unit> _goDownSubject = new Subject<Unit>();

        public IObservable<Unit> JumpStartAction => _jumpStartSubject.AsObservable();
        public IObservable<Unit> JumpEndAction => _jumpEndSubject.AsObservable();
        public IObservable<float> RunAction => _runSubject.AsObservable();
        public IObservable<Vector2> DashAction => _dashSubject.AsObservable();
        public IObservable<Vector2> UseItemAction => _useItemSubject.AsObservable();
        public IObservable<Unit> HookStartAction => _hookStartSubject.AsObservable();
        public IObservable<Unit> HookEndAction => _hookEndSubject.AsObservable();
        public IObservable<Unit> LookUpAction => _lookUpSubject.AsObservable();
        public IObservable<Unit> GoDownAction => _goDownSubject.AsObservable();
    }

    public partial class InputEventManager : IInitializable
    {
        public void Initialize()
        {
            CheckForCurrentController();
        }
    }
    
    public partial class InputEventManager : ITickable
    {
        public void Tick()
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
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jumpStartSubject.OnNext();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                _jumpEndSubject.OnNext();
            }
            
            _runSubject.OnNext(Input.GetAxisRaw("Horizontal"));
            
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _dashSubject.OnNext(GetNormalizedRawAxesVector2()); 
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                _useItemSubject.OnNext(GetNormalizedRawAxesVector2());
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _hookStartSubject.OnNext();
            }

            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                _hookEndSubject.OnNext();
            }
            
            if (Input.GetKey(KeyCode.W))
            {
                _lookUpSubject.OnNext();
            }

            if (Input.GetKey(KeyCode.S))
            {
                _goDownSubject.OnNext();
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

        private static Vector2 GetNormalizedRawAxesVector2()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
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
                if (!IsControllerValid(index))
                {
                    if (IsXboxConnected(index))
                    {
                        ChangeControllerType(ControllerType.Xbox);
                        return;
                    }
                    else if (IsPlaystationConnected(index))
                    {
                        ChangeControllerType(ControllerType.Playstation);
                        return;
                    }
                    else if (IsNintendoConnected(index))
                    {
                        ChangeControllerType(ControllerType.Nintendo);
                        return;
                    }
                }
            }
        }

        private bool IsControllerValid(int index)
        {
            return controllers[index] == "";
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
