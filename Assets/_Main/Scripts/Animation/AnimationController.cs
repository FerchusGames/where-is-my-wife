using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace WhereIsMyWife.Controllers
{
    public interface IAnimationControllerInput
    {
        IObservable<string> PlayAnimationStateAction { get; }
    }

    public class AnimationController : MonoBehaviour
    {
        [Inject] private IAnimationControllerInput _animationControllerInput;

        [SerializeField] private Animator _animator;

        private void Start()
        {
            _animationControllerInput.PlayAnimationStateAction.Subscribe(PlayAnimationState);
        }

        private void PlayAnimationState(string newState)
        {
            _animator.Play(newState);
        }
    }
}

