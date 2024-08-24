using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace WhereIsMyWife.Managers
{
    public partial class InputManager : ITickable
    {
        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jumpStartSubject.OnNext();
            }
            
            _runSubject.OnNext(Input.GetAxisRaw("Horizontal"));

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _dashStartSubject.OnNext(GetNormalizedRawAxesVector2()); 
            }
        }

        private static Vector2 GetNormalizedRawAxesVector2()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        }
    }

    public partial class InputManager : IPlayerInput
    {
        private readonly Subject<Unit> _jumpStartSubject = new Subject<Unit>();
        private readonly Subject<float> _runSubject = new Subject<float>();
        private readonly Subject<Vector2> _dashStartSubject = new Subject<Vector2>();

        public IObservable<Unit> JumpStartAction => _jumpStartSubject.AsObservable();
        public IObservable<float> RunAction => _runSubject.AsObservable();
        public IObservable<Vector2> DashStartAction => _dashStartSubject.AsObservable();
    }

    public partial class InputManager
    {
        int QuantizeAxis ( float input)
        {
            if (input < -0.5f) return -1;
            if (input > 0.5f) return 1;
            return 0;
        }
    }
}
