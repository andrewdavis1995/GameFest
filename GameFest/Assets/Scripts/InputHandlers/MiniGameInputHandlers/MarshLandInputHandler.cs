using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MarshLandInputAction { Triangle, Circle, Cross, Square, L1, L2, R1, R2 }

public class MarshLandInputHandler : GenericInputHandler
{
    List<MarshLandInputAction> _actions = new List<MarshLandInputAction>();
    PlayerJumper _jumpScript;
    bool _inWater = false;
    int _playerIndex;

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerIndex">The index of the player</param>
    /// <returns>The transform that was created</returns>
    public override Transform Spawn(Transform prefab, Vector2 position, int characterIndex, string playerName, int playerIndex)
    {
        _playerIndex = playerIndex;

        // create the player display
        var spawned = Instantiate(prefab, position, Quaternion.identity);

        // set the height of the object
        SetHeight(spawned, characterIndex);

        // use the correct animation controller
        SetAnimation(spawned, characterIndex);

        // set the layers
        spawned.gameObject.layer = LayerMask.NameToLayer("Player" + (playerIndex+1));
        spawned.GetComponent<SpriteRenderer>().sortingLayerName = "Player" + (playerIndex+1);

        // get the jump script
        _jumpScript = spawned.GetComponent<PlayerJumper>();

        return spawned;
    }

    /// <summary>
    /// Creates a list of actions for the player to jump to the next marshmallow
    /// </summary>
    /// <param name="numMarshmallows">How many actions are required</param>
    public void SetActionList(int numMarshmallows)
    {
        Debug.Log(numMarshmallows + "marshers");

        for(int i = 0; i < numMarshmallows; i++)
        {
            // add a random action
            var action = UnityEngine.Random.Range(0, Enum.GetValues(typeof(MarshLandInputAction)).Length);
            _actions.Add((MarshLandInputAction)action);
        }

        if (_actions.Count > 0)
            MarshLandController.Instance.SetAction(_playerIndex, _actions.First());
        else
            MarshLandController.Instance.HideDisplay(_playerIndex);
    }

    /// <summary>
    /// Jump to the next platform
    /// </summary>
    void Jump_()
    {
        if (_jumpScript.OnGround())
        {
            _actions.RemoveAt(0);
            _jumpScript.Jump();

            if (_actions.Count > 0)
                MarshLandController.Instance.SetAction(_playerIndex, _actions.First());
            else
                MarshLandController.Instance.HideDisplay(_playerIndex);
        }
    }

    /// <summary>
    /// Fall from the current platform
    /// </summary>
    void Fall_()
    {

    }

    /// <summary>
    /// Checks if the specified input matches the first one in the list
    /// </summary>
    /// <param name="action">The entered action</param>
    /// <returns>Whether it is a match</returns>
    bool MatchesTargetAction_(MarshLandInputAction action)
    {
        return action == _actions.First();
    }

    /// <summary>
    /// A button has been pressed. Handle it
    /// </summary>
    /// <param name="action">The entered action</param>
    void InputReceived_(MarshLandInputAction action)
    {
        // if in the game, not frozen
        if (!_inWater)
        {
            if (_jumpScript.OnGround())
            {
                // check for a match
                if (MatchesTargetAction_(action))
                    Jump_();
                else
                    Fall_();
            }
        }
        else
        {
            // try to recover
        }
    }

    #region Input Handlers - all pass through to InputRecieved_
    public override void OnCircle()
    {
        InputReceived_(MarshLandInputAction.Circle);
    }

    public override void OnCross()
    {
        InputReceived_(MarshLandInputAction.Cross);
    }

    public override void OnSquare()
    {
        InputReceived_(MarshLandInputAction.Square);
    }

    public override void OnTriangle()
    {
        InputReceived_(MarshLandInputAction.Triangle);
    }

    public override void OnL1()
    {
        InputReceived_(MarshLandInputAction.L1);
    }

    public override void OnL2()
    {
        InputReceived_(MarshLandInputAction.L2);
    }

    public override void OnR1()
    {
        InputReceived_(MarshLandInputAction.R1);
    }

    public override void OnR2()
    {
        InputReceived_(MarshLandInputAction.R2);
    }
#endregion

}