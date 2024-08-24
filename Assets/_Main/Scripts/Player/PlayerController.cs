using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace WhereIsMyWife.Controllers
{
    public interface IPlayerControllerInput
    {
        IObservable<float> JumpStart { get; }
        IObservable<float> Run { get; }
        IObservable<Vector2> DashStart { get; }
        IObservable<Unit> DashEnd { get; }
    }

    public interface IPlayerControllerEvent
    {
        void SetPlayerControllerData(IPlayerControllerData playerControllerData);
    }

    public interface IPlayerControllerData
    {
        Vector2 RigidbodyVelocity { get; }
    }
    
    public partial class PlayerController : IPlayerControllerData
    {
        public Vector2 RigidbodyVelocity => _rigidbody2D.velocity;
    }
    
    public partial class PlayerController : MonoBehaviour
    {
        [Inject] private IPlayerControllerInput _playerControllerInput;
        [Inject] private IPlayerControllerEvent _playerControllerEvent;

        [SerializeField] private Rigidbody2D _rigidbody2D;
        
        private void Start()
        {
            _playerControllerEvent.SetPlayerControllerData(this);

            SubscribeToObservables();
        }

        private void SubscribeToObservables()
        {
            _playerControllerInput.JumpStart.Subscribe(JumpStart).AddTo(this);
            _playerControllerInput.Run.Subscribe(Run).AddTo(this);
            _playerControllerInput.DashStart.Subscribe(DashStart).AddTo(this);
            _playerControllerInput.DashEnd.Subscribe(DashEnd).AddTo(this);
        }

        private void JumpStart(float jumpForce)
        {
            _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        private void Run(float runAcceleration)
        {
            _rigidbody2D.AddForce(Vector2.right * runAcceleration, ForceMode2D.Force);
        }

        private void DashStart(Vector2 dashForce)
        {
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.AddForce(dashForce, ForceMode2D.Impulse);
        }

        private void DashEnd()
        {
            _rigidbody2D.velocity = Vector2.zero;
        }
    }
}
