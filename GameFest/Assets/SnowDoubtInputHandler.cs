using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnowDoubtInputHandler : GenericInputHandler
{
    TopDownMovement _movement;

    // Start is called before the first frame update
    void Start()
    {
        _movement = SnowDoubtController.Instance.PlayerMovements[0];
    }

    // Update is called once per frame
    void Update()
    {
        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            x = 1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            x = -1;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            y= 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            y= -1;
        }

        _movement.SetMovement(new Vector2(x, y));
    }

    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {

    }
}
