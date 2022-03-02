using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class DrinkSlideInputHandler : GenericInputHandler
{
    List<Vector2> _joystickReadings = new List<Vector2>();
    int NUM_READINGS = 5;

    public void Initialise(InputDevice device)
    {
        NUM_READINGS = device is DualShockGamepad ? 5 : 2;
    }

    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        _joystickReadings.Add(ctx.ReadValue<Vector2>());

        if (_joystickReadings.Count > NUM_READINGS)
            _joystickReadings.RemoveAt(0);

        if (ctx.ReadValue<Vector2>().y >= -0.05f && _joystickReadings.Count == NUM_READINGS)
        {
            Debug.Log("IT WAS: " + _joystickReadings[0].y);
            // TODO: calculate angle
            // TODO: pass to controller (with power and angle)
        }
    }
}
