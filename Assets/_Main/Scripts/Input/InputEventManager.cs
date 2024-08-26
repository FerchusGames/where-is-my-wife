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
    
    public partial class InputEventManager : ITickable
    {
        public void Tick()
        {
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

        private static Vector2 GetNormalizedRawAxesVector2()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        }
    }
}
