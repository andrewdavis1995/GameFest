using System;
using System.Collections;
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
    bool _active = false;

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
        spawned.gameObject.layer = LayerMask.NameToLayer("Player" + (playerIndex+1) + "A");
        spawned.GetComponent<SpriteRenderer>().sortingLayerName = "Player" + (playerIndex + 1);
        spawned.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        // get the jump script
        _jumpScript = spawned.GetComponent<PlayerJumper>();
        _jumpScript.SetCollisionCallback(CheckFinish_);

        return spawned;
    }

    /// <summary>
    /// Checks if the current player can move
    /// </summary>
    /// <returns>Whether the player can move</returns>
    public bool Active()
    {
        return _active;
    }

    /// <summary>
    /// Sets if the current player can move
    /// </summary>
    /// <param name="active">If the player is active</param>
    public void Active(bool active)
    {
        _active = active;
    }

    /// <summary>
    /// Checks if the player has collided with a finish platform
    /// </summary>
    /// <param name="collision"></param>
    void CheckFinish_(Collision2D collision)
    {
        // if the new platform is the finish
        if (collision.gameObject.name.Contains("End"))
        {
            // disable the player
            Active(false);

            // celebrate
            StartCoroutine(Celebrate());
        }
    }

    /// <summary>
    /// Celebrates at the finish line
    /// </summary>
    private IEnumerator Celebrate()
    {
        yield return new WaitForSeconds(0.6f);
        _jumpScript.SetAnimation("Celebrate");
    }

    /// <summary>
    /// Creates a list of actions for the player to jump to the next marshmallow
    /// </summary>
    /// <param name="numMarshmallows">How many actions are required</param>
    public void SetActionList(int numMarshmallows)
    {
        for(int i = 0; i < numMarshmallows; i++)
        {
            // add a random action
            var action = UnityEngine.Random.Range(0, Enum.GetValues(typeof(MarshLandInputAction)).Length);
            _actions.Add((MarshLandInputAction)action);
        }

        // move to the next action
        NextAction_();

        Active(true);
    }

    /// <summary>
    /// Move to the next action in the list
    /// </summary>
    void NextAction_()
    {
        // if there are any left, usse this one
        if (_actions.Count > 0)
            MarshLandController.Instance.SetAction(_playerIndex, _actions.First());
        else
        {
            // otherwise, done
            MarshLandController.Instance.HideDisplay(_playerIndex);

            // disable the player
            Active(false);

            // celebrate
            StartCoroutine(Celebrate());
        }
    }

    /// <summary>
    /// Jump to the next platform
    /// </summary>
    void Jump_()
    {
        // only jump if on ground
        if (_jumpScript.OnGround())
        {
            // move to next action
            _actions.RemoveAt(0);

            // move to next platform
            _jumpScript.Jump();

            NextAction_();
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
        // if not playing yet/finished, just stop
        if (!_active) return;

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