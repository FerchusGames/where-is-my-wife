using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace WhereIsMyWife.Controllers
{
    public partial class PlayerController : IPlayerControllerData
    {
        public Vector2 RigidbodyVelocity => _rigidbody2D.velocity;
        public Vector2 GroundCheckPosition => _groundCheckTransform.position;
    }
    
    public partial class PlayerController : MonoBehaviour
    {
        [Inject] private IPlayerControllerInput _playerControllerInput;
        [Inject] private IPlayerControllerEvent _playerControllerEvent;
        [Inject] private IRespawn _respawn;

        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Transform _groundCheckTransform = null;

        private float _runAcceleration;
        
        private void Start()
        {
            _playerControllerEvent.SetPlayerControllerData(this);

            SubscribeToObservables();
        }

        private void FixedUpdate()
        {
            Run();
        }

        private void SubscribeToObservables()
        {
            _playerControllerInput.JumpStart.Subscribe(JumpStart).AddTo(this);
            
            // TODO: Subscribe to Run() instead after being called every FixedUpdate
            _playerControllerInput.Run.Subscribe(SetRunAccelerationRate).AddTo(this); 
                
            _playerControllerInput.DashStart.Subscribe(DashStart).AddTo(this);
            _playerControllerInput.DashEnd.Subscribe(DashEnd).AddTo(this);
            _playerControllerInput.GravityScale.Subscribe(SetGravityScale).AddTo(this);
            _playerControllerInput.FallSpeedCap.Subscribe(SetFallSpeedCap).AddTo(this);
            _playerControllerInput.Turn.Subscribe(Turn).AddTo(this);
            
            _respawn.RespawnAction.Subscribe(Respawn).AddTo(this);
        }

        private void JumpStart(float jumpForce)
        {
            _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        private void SetRunAccelerationRate(float runAcceleration)
        {
            _runAcceleration = runAcceleration;
        }

        private void Run()
        {
            Debug.Log($"Run Accceleration: {_runAcceleration}");
            _rigidbody2D.AddForce(Vector2.right * _runAcceleration, ForceMode2D.Force);
        }

        private void Turn()
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        private void DashStart(Vector2 dashForce)
        {
            _rigidbody2D.velocity = dashForce;
        }

        private void DashEnd()
        {
            _rigidbody2D.velocity = Vector2.zero;
        }

        private void SetGravityScale(float gravityScale)
        {
            _rigidbody2D.gravityScale = gravityScale;
        }

        private void SetFallSpeedCap(float fallSpeedCap)
        {
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x,
                Mathf.Max(_rigidbody2D.velocity.y, -fallSpeedCap));
        }

        private void Respawn(Vector3 respawnPosition)
        {
            transform.position = respawnPosition;
        }
    }
}
