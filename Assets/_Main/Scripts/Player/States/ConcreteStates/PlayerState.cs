using UnityEngine;
using WhereIsMyWife.Player.StateMachine;
using Zenject;

namespace WhereIsMyWife.Player.State
{
    public class PlayerState : IBaseState<PlayerStateMachine.PlayerState>
    {
        public PlayerState(PlayerStateMachine.PlayerState stateKey)
        {
            StateKey = stateKey;
        }
        
        [Inject] protected IPlayerStateInput _playerStateInput;
        
        protected virtual void SubscribeToObservables() {}
        protected virtual void UnsubscribeToObservables() {}

        public void EnterState()
        {
            SubscribeToObservables();
        }

        public void ExitState()
        {
            UnsubscribeToObservables();
        }

        public void UpdateState() { }

        public void FixedUpdateState() { }
        public PlayerStateMachine.PlayerState NextState { get; protected set; }
        public PlayerStateMachine.PlayerState StateKey { get; protected set; }
        public void OnTriggerEnter2D(Collider2D other) { }

        public void OnTriggerStay2D(Collider2D other) { }

        public void OnTriggerExit2D(Collider2D other) { }
    }

}