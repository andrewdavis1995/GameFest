using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MarshLandInputAction { Triangle, Circle, Cross, Square, L1, R1 }

public class MarshLandInputHandler : GenericInputHandler
{
    // the button to be used to recover
    const MarshLandInputAction RECOVERY_ACTION = MarshLandInputAction.Cross;

    // links to other scripts
    PlayerJumper _jumpScript;
    PlayerMovement _movement;
    Transform _player;
    MarshmallowScript _currentPlatform;

    // status values
    bool _inWater = false;
    int _playerIndex;
    bool _active = false;
    bool _leftStartPoint = false;
    int _recoveryPressesRemaining = 0;
    bool _walkingOn = false;
    bool _walkingOnComplete = false;
    Action _walkOnCallBack;
    string _playerName;
    List<MarshLandInputAction> _actions = new List<MarshLandInputAction>();

    // called once per frame
    private void Update()
    {
        if (_walkingOn)
        {
            // move to the right
            _movement.Move(new Vector2(1, 0));

            // if reached the end point, stop
            if (_movement.transform.position.x > MarshLandController.Instance.ServingPosition && !_walkingOnComplete)
            {
                StartCoroutine(GetServed_());
            }
        }
    }

    /// <summary>
    /// Returns the name of the player linked to this handler
    /// </summary>
    /// <returns>The player's name</returns>
    public string PlayerName()
    {
        return _playerName;
    }

    /// <summary>
    /// Gets the transform of the player output/display
    /// </summary>
    /// <returns></returns>
    public Transform GetPlayerTransform()
    {
        return _player;
    }

    /// <summary>
    /// Handles the pause, then walk off
    /// </summary>
    IEnumerator GetServed_()
    {
        _walkingOn = false;
        _movement.Move(new Vector2(0, 0));
        _walkingOnComplete = true;
        yield return new WaitForSeconds(2);

        // tell the controller they are done
        _walkOnCallBack?.Invoke();
        _movement.Move(new Vector2(1, 0));
    }

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerIndex">The index of the player</param>
    /// <returns>The transform that was created</returns>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex)
    {
        _playerIndex = playerIndex;
        _playerName = playerName;

        // create the player display
        _player = Instantiate(prefab, position, Quaternion.identity);

        // set the height of the object
        SetHeight(_player, characterIndex);

        // use the correct animation controller
        SetAnimation(_player, characterIndex);

        // get the jump script
        _jumpScript = _player.GetComponent<PlayerJumper>();
        _jumpScript.SetCollisionCallback(CheckFinish_);

        // get the movement script - disable it to stop the animations getting in each others way
        _movement = _player.GetComponent<PlayerMovement>();
        _movement.enabled = false;

        // set the layers
        _player.gameObject.layer = LayerMask.NameToLayer("Player" + (playerIndex + 1) + "A");
        _jumpScript.ColliderB.gameObject.layer = LayerMask.NameToLayer("Player" + (playerIndex + 1) + "C");

        // sprite renderer updates
        var sr = _player.GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Player" + (playerIndex + 1) + "B";
        sr.sortingOrder = 1;
        sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        return _player;
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
        _currentPlatform = collision.gameObject.GetComponent<MarshmallowScript>();

        // if the new platform is the finish
        if (collision.gameObject.name.Contains("End"))
        {
            // disable the player
            Active(false);

            // celebrate
            StartCoroutine(Celebrate());
        }
        else
        {
            NextAction_();
        }
    }

    /// <summary>
    /// Celebrates at the finish line
    /// </summary>
    private IEnumerator Celebrate()
    {
        yield return new WaitForSeconds(0.6f);
        _jumpScript.SetAnimation("Celebrate");

        // check if there are any players remaining
        MarshLandController.Instance.CheckComplete(_playerIndex);
    }

    /// <summary>
    /// Creates a list of actions for the player to jump to the next marshmallow
    /// </summary>
    /// <param name="numMarshmallows">How many actions are required</param>
    public void SetActionList(int numMarshmallows)
    {
        for (int i = 0; i < numMarshmallows; i++)
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
        // if there are any left, use this one
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

            MarshLandController.Instance.ClearAction(_playerIndex);

            // can now fall in
            _leftStartPoint = true;
        }
    }

    /// <summary>
    /// Fall from the current platform
    /// </summary>
    void Fall_()
    {
        // can't fall off fof start
        if (!_leftStartPoint) return;

        // start the recoveryy process
        _recoveryPressesRemaining = 20;
        _inWater = true;

        _jumpScript.RecoveryComplete(false);
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
            if (action == RECOVERY_ACTION)
            {
                _recoveryPressesRemaining--;
                if (_recoveryPressesRemaining == 0)
                {
                    StartCoroutine(Recover());
                }
            }
        }
    }

    /// <summary>
    /// Recover after falling in
    /// </summary>
    private IEnumerator Recover()
    {
        _inWater = false;
        _jumpScript.Recover();

        // allow time to get above marshmallow
        yield return new WaitForSeconds(0.5f);
        _jumpScript.RecoveryComplete(true);
    }

    /// <summary>
    /// Walk on until the player reaches the specified point
    /// </summary>
    /// <param name="callback">The function to call when walked on</param>
    internal void WalkOn(Action callback)
    {
        _walkingOn = true;
        _walkOnCallBack = callback;
        _movement.enabled = true;
    }

    #region Input Handlers - all pass through to InputReceived
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

    public override void OnR1()
    {
        InputReceived_(MarshLandInputAction.R1);
    }
    #endregion

}