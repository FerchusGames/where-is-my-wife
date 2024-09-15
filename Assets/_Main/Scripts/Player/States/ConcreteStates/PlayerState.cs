using UnityEngine;
using WhereIsMyWife.Player.StateMachine;
using Zenject;

namespace WhereIsMyWife.Player.State
{
    public class PlayerState : BaseState<PlayerStateMachine.PlayerState>
    {
        [Inject] protected IPlayerStateInput _playerStateInput;
        
        public PlayerState(PlayerStateMachine.PlayerState key) : base(key)
        {
        }
        
        protected virtual void SubscribeToObservables() {}
        protected virtual void UnsubscribeToObservables() {}

        public override void EnterState()
        {
            SubscribeToObservables();
        }

        public override void ExitState()
        {
            UnsubscribeToObservables();
        }

        public override void UpdateState() { }

        public override void FixedUpdateState() { }
        public override void OnTriggerEnter2D(Collider2D other) { }

        public override void OnTriggerStay2D(Collider2D other) { }

        public override void OnTriggerExit2D(Collider2D other) { }
    }

}