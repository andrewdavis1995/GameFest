using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FollowBackInputHandler : GenericInputHandler
{
    public int STARTING_FOLLOWERS = 400;
    const int OUT_OF_BOUNDS_FOLLOWERS_LOST = 175;
    const float SELFIE_DELAY = 4f;
    const int NUM_TROLLS = 5;

    PlayerMovement _movement;
    TimeLimit _roundLimit;
    [SerializeField]
    int _followers = 0;
    int _roundFollowers = 0;
    bool _canTakeSelfie = false;
    bool _selfieTakenThisRound = false;
    bool _canMove = false;
    Transform _trollPrefab;
    bool _trollsPending = false;
    List<TrollAttackScript> _activeTrolls = new List<TrollAttackScript>();

    /// <summary>
    /// Left joystick event handler
    /// </summary>
    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        // can't move if not enabled, or trolls are active
        if (!_canMove) return;
        if (_activeTrolls.Count > 0) return;

        _movement.Move(ctx.ReadValue<Vector2>());
    }

    /// <summary>
    /// Cross event handler
    /// </summary>
    public override void OnCross()
    {
        if (!_canMove) return;
        if (_activeTrolls.Count > 0) return;
        _movement.Jump();
    }

    /// <summary>
    /// Square event handler
    /// </summary>
    public override void OnSquare()
    {
        if (!_canMove) return;

        if (_activeTrolls.Count > 0)
        {
            // attack the first troll
            bool destroyed = _activeTrolls[0].ApplyDamage();
            if (destroyed)
                _activeTrolls.RemoveAt(0);

            // if that was the last one, reenable movement
            if (_activeTrolls.Count == 0)
            {
                // if in zone, start checking for selfies
                if (FollowBackController.Instance.PlayerInZone(this) && !_selfieTakenThisRound)
                    StartCoroutine(SelfieCheck_());

                _movement.Reenable();
            }
        }
    }

    /// <summary>
    /// Triangle event handler
    /// </summary>
    public override void OnTriangle()
    {
        // no selfies if not enabled or trolls are active
        if (!_canMove) return;
        if (_activeTrolls.Count > 0) return;

        TakeSelfie_();
    }

    /// <summary>
    /// L1 event handler
    /// </summary>
    public override void OnL1()
    {
        if (PauseGameHandler.Instance.IsPaused() && IsHost())
        {
            PauseGameHandler.Instance.PreviousPage();
        }
    }


    /// <summary>
    /// Triangle event handler
    /// </summary>
    public override void OnR1()
    {
        if (PauseGameHandler.Instance.IsPaused() && IsHost())
        {
            PauseGameHandler.Instance.NextPage();
        }
    }

    /// <summary>
    /// Trolls have removed all followers
    /// <summary>
    public void TrollsDone()
    {
        // destroy all
        foreach (var t in _activeTrolls)
        {
            t.Destroy();
        }

        // free trolls
        _activeTrolls.Clear();

        // start movement again
        _movement.Reenable();
    }

    /// <summary>
    /// Player can now move
    /// </summary>
    public void EnableMovement()
    {
        _canMove = true;
    }

    /// <summary>
    /// Player can no longer move
    /// </summary>
    public void DisableMovement()
    {
        _canMove = false;
    }

    /// <summary>
    /// Game has ended, so destroy all
    /// </summary>
    public void DestroyAllTrolls()
    {
        // destroy object
        foreach (var troll in _activeTrolls)
        {
            Destroy(troll.gameObject);
        }

        // clear list
        _activeTrolls.Clear();
    }

    /// <summary>
    /// Set the height of the player based on the selected character
    /// </summary>
    /// <param name="player">The transform the the player display</param>
    /// <param name="characterIndex">The index of the chosen character</param>
    public override void SetHeight(Transform player, int characterIndex)
    {
        float size = 0.125f;

        switch (characterIndex)
        {
            // Andrew
            case 0:
                size += 0.00f;
                break;
            // Rachel
            case 1:
                size -= 0.025f;
                break;
            // Naomi
            case 2:
                size -= 0.015f;
                break;
            // Heather
            case 3:
                size -= 0.017f;
                break;
            // Mum & Dad
            case 4:
            case 5:
                size -= 0.01f;
                break;
            // John & Fraser
            case 6:
            case 7:
                size -= 0.011f;
                break;
            // Matthew
            case 8:
                size += 0.004f;
                break;
        }

        // set height
        player.localScale = new Vector3(size, size, 1);
    }

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerIndex">The index of the player</param>
    /// <param name="id">ID of the profile</param>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex, Guid id)
    {
        base.Spawn(prefab, position, characterIndex, playerName, playerIndex, id);

        // create the player display
        var spawned = Instantiate(prefab, position, Quaternion.identity);

        // get the movement script attached to the visual player
        _movement = spawned.GetComponent<PlayerMovement>();

        // dampen jump force slightly
        _movement.SetJumpModifier(0.85f);

        // assign callbacks for when the item interacts with triggers
        _movement.AddTriggerCallbacks(TriggerEntered_, TriggerExited_);

        // set collision callbacks (to check landing for spawning trolls)
        _movement.AddMovementCallbacks(ColliderHit_, null);

        // set the height of the object
        SetHeight(spawned, characterIndex);

        // use the correct animation controller
        SetAnimation(spawned, characterIndex);

        _followers = STARTING_FOLLOWERS;

        return spawned;
    }

    /// <summary>
    /// Callback for when the player collides with an object
    /// </summary>
    /// <param name="collider">The item that was collided with</param>
    void ColliderHit_(Collision2D collider)
    {
        if (collider.gameObject.tag == "Ground" && _trollsPending)
        {
            StartCoroutine(SpawnTrolls_());
        }
    }

    /// <summary>
    /// Returns the transform related to the movement script (the visual player)
    /// </summary>
    /// <returns>The visual object for this player</returns>
    internal Transform MovementObject()
    {
        return _movement.transform;
    }

    /// <summary>
    /// Starts a check for if the player has been in the zone for X seconds - allow a selfie if so
    /// </summary>
    IEnumerator SelfieCheck_()
    {
        for (float f = 0; f < SELFIE_DELAY && SelfiePossible_(); f += 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }


        if (SelfiePossible_())
            SelfieAvailable_(_activeTrolls.Count == 0);
    }

    /// <summary>
    /// Checks if the player is in the correct zone, and there are no trolls active
    /// </summary>
    /// <returns></returns>
    bool SelfiePossible_()
    {
        return FollowBackController.Instance.PlayerInZone(this) && !TrollsActive();
    }

    /// <summary>
    /// Are the any trolls active on this player?
    /// </summary>
    /// <returns>Whether the player has trolls active</returns>
    public bool TrollsActive()
    {
        return _activeTrolls.Count > 0;
    }

    /// <summary>
    /// Event handler for when a trigger is entered
    /// </summary>
    /// <param name="collider">The object with the trigger</param>
    void TriggerEntered_(Collider2D collider)
    {
        // if it was the VIP zone, tell controller
        if (collider.tag == "AreaTrigger")
        {
            FollowBackController.Instance.PlayerEnteredZone(this);

            // start delay before selfie allowed
            if (!_selfieTakenThisRound)
                StartCoroutine(SelfieCheck_());
        }
        // fell off bottom
        else if (collider.tag == "KickBack" && _canMove)
        {
            // jump back to top
            _movement.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            _movement.transform.localPosition = new Vector3(_movement.transform.localPosition.x, 6, _movement.transform.localPosition.z);

            // lose points
            LoseFollower(false, OUT_OF_BOUNDS_FOLLOWERS_LOST);
            DisplayFollowersUpdate_($"fell out of relevance and lost {OUT_OF_BOUNDS_FOLLOWERS_LOST} followers");
        }
        // notification triggers
        else if (collider.tag == "PowerUp")
        {
            // follower notification collected
            if (collider.gameObject.name == "Follower")
            {
                // add a random number of followers
                var numFollowers = UnityEngine.Random.Range(7, 30);
                AddFollower(false, numFollowers);
                DisplayFollowersUpdate_($" gained <color=#11ea11>{numFollowers}</color> followers");
            }
            // alert notification
            else if (collider.gameObject.name == "Notification")
            {
                FollowBackController.Instance.EventNotificationTriggered();
            }

            collider.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates UI after followers updated
    /// </summary>
    /// <param name="message">The message to display</param>
    private void DisplayFollowersUpdate_(string message)
    {
        FollowBackController.Instance.AddVidiprinterItem(this, message);
        FollowBackController.Instance.UpdatePlayerUIs(this);
    }

    /// <summary>
    /// Event handler for when a trigger is exited
    /// </summary>
    /// <param name="collider">The object with the trigger</param>
    void TriggerExited_(Collider2D collider)
    {
        // if it was the VIP zone, tell controller
        if (collider.tag == "AreaTrigger")
        {
            FollowBackController.Instance.PlayerLeftZone(this);

            // selfie check invalidated
            StopCoroutine(SelfieCheck_());
            SelfieAvailable_(false);
        }
    }

    /// <summary>
    /// Adds the specified number of followers to the count
    /// </summary>
    /// <param name="roundSpecific">Whether to update the round followers count</param>
    /// <param name="count">The number of followers to add (defaults to 1)</param>
    public void AddFollower(bool roundSpecific, int count = 1)
    {
        _followers += count;

        if (roundSpecific)
            _roundFollowers += count;
    }

    /// <summary>
    /// Removes the specified number of followers from the count
    /// </summary>
    /// <param name="roundSpecific">Whether to update the round followers count</param>
    /// <param name="count">The number of followers to remove (defaults to 1)</param>
    public void LoseFollower(bool roundSpecific, int count = 1)
    {
        _followers -= count;

        // ensure we don't go under 0
        if (_followers < 0)
            _followers = 0;

        // add to round-specific stats if specified
        if (roundSpecific)
            _roundFollowers -= count;
    }

    /// <summary>
    /// Gets the number of followers this player has
    /// </summary>
    /// <returns>The number of followers</returns>
    public int GetFollowerCount()
    {
        return _followers;
    }

    /// <summary>
    /// Gets the number of followers this player has gained this round
    /// </summary>
    /// <returns>The number of followers gained</returns>
    public int GetFollowerCountRound()
    {
        return _roundFollowers;
    }

    /// <summary>
    /// Resets followers gained this round
    /// </summary>
    public void NewRound()
    {
        _roundFollowers = 0;
        _selfieTakenThisRound = false;
        SelfieAvailable_(false);
    }

    /// <summary>
    /// Resets followers gained this round
    /// </summary>
    /// <param id="available">Can the player take a selfie</param>
    void SelfieAvailable_(bool available)
    {
        _canTakeSelfie = available;
        _movement.SetIcon(available ? FollowBackController.Instance.SelfieIcon : null);
        _movement.ActivePlayerIcon.gameObject.SetActive(available);
    }

    /// <summary>
    /// Takes a selfie (if allowed)
    /// </summary>
    void TakeSelfie_()
    {
        if (!_canTakeSelfie) return;

        SelfieAvailable_(false);
        _selfieTakenThisRound = true;
        FollowBackController.Instance.SelfieTaken(this);
    }

    /// <summary>
    /// Need to spawn trolls
    /// </summary>
    /// <param id="trollPrefab"/>The prefab to spawn for each troll</param>
    public void TrollAttack(Transform trollPrefab)
    {
        _trollPrefab = trollPrefab;

        // don't do trolls if no followers
        if (GetFollowerCount() > 0)
        {
            // spawn if on ground, or wait until landed if in the air
            if (_movement.OnGround())
            {
                StartCoroutine(SpawnTrolls_());
            }
            else
            {
                _trollsPending = true;
            }
        }
    }

    /// <summary>
    /// Spawns trolls
    /// </summary>
    IEnumerator SpawnTrolls_()
    {
        _trollsPending = false;
        _movement.Disable(FollowBackController.Instance.DisabledImages[GetCharacterIndex()]);

        // selfie check invalidated
        StopCoroutine(SelfieCheck_());
        SelfieAvailable_(false);

        for (int i = 0; i < NUM_TROLLS; i++)
        {
            // spawn a troll
            var spawned = Instantiate(_trollPrefab, _movement.transform.localPosition, Quaternion.identity);

            // keep track of the troll
            var trollScript = spawned.GetComponent<TrollAttackScript>();
            trollScript.Setup(this, _activeTrolls.Count + 1);
            _activeTrolls.Add(trollScript);

            yield return new WaitForSeconds(0.3f);
        }
    }
}
