using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player input for the "Shop Drop" mini game
/// </summary>
public class ShopDropInputHandler : GenericInputHandler
{
    // the paddles assigned to this player
    PaddleScript[] _paddles;

    // other linked objects
    Transform _playerTransform;
    Transform _trolley;

    // links to other scripts
    List<ShopDropBallScript> _foodCollected = new List<ShopDropBallScript>();
    PlayerAnimation _animator;

    // how many coroutines are running (for celebration animation)
    int _animationCoroutines = 0;

    /// <summary>
    /// Finds the paddles assigned to this player
    /// </summary>
    public void AssignPaddles(int playerIndex)
    {
        // fetch paddles associated with the player
        _paddles = FindObjectsOfType<PaddleScript>().Where(t => t.gameObject.name == "PADDLE_" + playerIndex).ToArray();

        // link to trolley, and show it
        _trolley = ShopDropController.Instance.Trolleys[playerIndex];
        _trolley.gameObject.SetActive(true);
    }

    /// <summary>
    /// When food lands in one of the players slots
    /// </summary>
    /// <param name="ball">The ball that landed</param>
    internal void FoodCollected(ShopDropBallScript ball)
    {
        // add to the list
        _foodCollected.Add(ball);

        // add points to player
        AddPoints(ball.Points);

        // put in trolley
        ball.MoveToTrolley(_trolley);

        // trigger the celebration animation
        StartCoroutine(Celebrate());
    }

    /// <summary>
    /// Returns the list of food collected by this player
    /// </summary>
    /// <returns>The list of food</returns>
    public List<ShopDropBallScript> GetFood()
    {
        return _foodCollected;
    }

    /// <summary>
    /// Celebration animation for one second, then reset
    /// </summary>
    /// <returns></returns>
    IEnumerator Celebrate()
    {
        _animationCoroutines++;

        // start celebrating
        _animator.SetAnimation("Celebrate");
        yield return new WaitForSeconds(1f);
        _animationCoroutines--;

        // if there are no other coroutines waiting, reset to Idle
        if (_animationCoroutines == 0)
            _animator.SetAnimation("Idle");
    }

    /// <summary>
    /// When the player moves their controls (joystick or errors)
    /// </summary>
    /// <param name="ctx">The context of the movement</param>
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        if (!ShopDropController.Instance.GameRunning()) return;

        // the vector of the movement input from the user
        var movement = ctx.ReadValue<Vector2>();

        // update the rotation of each paddle
        foreach (var paddle in _paddles)
        {
            paddle.SetMovement(movement.x);
        }
    }

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerName">The index of the selected character</param>
    public override void Spawn(Transform prefab, Vector2 position, int characterIndex, string playerName)
    {
        // create the player display
        _playerTransform = Instantiate(prefab, position, Quaternion.identity);

        // set the height of the object
        SetHeight(_playerTransform, characterIndex);
        _playerTransform.localScale /= 1.5f;

        // use the correct animation controller
        SetAnimation(_playerTransform, characterIndex);

        // get animator and set player to idle
        _animator = _playerTransform.GetComponent<PlayerAnimation>();
    }
}
