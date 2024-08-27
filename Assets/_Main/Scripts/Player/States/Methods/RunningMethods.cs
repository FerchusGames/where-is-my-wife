using UnityEngine;
using WhereIsMyWife.Managers;
using WhereIsMyWife.Managers.Properties;
using Zenject;

public interface IRunningMethods
{
    public float GetRunAcceleration(float runDirection, float currentVelocityX);
}

public class RunningMethods : IRunningMethods
{
    [Inject] private IPlayerStateIndicator _stateIndicator;
    
    [Inject] private IPlayerMovementProperties _movementProperties;
    [Inject] private IPlayerJumpProperties _jumpProperties;
    
    private float _accelerationRate = 0;
    private float _targetSpeed = 0;
    
    public float GetRunAcceleration(float runDirection, float currentVelocityX)
    {
        _targetSpeed = runDirection * _movementProperties.RunMaxSpeed;
        
        UpdateAccelerationRate();

        return GetTargetAndCurrentSpeedDifference(currentVelocityX) * _accelerationRate;
    }
    
    private void UpdateAccelerationRate()
    {
        UpdateBaseAccelerationRate();
        AddJumpHangMultipliers();
    }
    
    private void UpdateBaseAccelerationRate()
    {
        if (_stateIndicator.IsOnGround())
        {
            _accelerationRate = GetGroundAccelerationRate();
        }

        else
        {
            _accelerationRate = GetAirAccelerationRate();
        }
    }
    
    private float GetGroundAccelerationRate()
    {
        if (IsAccelerating())
        {
            return _movementProperties.RunAccelerationRate;
        }

        return _movementProperties.RunDecelerationRate;
    }
    
    private float GetAirAccelerationRate()
    {
        if (IsAccelerating())
        {
            return _movementProperties.RunAccelerationRate 
                   * _movementProperties.AirAccelerationMultiplier;
        }
            
        return _movementProperties.RunDecelerationRate 
               * _movementProperties.AirDecelerationMultiplier;
    }
    
    private bool IsAccelerating()
    {
        return Mathf.Abs(_targetSpeed) > 0.01f;
    }
    
    private void AddJumpHangMultipliers()
    {
        if (_stateIndicator.IsInJumpHang())
        {
            _accelerationRate *= _jumpProperties.HangAccelerationMultiplier;
            _targetSpeed *= _jumpProperties.HangMaxSpeedMultiplier;
        }
    }
    
    private float GetTargetAndCurrentSpeedDifference(float currentVelocityX)
    {
        return _targetSpeed - currentVelocityX;
    }
}
