using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    const float SPATULA_MOVE_LEFT = -4f;
    const float SPATULA_MOVE_RIGHT = 4f;
    const float SPATULA_MOVE_TOP = 2.25f;
    const float SPATULA_MOVE_BOTTOM = -4.5f;

    // components
    public SelectionHand SelectionHand;
    public SpriteRenderer SelectionHandColour;
    public SelectionHand SelectionSpatula;
    public Transform Plate;
    public Transform ChoppingBoard;
    public SpriteRenderer ChoppingBoardColour;
    public Transform Knife;
    public Camera ChefCamera;
    public BurgerScript[] Burgers;
    public CookingSelectionObject[] BreadOptions;
    public SpriteRenderer ChopBunRendererTop;
    public SpriteRenderer ChopBunRendererBottom;
    public SpriteRenderer TopBun;
    public Transform BurgerTray;
    public Transform Bin;
    public Transform BinPatty;
    public TextMesh BinText;
    public TextMesh BinPattyText;
    public Transform OrderHolder;
    public Transform OrderList;
    public BurgerOrderDisplayScript[] OrderDisplayElements;
    public GameObject ConfirmPopup;
    public Text ConfirmPopupText;

    // config
    public float BurgerTrayY;
    public float BinY;

    /// <summary>
    /// Displays the specified orders in the UI
    /// </summary>
    /// <param name="list">Orders to display</param>
    internal void DisplayOrders(List<CustomerOrder> list)
    {
        for(int i = 0; i < list.Count && i < OrderDisplayElements.Length; i++)
        {
            OrderDisplayElements[i].Initialise(list[i].GetRequest(), list[i].GetCustomerName());
        }
    }

    // camera config
    public float CameraPositionLeft;
    public float CameraPositionRight;

    // status variables
    ChefCameraMovement _cameraMoving;
    float _plateX;

    // Called once at startup
    private void Start()
    {
        _plateX = Plate.localPosition.x;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera_();
    }

    /// <summary>
    /// X-pposition of the plate
    /// </summary>
    /// <returns>The position of the plate</returns>
    public float PlatePosition()
    {
        return _plateX;
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
        SelectionSpatula.gameObject.SetActive(true);
        SelectionHand.gameObject.SetActive(false);
    }

    /// <summary>
    /// Starts moving the camera to the right
    /// </summary>
    public void CameraRight_()
    {
        _cameraMoving = ChefCameraMovement.Right;
        SelectionSpatula.gameObject.SetActive(false);
        SelectionHand.gameObject.SetActive(true);
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
    /// Move the hand in the specified direction
    /// </summary>
    /// <param name="x">The X direction to move in</param>
    /// <param name="y">The Y direction to move in</param>
    internal void MoveSpatula(float x, float y)
    {
        var currentPosition = SelectionSpatula.transform.localPosition;

        // check if we are in the bounds of the zone
        var xMove = ((x < 0 && currentPosition.x > SPATULA_MOVE_LEFT) || (x > 0 && currentPosition.x < SPATULA_MOVE_RIGHT)) ? x : 0;
        var yMove = ((y < 0 && currentPosition.y > SPATULA_MOVE_BOTTOM) || (y > 0 && currentPosition.y < SPATULA_MOVE_TOP)) ? y : 0;

        // move the hand
        SelectionSpatula.transform.Translate(new Vector2(xMove, yMove) * Time.deltaTime * HAND_MOVE_SPEED);
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
        SelectionSpatula.AddItemSelectionCallbacks(triggerEnter, triggerExit);
    }
}
