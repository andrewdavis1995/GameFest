using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the behaviour of the carts
/// </summary>
public class MediaJamWheel : MonoBehaviour
{
    public Transform Platform;
    public SpriteRenderer Wheel;
    public SpriteRenderer WheelIcon;

    public float LeftPositionX;
    public float RightPositionX;

    bool _playerOnPlatform;
    bool _playerControl;
    float _targetPositionX;
    const float MOVE_SPEED = 3f;
    Vector2 _joystickPosition;

    Vector2[] _spinPositions = new Vector2[] { new Vector2(-1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(0, -1f) };
    int _spinPositionIndex = 0;

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
        if (!_playerControl) return;

        _joystickPosition = joystickPosition;

        // is it moving clockwise?
        if (NextZone_(joystickPosition))
        {
            _targetPositionX += 0.15f;
            if (_targetPositionX > RightPositionX)
                _targetPositionX = RightPositionX;

            Wheel.transform.eulerAngles -= new Vector3(0, 0, 10f);
        }
    }

    /// <summary>
    /// Check if the joystick is moving clockwise
    /// </summary>
    /// <param name="joystickPosition">Position of the joystick</param>
    /// <returns>If the joystick is moving clockwise</returns>
    private bool NextZone_(Vector2 joystickPosition)
    {
        var hit = false;

        var xDiff = Mathf.Abs(joystickPosition.x - _spinPositions[_spinPositionIndex].x);
        var yDiff = Mathf.Abs(joystickPosition.y - _spinPositions[_spinPositionIndex].y);

        // if close enough, progress
        if(xDiff < 0.1f && yDiff < 0.1f)
        {
            hit = true;
            _spinPositionIndex++;
            if(_spinPositionIndex >= _spinPositions.Length)
            {
                _spinPositionIndex = 0;
            }
        }

        return hit;
    }

    /// <summary>
    /// Triggered when the player lands on the platform
    /// </summary>
    public void PlayerLanded()
    {
        _playerOnPlatform = true;
        _playerControl = true;

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
        _playerControl = false;

        yield return new WaitForSeconds(1f);

        _playerOnPlatform = false;

        // move platform to the start
        _targetPositionX = LeftPositionX;
    }
}