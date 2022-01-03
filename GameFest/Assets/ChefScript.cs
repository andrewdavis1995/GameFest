using System;
using UnityEngine;

enum ChefCameraMovement { None, Left, Right };

/// <summary>
/// Controls a player/chef
/// </summary>
public class ChefScript : MonoBehaviour
{
    // constants
    const float CAMERA_SPEED = 30;
    const float HAND_MOVE_SPEED = 5f;
    const float HAND_MOVE_LEFT = 1.5f;
    const float HAND_MOVE_RIGHT = 23.3f;
    const float HAND_MOVE_TOP = 7.5f;
    const float HAND_MOVE_BOTTOM = -3f;

    // components
    public SelectionHand SelectionHand;
    public Transform Plate;
    public Camera ChefCamera;
    public BurgerScript[] Burgers;

    // camera config
    public float CameraPositionLeft;
    public float CameraPositionRight;

    // status variables
    ChefCameraMovement _cameraMoving;

    // Update is called once per frame
    void Update()
    {
        MoveCamera_();
    }

    /// <summary>
    /// Moves the camera if it is specified
    /// </summary>
    private void MoveCamera_()
    {
        // if the camera is moving to the left
        if (_cameraMoving == ChefCameraMovement.Left)
        {
            // move to the left
            ChefCamera.transform.Translate(new Vector3(-CAMERA_SPEED * Time.deltaTime, 0, 0));

            // stop at end
            if (ChefCamera.transform.localPosition.x < CameraPositionLeft)
            {
                _cameraMoving = ChefCameraMovement.None;
            }
        }
        // if the camera is moving to the right
        else if (_cameraMoving == ChefCameraMovement.Right)
        {
            // move to the right
            ChefCamera.transform.Translate(new Vector3(CAMERA_SPEED * Time.deltaTime, 0, 0));

            // stop at end
            if (ChefCamera.transform.localPosition.x > CameraPositionRight)
            {
                _cameraMoving = ChefCameraMovement.None;
            }
        }
    }

    /// <summary>
    /// Starts moving the camera to the left
    /// </summary>
    public void CameraLeft_()
    {
        _cameraMoving = ChefCameraMovement.Left;
    }

    /// <summary>
    /// Starts moving the camera to the right
    /// </summary>
    public void CameraRight_()
    {
        _cameraMoving = ChefCameraMovement.Right;
    }

    /// <summary>
    /// Move the hand in the specified direction
    /// </summary>
    /// <param name="x">The X direction to move in</param>
    /// <param name="y">The Y direction to move in</param>
    internal void MoveHand(float x, float y)
    {
        var currentPosition = SelectionHand.transform.localPosition;

        // check if we are in the bounds of the zone
        var xMove = ((x < 0 && currentPosition.x > HAND_MOVE_LEFT) || (x > 0 && currentPosition.x < HAND_MOVE_RIGHT)) ? x : 0;
        var yMove = ((y < 0 && currentPosition.y > HAND_MOVE_BOTTOM) || (y > 0 && currentPosition.y < HAND_MOVE_TOP)) ? y : 0;

        // move the hand
        SelectionHand.transform.Translate(new Vector2(xMove, yMove) * Time.deltaTime * HAND_MOVE_SPEED);
    }

    /// <summary>
    /// Sets the functions to call when the player collides with a trigger
    /// </summary>
    /// <param name="triggerEnter">Function to call when player enters the trigger</param>
    /// <param name="triggerExit">Function to call when player leaves the trigger</param>
    internal void AddItemSelectionCallbacks(Action<CookingSelectionObject> triggerEnter, Action<CookingSelectionObject> triggerExit)
    {
        // assign callbacks
        SelectionHand.AddItemSelectionCallbacks(triggerEnter, triggerExit);
    }
}
