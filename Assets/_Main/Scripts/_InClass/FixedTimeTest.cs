using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FixedTimeTest : MonoBehaviour
{
    [SerializeField] private float _movementTime = 1f;
    [FormerlySerializedAs("_target")] [SerializeField] private Transform _movementTarget;
    
    private CharacterController _characterController;
    
    private Vector3 _movementVector;

    private int _framesToMove;
    private int _traversedFrames;
    
    private bool _isMoving = false;

    private Vector3 _moveVector;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _framesToMove = (int)Math.Ceiling(_movementTime / Time.fixedDeltaTime);
        Debug.Log(_framesToMove);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartMovement();
        }
    }

    private void StartMovement()
    {
        if (!_isMoving)
        {
            _isMoving = true;
            _traversedFrames = 0;
            _moveVector = (_movementTarget.position - transform.position) * Time.fixedDeltaTime;
        }
    }

    private void FixedUpdate()
    {
        AutomaticMovement();
    }

    private void AutomaticMovement()
    {
        if (!_isMoving) return;
        
        _characterController.Move(_moveVector);
        _traversedFrames++;
        
        if (_traversedFrames >= _framesToMove)
        {
            _isMoving = false;
        }
    }
    

}
