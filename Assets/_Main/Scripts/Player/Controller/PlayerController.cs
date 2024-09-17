using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using WhereIsMyWife.Managers;
using WhereIsMyWife.Player.State;
using Zenject;

namespace WhereIsMyWife.Controllers
{
    public partial class PlayerController : IPlayerControllerData
    {
        public Vector2 RigidbodyVelocity => _rigidbody2D.velocity;
        public Vector2 GroundCheckPosition => _groundCheckTransform.position;
        public Vector2 WallHangCheckUpPosition => _wallHangCheckUpTransform.position;
        public Vector2 WallHangCheckDownPosition => _wallHangCheckDownTransform.position;
        public float HorizontalScale => transform.localScale.x;
    }
    
    public partial class PlayerController : MonoBehaviour
    {
        [Inject] private IMovementStateEvents _movementStateEvents;
        [Inject] private IWallHangStateEvents _wallHangStateEvents;
        [Inject] private IWallJumpStateEvents _wallJumpStateEvents;
        
        [Inject] private IPlayerStateIndicator _playerStateIndicator;
        [Inject] private IPlayerControllerEvent _playerControllerEvent;
        [Inject] private IRespawn _respawn;

        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Transform _groundCheckTransform = null;
        [SerializeField] private Transform _wallHangCheckUpTransform = null;
        [SerializeField] private Transform _wallHangCheckDownTransform = null;
        
        private void Start()
        {
            _playerControllerEvent.SetPlayerControllerData(this);

            SubscribeToObservables();
        }

        private void Update()
        {
            
        }

        private void SubscribeToObservables()
        {
            _movementStateEvents.Run.Subscribe(Run).AddTo(this); 
            _movementStateEvents.JumpStart.Subscribe(JumpStart).AddTo(this);
            _movementStateEvents.GravityScale.Subscribe(SetGravityScale).AddTo(this);
            _movementStateEvents.FallSpeedCap.Subscribe(SetFallSpeedCap).AddTo(this);

            _wallHangStateEvents.WallHangVelocity.Subscribe(WallHangVelocity).AddTo(this);
            _wallHangStateEvents.Turn.Subscribe(Turn).AddTo(this);
            _wallHangStateEvents.WallJumpStart.Subscribe(JumpStart).AddTo(this);

            _wallJumpStateEvents.WallJumpVelocity.Subscribe(SetHorizontalSpeed).AddTo(this);
            _wallJumpStateEvents.GravityScale.Subscribe(SetGravityScale).AddTo(this);
            _wallJumpStateEvents.FallSpeedCap.Subscribe(SetFallSpeedCap).AddTo(this);
            
            _respawn.RespawnAction.Subscribe(Respawn).AddTo(this);
        }

        private void JumpStart(float jumpForce)
        {
            _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        private void Run(float runAcceleration)
        {
            FaceDirection(_playerStateIndicator.IsRunningRight);
            _rigidbody2D.AddForce(Vector2.right * runAcceleration, ForceMode2D.Force);
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

        private void SetHorizontalSpeed(float speed)
        {
            _rigidbody2D.velocity = new Vector2(speed, _rigidbody2D.velocity.y);
        }
        
        private void WallHangVelocity(float fallVelocity)
        {
            SetGravityScale(0f);
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x,
                fallVelocity);
        }

        private void Turn()
        {
            Vector3 scale = transform.localScale;

            scale.x *= -1;
            
            transform.localScale = scale;
        }
        
        private void FaceDirection(bool shouldFaceRight)
        {
            Vector3 scale = transform.localScale;
            
            if (shouldFaceRight)
            {
                scale.x = 1;
            }

            else
            {
                scale.x = -1;
            }

            transform.localScale = scale;
        }
        
        private void Respawn(Vector3 respawnPosition)
        {
            transform.position = respawnPosition;
        }
    }
}
