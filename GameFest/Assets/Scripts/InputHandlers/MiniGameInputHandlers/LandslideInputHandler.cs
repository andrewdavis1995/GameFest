using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LandslideInputHandler : GenericInputHandler
{
    private PlayerClimber _climber;
    int _powerUpLevel;

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerIndex">The index of the player</param>
    /// <param name="id">ID of the profile</param>
    /// <returns>The transform that was created</returns>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex, Guid id)
    {
        base.Spawn(prefab, position, characterIndex, playerName, playerIndex, id);

        // create the player display
        var player = Instantiate(prefab, position, Quaternion.identity);

        // set the height of the object
        SetHeight(player, characterIndex);

        // use the correct animation controller
        SetAnimation(player, characterIndex);

        // get the jump script
        _climber = player.GetComponent<PlayerClimber>();
        _climber.Initialise(playerIndex, playerName, IncreasePowerUpLevel, ClearPowerUpLevel, DecreasePowerUpLevel);

        return player;
    }

    /// <summary>
    /// When cross is pressed
    /// </summary>
    public override void OnCross()
    {
        _climber.Jump();
    }

    /// <summary>
    /// When square is pressed
    /// </summary>
    public override void OnSquare()
    {
        _climber.RecoveryKeyPressed();
    }

    /// <summary>
    /// When triangle is pressed
    /// </summary>
    public override void OnTriangle()
    {
        PerformPowerUpAction_(_powerUpLevel);
    }

    /// <summary>
    /// When the move event is triggered
    /// </summary>
    /// <param name="ctx">The context of the movement</param>
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        // move the player
        _climber.SetMovementVector(ctx.ReadValue<Vector2>().x);
    }

    /// <summary>
    /// Performs the action based on the power up level
    /// </summary>
    /// <param name="powerUpLevel">The power up level the player has reached</param>
    private void PerformPowerUpAction_(int powerUpLevel)
    {
        switch (powerUpLevel)
        {
            case 0:
                // do nothing
                break;
            case 1:
                // spawn a few small rocks
                LandslideController.Instance.RockBarageSmall(GetPlayerIndex());
                break;
            case 2:
                // spawn a mixture of small and bigger rocks
                LandslideController.Instance.RockBarage(GetPlayerIndex());
                break;
            default:
                // spawn a giant rock
                LandslideController.Instance.SpawnGiantRock(GetPlayerIndex());
                break;
        }

        // go back to zero
        ClearPowerUpLevel();
    }

    /// <summary>
    /// Moves the power up level up by 1
    /// </summary>
    void IncreasePowerUpLevel()
    {
        _powerUpLevel++;
    }

    /// <summary>
    /// Sets the power up layer back to zero
    /// </summary>
    void ClearPowerUpLevel()
    {
        _powerUpLevel = 0;
    }

    /// <summary>
    /// Moves the power up layer down by 1S
    /// </summary>
    void DecreasePowerUpLevel()
    {
        if (_powerUpLevel > 0)
            _powerUpLevel--;
    }

    /// <summary>
    /// Mark the player as completed
    /// </summary>
    internal void Finish()
    {
        _climber.Complete();
    }

    /// <summary>
    /// Checks if the player is complete
    /// </summary>
    /// <returns>Whether the player is complete</returns>
    internal bool IsComplete()
    {
        return _climber.IsComplete();
    }

    /// <summary>
    /// Sets the animation of the player
    /// </summary>
    /// <param name="animation">The trigger to set</param>
    internal void SetAnimationTrigger(string animation)
    {
        _climber.SetAnimationTrigger(animation);
    }

    /// <summary>
    /// Gets the position at which the player ended the game
    /// </summary>
    /// <returns>The position of the player</returns>
    internal Vector2 GetEndPosition()
    {
        return _climber.transform.position;
    }
}
