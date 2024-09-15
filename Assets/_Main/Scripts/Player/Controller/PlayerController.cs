using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using WhereIsMyWife.Player.State;
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
        [Inject] private IMovementStateEvents _movementStateEvents;
        [Inject] private IPlayerControllerEvent _playerControllerEvent;
        [Inject] private IRespawn _respawn;

        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Transform _groundCheckTransform = null;
        
        private void Start()
        {
            _playerControllerEvent.SetPlayerControllerData(this);

            SubscribeToObservables();
        }

        private void SubscribeToObservables()
        {
            _movementStateEvents.JumpStart.Subscribe(JumpStart).AddTo(this);
            
            // TODO: Subscribe to Run() instead after being called every FixedUpdate
            _movementStateEvents.Run.Subscribe(Run).AddTo(this); 
            
            _movementStateEvents.GravityScale.Subscribe(SetGravityScale).AddTo(this);
            _movementStateEvents.FallSpeedCap.Subscribe(SetFallSpeedCap).AddTo(this);
            _movementStateEvents.Turn.Subscribe(Turn).AddTo(this);
            
            _respawn.RespawnAction.Subscribe(Respawn).AddTo(this);
        }

        private void JumpStart(float jumpForce)
        {
            _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        private void Run(float runAcceleration)
        {
            Debug.Log($"Run Accceleration: {runAcceleration}");
            _rigidbody2D.AddForce(Vector2.right * runAcceleration, ForceMode2D.Force);
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
