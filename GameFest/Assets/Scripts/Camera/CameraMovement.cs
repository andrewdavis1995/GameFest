using System;
using UnityEngine;

/// <summary>
/// Enum for the movement in the x-axis
/// </summary>
enum MovementDirectionX { None, Left, Right }
/// <summary>
/// Enum for the movement in the y-axis
/// </summary>
enum MovementDirectionY { None, Up, Down }
/// <summary>
/// Enum for the zooming
/// </summary>
enum ZoomDirection { None, In, Out }

public class CameraMovement : MonoBehaviour
{
    // directions of movements
    [SerializeField]
    MovementDirectionX _xMovement = MovementDirectionX.None;
    [SerializeField]
    MovementDirectionY _yMovement = MovementDirectionY.None;
    [SerializeField]
    ZoomDirection _zoomDirection = ZoomDirection.None;

    // how quickly to move in each direction
    [SerializeField]
    float _xSpeed = 0;
    [SerializeField]
    float _ySpeed = 0;
    [SerializeField]
    float _zoomSpeed = 0;
    public float SpeedAdjustment = 2f;

    // where it should end up
    [SerializeField]
    Vector2 _targetPosition = new Vector2(0, 0);
    [SerializeField]
    float _targetZoom;

    // link to the camera object
    public Camera TheCamera;

    // function to call when no more movement
    Action _callback;

    // needed so the callback is only called once
    [SerializeField]
    bool _callbackCalled = true;

    void Update()
    {
        if (_callbackCalled && _callback != null) return;

        // update the position and zoom where appropriate
        MoveX_();
        MoveY_();
        Zoom_();

        // if there is no movement ongoing, and the callback has yet to be called, perform the callback action
        if (_xMovement == MovementDirectionX.None
            && _yMovement == MovementDirectionY.None
            && _zoomDirection == ZoomDirection.None)
            PerformCallback_();
    }

    /// <summary>
    /// Moves the camera left/right until it reaches the target
    /// </summary>
    void MoveX_()
    {
        switch (_xMovement)
        {
            // move left
            case MovementDirectionX.Left:
                transform.localPosition -= new Vector3(_xSpeed * Time.deltaTime * SpeedAdjustment, 0, 0);
                // stop when target reached
                if (transform.localPosition.x < _targetPosition.x)
                {
                    transform.localPosition = new Vector3(_targetPosition.x, transform.localPosition.y, transform.localPosition.z);
                    _xMovement = MovementDirectionX.None;
                }
                break;
            // move right
            case MovementDirectionX.Right:
                transform.localPosition += new Vector3(_xSpeed * Time.deltaTime * SpeedAdjustment, 0, 0);
                // stop when target reached
                if (transform.localPosition.x > _targetPosition.x)
                {
                    transform.localPosition = new Vector3(_targetPosition.x, transform.localPosition.y, transform.localPosition.z);
                    _xMovement = MovementDirectionX.None;
                }
                break;
        }
    }

    /// <summary>
    /// Moves the camera up/down until it reaches the target
    /// </summary>
    void MoveY_()
    {
        switch (_yMovement)
        {
            // move up
            case MovementDirectionY.Up:
                transform.localPosition += new Vector3(0, _ySpeed * Time.deltaTime * SpeedAdjustment, 0);
                // stop when target reached
                if (transform.localPosition.y > _targetPosition.y)
                {
                    _yMovement = MovementDirectionY.None;
                    transform.localPosition = new Vector3(transform.localPosition.x, _targetPosition.y, transform.localPosition.z);
                }
                break;
            // move down
            case MovementDirectionY.Down:
                transform.localPosition -= new Vector3(0, _ySpeed * Time.deltaTime * SpeedAdjustment, 0);
                // stop when target reached
                if (transform.localPosition.y < _targetPosition.y)
                {
                    _yMovement = MovementDirectionY.None;
                    transform.localPosition = new Vector3(transform.localPosition.x, _targetPosition.y, transform.localPosition.z);
                }
                break;
        }
    }

    /// <summary>
    /// Zooms the camera in/out until it reaches the target
    /// </summary>
    void Zoom_()
    {
        switch (_zoomDirection)
        {
            // zoom in
            case ZoomDirection.In:
                TheCamera.orthographicSize -= _zoomSpeed * Time.deltaTime * SpeedAdjustment;
                // stop when target reached
                if (TheCamera.orthographicSize < _targetZoom)
                {
                    _zoomDirection = ZoomDirection.None;
                    TheCamera.orthographicSize = _targetZoom;
                }
                break;
            // zoom out
            case ZoomDirection.Out:
                TheCamera.orthographicSize += _zoomSpeed * Time.deltaTime * SpeedAdjustment;
                // stop when target reached
                if (TheCamera.orthographicSize > _targetZoom)
                {
                    _zoomDirection = ZoomDirection.None;
                    TheCamera.orthographicSize = _targetZoom;
                }
                break;
        }
    }

    /// <summary>
    /// Call the callback, if not already called
    /// </summary>
    void PerformCallback_()
    {
        // check that the callback has not been called already, and that a callback exists
        if (!_callbackCalled && _callback != null)
            _callback();

        _callbackCalled = true;
    }

    /// <summary>
    /// Start the movement
    /// </summary>
    /// <param name="targetPosition">Where to move to (where the camera should end up)</param>
    public void StartMovement(Vector2 targetPosition, float targetZoom)
    {
        // get the differences in X and Y directions
        var xDifference = Math.Abs(targetPosition.x - transform.localPosition.x);
        var yDifference = Math.Abs(targetPosition.y - transform.localPosition.y);
        var zDifference = Math.Abs(targetZoom - TheCamera.orthographicSize);

        // calculate the speed to move at
        var max = (Math.Max(Math.Max(xDifference, yDifference), zDifference));
        _xSpeed = xDifference / max;
        _ySpeed = yDifference / max;
        _zoomSpeed = zDifference / max;

        _targetPosition = targetPosition;
        _targetZoom = targetZoom;

        // get the original movement
        if (_targetPosition.x != transform.localPosition.x)
            _xMovement = _targetPosition.x > transform.localPosition.x ? MovementDirectionX.Right : MovementDirectionX.Left;
        if (_targetPosition.y != transform.localPosition.y)
            _yMovement = _targetPosition.y > transform.localPosition.y ? MovementDirectionY.Up : MovementDirectionY.Down;
        if (_targetZoom != TheCamera.orthographicSize)
            _zoomDirection = _targetZoom < TheCamera.orthographicSize ? ZoomDirection.In : ZoomDirection.Out;

        // set values
        _callbackCalled = false;
    }

    /// <summary>
    /// Sets the function to call once all movement has completed
    /// </summary>
    /// <param name="completionCallback">The function to call</param>
    public void SetCallback(Action completionCallback)
    {
        _callback = completionCallback;
    }
}