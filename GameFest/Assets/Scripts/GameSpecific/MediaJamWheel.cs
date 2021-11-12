using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the behaviour of the carts
/// </summary>
public class MediaJamWheel/* : MonoBehaviour*/
{
    public Transform Platform;
    public SpriteRenderer Wheel;
    public SpriteRenderer WheelIcon;

    public float LeftPositionX;
    public float RightPositionX;

    bool _playerOnPlatform;
    float _targetPositionX;
    const float MOVE_SPEED = 2f;

    /// <summary>
    /// Called once per frame
    /// </summary>
    void Update()
    {
        if (_playerOnPlatform && Platform.localPosition.x < _targetPositionX)
        {
            Platform.Translate(new Vector3(MOVE_SPEED * Time.deltaTime, 0, 0));
        }
        else if (!_playerOnPlatform && Platform.localPosition.x > LeftPositionX)
        {
            Platform.Translate(new Vector3(-MOVE_SPEED * Time.deltaTime, 0, 0));
        }
    }

    /// <summary>
    /// CHecks whether the platform should move
    /// </summary>
    /// <param name="joystickPosition">The position the joystick is in</param>
    public void OnMove(Vector2 joystickPosition)
    {
        if (!_playerOnPlatform) return;

        // is it moving clockwise?
        if (NextZone_(joystickPosition))
        {
            _targetPositionX += 1f;
            if (_targetPositionX > RightPositionX)
                _targetPositionX = RightPositionX;
        }
        // is it moving anti-clockwise?
        else if (PreviousZone_(joystickPosition))
        {
            _targetPositionX -= 1f;
            if (_targetPositionX < LeftPositionX)
                _targetPositionX = LeftPositionX;
        }
    }

    /// <summary>
    /// Check if the joystick is moving anti-clockwise
    /// </summary>
    /// <param name="joystickPosition">Position of the joystick</param>
    /// <returns>If the joystick is moving anti-clockwise</returns>
    private bool PreviousZone_(Vector2 joystickPosition)
    {
        // TODO: Implement this check
        return false;
    }

    /// <summary>
    /// Check if the joystick is moving clockwise
    /// </summary>
    /// <param name="joystickPosition">Position of the joystick</param>
    /// <returns>If the joystick is moving clockwise</returns>
    private bool NextZone_(Vector2 joystickPosition)
    {
        // TODO: Implement this check
        return false;
    }

    /// <summary>
    /// Triggered when the player lands on the platform
    /// </summary>
    public void PlayerLanded()
    {
        _playerOnPlatform = true;

        StopAllCoroutines();

        // stop the platform from resetting
        _targetPositionX = Platform.localPosition.x;
    }

    /// <summary>
    /// Triggered when the player leaves the platform
    /// </summary>
    public void PlayerLeft()
    {
        StartCoroutine(ReturnToStart_());
    }

    /// <summary>
    /// Waits briefly, then moves the player back to the start
    /// </summary>
    IEnumerator ReturnToStart_()
    {
        yield return new WaitForSeconds(1.5f);

        // move platform to the start
        _playerOnPlatform = false;
        _targetPositionX = LeftPositionX;
    }
}



