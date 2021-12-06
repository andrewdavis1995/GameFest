using System;
using System.Collections;
using UnityEngine;

public class FollowBackInputHandler : GenericInputHandler
{
    const int STARTING_FOLLOWERS = 400;
    const int OUT_OF_BOUNDS_FOLLOWERS_LOST = 250;

    PlayerMovement _movement;
    TimeLimit _roundLimit;
    int _followers = 0;
    int _roundFollowers = 0;

    private void Update()
    {
        // TODO: move all this to input system methods
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _movement.Move(new Vector2(-1, 0));
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            _movement.Move(new Vector2(1, 0));
        }
        else
        {
            _movement.Move(new Vector2(0, 0));
        }

        if (Input.GetKey(KeyCode.Space))
        {
            _movement.Jump();
        }
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

        // set the height of the object
        SetHeight(spawned, characterIndex);

        // use the correct animation controller
        //SetAnimation(spawned, characterIndex);    // TODO

        _followers = STARTING_FOLLOWERS;

        return spawned;
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
    /// Event handler for when a trigger is entered
    /// </summary>
    /// <param name="collider">The object with the trigger</param>
    void TriggerEntered_(Collider2D collider)
    {
        // if it was the VIP zone, tell controller
        if (collider.tag == "AreaTrigger")
        {
            FollowBackController.Instance.PlayerEnteredZone(this);
        }
        // fell off bottom
        else if (collider.tag == "KickBack")
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
                var numFollowers = UnityEngine.Random.Range(1, 20);
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
    }
}
