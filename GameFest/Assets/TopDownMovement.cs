using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    public float Speed;

    Vector2 _movementInput;
    Rigidbody2D m_Rigidbody;
    Animator _animator;
    string _currentAnimation = "Idle";

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Move the Rigidbody downwards constantly at the speed you define (the green arrow axis in Scene view)
        m_Rigidbody.velocity = transform.up * Speed * _movementInput.y;

        //rotate the sprite about the Z axis in the positive direction
        transform.Rotate(new Vector3(0, 0, 250) * Time.deltaTime * _movementInput.x, Space.World);
    }

    internal void SetMovement(Vector2 movement)
    {
        _movementInput = movement;

        string trigger = "Idle";
        if(Math.Abs(movement.y) > 0.1f)
        {
            trigger = "Walk";
        }

        if(trigger != _currentAnimation)
        {
            _animator.ResetTrigger("Walk");
            _animator.ResetTrigger("Idle");
            _animator.SetTrigger(trigger);
            _currentAnimation = trigger;
        }
    }
}
