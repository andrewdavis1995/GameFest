using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class DrinkSlideInputHandler : GenericInputHandler
{
    List<Vector2> _joystickReadings = new List<Vector2>();
    int NUM_READINGS = 5;
    bool _isActive = false;
    bool _canFire = false;
    float _spin = 0f;
    
    public void IsActive(bool state)
    {
        _isActive = state;
        _canFire = state;
    }

    public void Initialise(InputDevice device)
    {
        NUM_READINGS = device is DualShockGamepad ? 5 : 2;
    }

    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        _joystickReadings.Add(ctx.ReadValue<Vector2>());

        if (_joystickReadings.Count > NUM_READINGS)
            _joystickReadings.RemoveAt(0);

        // calculate angle - flip by -1 to point opposite direction
        float angle = Mathf.Atan2(_joystickReadings[0].y _joystickReadings[0].x) * 180 * -1 * Mathf.PI - 90;
        if(_isActive)
            DrinkSlideController.Instance.UpdatePointer(GetPlayerIndex(), angle);
                
        if (ctx.ReadValue<Vector2>().y >= -0.05f && _joystickReadings.Count == NUM_READINGS)
        {
            if(_canFire)
            {   
                _canFire = false;
                _isActive = false;
                // calculate power
                float powerMultiplier = Math.Abs(_joystickReadings[0].x) + Math.Abs(_joystickReadings[0].y);
                // pass to controller (with power and angle)
                DrinkSlideController.Instance.Fire(GetPlayerIndex(), angle, powerMultiplier);
            }
        }
    }

    public override void OnMoveRight(InputAction.CallbackContext ctx)
    {        
        _spin = ctx.ReadValue<Vector2>();
    }
}
