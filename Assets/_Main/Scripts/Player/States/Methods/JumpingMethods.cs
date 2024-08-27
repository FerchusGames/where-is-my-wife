using UnityEngine;
using WhereIsMyWife.Managers;
using WhereIsMyWife.Managers.Properties;
using Zenject;

public interface IJumpingMethods
{
    public float GetJumpForce(float currentVelocityY);
}

public class JumpingMethods : IJumpingMethods
{
    [Inject] private IPlayerStateIndicator _stateIndicator;
    
    [Inject] private IPlayerJumpProperties _jumpProperties;


    public float GetJumpForce(float currentVelocityY)
    {
        float force = _jumpProperties.ForceMagnitude;

        if (currentVelocityY < 0)
        {
            force -= currentVelocityY; // To always jump the same amount.
        }

        return force;
    }
}
