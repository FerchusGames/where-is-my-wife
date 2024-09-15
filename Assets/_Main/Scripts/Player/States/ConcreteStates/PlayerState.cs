using System;
using UnityEngine;
using WhereIsMyWife.Player.StateMachine;
using Zenject;

namespace WhereIsMyWife.Player.State
{
    public class PlayerState 
    {
        protected PlayerState(PlayerStateMachine.PlayerState stateKey)
        {
            StateKey = stateKey;
            NextState = stateKey;
        }
        
        [Inject] protected IPlayerStateInput _playerStateInput;

        protected virtual void SubscribeToObservables()
        {
            throw new NotImplementedException();
        }

        protected virtual void UnsubscribeToObservables()
        {
            throw new NotImplementedException();
        }

        public virtual void EnterState()
        {
            NextState = StateKey;
            SubscribeToObservables();
        }

        public virtual void ExitState()
        {
            UnsubscribeToObservables();
        }

        public virtual void UpdateState() { }

        public virtual void FixedUpdateState() { }
        public PlayerStateMachine.PlayerState NextState { get; protected set; }
        public PlayerStateMachine.PlayerState StateKey { get; private set; }
        public virtual void OnTriggerEnter2D(Collider2D other) { }
        public virtual void OnTriggerStay2D(Collider2D other) { }
        public virtual void OnTriggerExit2D(Collider2D other) { }
    }

}