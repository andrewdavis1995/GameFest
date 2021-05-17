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

    // the player to be shown at the bottom
    Transform playerTransform_;

    /// <summary>
    /// Finds the paddles assigned to this player
    /// </summary>
    public void AssignPaddles(int playerIndex)
    {
        _paddles = FindObjectsOfType<PaddleScript>().Where(t => t.gameObject.name == "PADDLE_" + playerIndex).ToArray();
    }

    /// <summary>
    /// When the player moves their controls (joystick or errors)
    /// </summary>
    /// <param name="ctx">The context of the movement</param>
    public override void OnMove(InputAction.CallbackContext ctx)
    {
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
        playerTransform_ = Instantiate(prefab, position, Quaternion.identity);

        // set the height of the object
        SetHeight(playerTransform_, characterIndex);

        // use the correct animation controller
        SetAnimation(playerTransform_, characterIndex);
    }
}
