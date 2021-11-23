using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CashDashInputHandler : GenericInputHandler
{
    bool _canMove = true;
    bool _complete = false;
    bool _hasBvKey = false;
    bool _offScreenDying = false;

    RectTransform _offScreenDisplay;
    MediaJamWheel[] _jamWheelPlatforms;
    PlayerMovement _movement;

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerName">The index of the selected character</param>
    /// <param name="id">ID of the profile</param>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex, Guid id)
    {
        base.Spawn(prefab, position, characterIndex, playerName, playerIndex, id);

        // create the player display
        var spawned = Instantiate(prefab, position, Quaternion.identity);

        // get the movement script attached to the visual player
        _movement = spawned.GetComponent<PlayerMovement>();

        _movement.SetJumpModifier(0.82f);
        _movement.AddMovementCallbacks(PlatformLanded, PlatformLeft);
        _movement.AddTriggerCallbacks(TriggerEnter, null);
        _movement.SetMasking(SpriteMaskInteraction.VisibleOutsideMask);

        // set the height of the object
        SetHeight(spawned, characterIndex);

        // use the correct animation controller
        SetAnimation(spawned, characterIndex);

        // sets the player name
        _movement.SetPlayerName(true, playerName);

        return spawned;
    }

    /// <summary>
    /// Callback for when the player collides with an object
    /// </summary>
    /// <param name="collider">The item that was collided with</param>
    void PlatformLanded(Collision2D collider)
    {
        if (collider.gameObject.tag == "Ground")
        {
            // check if it was a cog
            _movement.transform.SetParent(collider.transform);
            var cwns = collider.transform.GetComponent<CogWheelNoteScript>();
            if (cwns != null)
            {
                // player landed
                cwns.CogSystem.PlayerLanded();
            }
        }
        else if (collider.gameObject.tag == "KickBack")
        {
            // bounce back a bit
            _movement.BounceBack(collider);
        }
    }

    /// <summary>
    /// Check if the player is complete
    /// </summary>
    /// <returns>If the player is complete</returns>
    internal bool Complete()
    {
        return _complete;
    }

    /// <summary>
    /// Callback for when the player stops colliding with an object
    /// </summary>
    /// <param name="collider">The item that was left</param>
    void PlatformLeft(Collision2D collider)
    {
        // check if the player left the ground
        if (collider.gameObject.tag == "Ground")
        {
            _movement.transform.SetParent(null);

            var cwns = collider.transform.GetComponent<CogWheelNoteScript>();
            if (cwns != null)
            {
                cwns.CogSystem.PlayerLeft();
            }
        }
    }

    /// <summary>
    /// Callback for when the player enters a trigger
    /// </summary>
    /// <param name="collider">The trigger that was triggered</param>
    void TriggerEnter(Collider2D collider)
    {
        // is it a coin?
        if (collider.gameObject.tag == "PowerUp")
        {
            var coin = collider.GetComponent<CoinScript>();
            coin.Disable();
            AddPoints(coin.Points);
        }
        // is it the BV key?
        else if (collider.gameObject.tag == "Card")
        {
            // only destroy if matches the player
            if (collider.gameObject.name == GetPlayerIndex().ToString())
                collider.gameObject.SetActive(false);

            if (!_hasBvKey)
            {
                _movement.ActivePlayerIcon.gameObject.SetActive(true);
                _hasBvKey = true;
                _movement.SetIcon(CashDashController.Instance.KeyIcon);
            }

            _movement.IgnoreCollisions(CashDashController.Instance.BvColliders);
        }
        // BV gate causes player to be blocked
        else if (collider.gameObject.tag == "KickBack")
        {
            var bvGate = collider.GetComponentInParent<BvGateScript>();
            if (bvGate != null)
            {
                // check if the player has a key
                if (_hasBvKey)
                {
                    // they may pass
                    _movement.ActivePlayerIcon.gameObject.SetActive(false);
                    bvGate.DisplayMessage("Welcome " + GetPlayerName(), GetPlayerIndex());
                }
                else
                {
                    // blocked 
                    StartCoroutine(_movement.Disable(2, CashDashController.Instance.DisabledImages[GetCharacterIndex()]));
                    bvGate.DisplayMessage("FOREIGN OBJECT", GetPlayerIndex());
                }
            }
        }
        else if (collider.gameObject.tag == "AreaTrigger")
        {
            // check if we have reached the end
            if (collider.gameObject.name == "END" && _canMove)
            {
                // assign completion points
                AddPoints(CashDashController.Instance.RemainingPoints());
                var bonuses = CashDashController.Instance.GetPositionalPoints();
                AddPoints(bonuses);
                SetBonusPoints(bonuses);

                // disable movement
                _canMove = false;
                _complete = true;
                _movement.Move(new Vector2(-1, 0));

                CashDashController.Instance.CheckForCompletion();
            }
        }
    }

    /// <summary>
    /// Sets the image to use when the player goes off screen
    /// </summary>
    /// <param name="rectTransform">The object to use</param>
    internal void SetOffScreenDisplay(RectTransform rectTransform)
    {
        _offScreenDisplay = rectTransform;
        _offScreenDisplay.GetComponentsInChildren<Image>()[2].sprite = CashDashController.Instance.FlailImages[GetCharacterIndex()];
    }

    /// <summary>
    /// Sets the platforms that are in use for this player
    /// </summary>
    public void SetMediaJamPlatforms(List<MediaJamWheel> wheels)
    {
        _jamWheelPlatforms = wheels.ToArray();
    }

    /// <summary>
    /// Set the height of the player based on the selected character
    /// </summary>
    /// <param name="player">The transform the the player display</param>
    /// <param name="characterIndex">The index of the chosen character</param>
    public override void SetHeight(Transform player, int characterIndex)
    {
        float size = 0.12f;

        switch (characterIndex)
        {
            // Rachel
            case 1:
                size -= .03f;
                break;
            // Naomi
            case 2:
                size -= .015f;
                break;
            // Heather
            case 3:
                size -= 0.2f;
                break;
            // Mum & Dad
            case 4:
            case 5:
                size -= 0.01f;
                break;
            // John & Fraser
            case 6:
            case 7:
                size -= 0.007f;
                break;
            // Matthew
            case 8:
                size += 0.003f;
                break;
        }

        // set height
        player.localScale = new Vector3(size, size, 1);
    }

    /// <summary>
    /// When the move event is triggered
    /// </summary>
    /// <param name="ctx">The context of the movement</param>
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        if (!_canMove || !CashDashController.Instance.IsActive() || PauseGameHandler.Instance.IsPaused()) return;

        // move the player
        _movement.Move(ctx.ReadValue<Vector2>());
    }

    /// <summary>
    /// When the cross event is triggered
    /// </summary>
    public override void OnCross()
    {
        if (!_canMove || !CashDashController.Instance.IsActive() || PauseGameHandler.Instance.IsPaused()) return;

        _movement.Jump();
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    void Update()
    {
        // do not update the position if dead
        if (_complete)
        {
            _offScreenDisplay.gameObject.SetActive(false);
            return;
        }

        var yPos = Camera.main.WorldToViewportPoint(_movement.transform.position).y;
        bool isVisible = yPos > 0 && yPos < 1;

        // if the player can't be seen, show the off-screen icon
        if (!isVisible)
        {
            var xPos = Camera.main.WorldToScreenPoint(_movement.transform.position).x;
            _offScreenDisplay.position = new Vector3(xPos, _offScreenDisplay.position.y, _offScreenDisplay.position.z);
            _offScreenDisplay.gameObject.SetActive(!_complete);

            // start the wait before dying
            if (!_offScreenDying)
            {
                _offScreenDying = true;
                StartCoroutine(WaitBeforeDying_());
            }
        }
        else
        {
            // hide if visible
            _offScreenDisplay.gameObject.SetActive(false);

            // if the player was off screen, stop the process of dying
            if (_offScreenDying)
            {
                _offScreenDying = false;
                StopAllCoroutines();

                // re-enable movement (disable completion would be skipped if off screen)
                _movement.Reenable();
            }
        }
    }

    /// <summary>
    /// If the player is off screen for 7+ seconds, they die
    /// </summary>
    private IEnumerator WaitBeforeDying_()
    {
        yield return new WaitForSeconds(7);

        // belt-and-braces check
        if (_offScreenDying)
        {
            _complete = true;

            // disable forever
            StartCoroutine(_movement.Disable(1000, CashDashController.Instance.DisabledImages[GetCharacterIndex()]));
        }
    }

    /// <summary>
    /// When the right joystick is moved
    /// </summary>
    /// <param name="ctx">Context of the input</param>
    public override void OnMoveRight(InputAction.CallbackContext ctx)
    {
        foreach (var platform in _jamWheelPlatforms)
        {
            platform.OnMove(ctx.ReadValue<Vector2>());
        }
    }

    /// <summary>
    /// Input handler for L1 button
    /// </summary>
    public override void OnL1()
    {
        if (PauseGameHandler.Instance.IsPaused() && IsHost())
        {
            PauseGameHandler.Instance.PreviousPage();
        }
    }

    /// <summary>
    /// Input handler for R1 button
    /// </summary>
    public override void OnR1()
    {
        if (PauseGameHandler.Instance.IsPaused() && IsHost())
        {
            PauseGameHandler.Instance.NextPage();
        }
    }
}
