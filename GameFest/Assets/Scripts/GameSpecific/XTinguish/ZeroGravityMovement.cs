using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZeroGravityMovement : MonoBehaviour
{
    const float MAX_SPEED = 5f;
    const float PROPULSION_FORCE = 15f;

    // links to unity objects
    Rigidbody2D _rigidBody;
    [SerializeField]
    SpriteRenderer _spaceman;
    [SerializeField]
    SpriteRenderer _extinguisher;
    [SerializeField]
    BoxCollider2D _collider;
    [SerializeField]
    BoxCollider2D _triggerCollider;
    [SerializeField]
    SpriteRenderer _colourDetails;
    [SerializeField]
    SpriteRenderer _healthBarFill;
    [SerializeField]
    GameObject _healthBar;

    // status
    float _xMovement = 0;
    bool _inDoorZone = false;
    List<int> _pointsCollected = new List<int>();
    bool _isComplete = false;
    bool _isDead = false;
    float _lastXInput = 0;
    float _lastYInput = 0;
    int _playerIndex;

    // status
    float _health = 100f;

    // callbacks
    Action<int> _addPointCallback;

    /// <summary>
    /// Called when item is created
    /// </summary>
    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// When the user chooses to move the player left to right
    /// </summary>
    /// <param name="xInput">How much they are moving</param>
    public void X_Movement(float xInput)
    {
        _lastXInput = xInput;
    }

    /// <summary>
    /// When the user chooses to move the player up
    /// </summary>
    /// <param name="xInput">How much they are moving</param>
    public void Y_Movement(float yInput)
    {
        _lastYInput = yInput;
    }

    /// <summary>
    /// Sets the callback to call when points have been collected
    /// </summary>
    public void SetAddPointsCallback(Action<int> callback)
    {
        _addPointCallback = callback;
    }

    /// <summary>
    /// Sets the colour of the suit details to match the players colour
    /// </summary>
    /// <param name="playerIndex"></param>
    internal void SetPlayerColour(int playerIndex)
    {
        _playerIndex = playerIndex;
        _colourDetails.color = ColourFetcher.GetColour(playerIndex);
    }

    // Called once per frame
    private void Update()
    {
        // only update the player if they are not finished
        if (!_isComplete)
        {
            // move player
            _xMovement += 0.1f * _lastXInput;

            if (_xMovement > MAX_SPEED) _xMovement = MAX_SPEED;
            if (_xMovement < -MAX_SPEED) _xMovement = -MAX_SPEED;

            // if the user is moving the player, keep updating the player
            bool moving = _lastXInput > 0.01f || _lastXInput < -0.01f;
            if (!moving)
            {
                if (_xMovement > 0) _xMovement -= 0.1f;
                else if (_xMovement < 0) _xMovement += 0.1f;
            }
            else
            {
                // make sure the player is facing the correct way
                _spaceman.flipX = _xMovement < 0;
                _extinguisher.flipX = _xMovement < 0;
                _colourDetails.flipX = _xMovement < 0;
            }

            // boost up
            if (_lastYInput > 0.2f)
                Propulsion();

            // move and rotate player
            transform.Translate(new Vector3(_xMovement * Time.deltaTime, 0));
            _spaceman.transform.eulerAngles = new Vector3(0, 0, -_xMovement * 7f);
        }
        else if (_isDead)
        {
            // if the player is dead, make them slowly rotate
            _spaceman.transform.eulerAngles += new Vector3(0, 0, 0.4f);
        }
    }

    /// <summary>
    /// Go through the door, and mark as complete
    /// </summary>
    public void Escape()
    {
        // can't escape if not in the correct area
        if (!_inDoorZone) return;

        // add points to player
        var totalValue = _pointsCollected.Sum(p => p);
        _addPointCallback(totalValue);

        // player is complete
        _isComplete = true;
        enabled = false;

        // show playing escaping
        StartCoroutine(Teleport());

        // inform the controller that this player is finished
        XTinguishController.Instance.CheckForComplete();
    }

    /// <summary>
    /// Move the player to behind the window
    /// </summary>
    private IEnumerator Teleport()
    {
        LockPlayer_();
        _spaceman.gameObject.SetActive(false);
        _spaceman.transform.eulerAngles = new Vector3(0, 0, 0);
        // move behind window
        transform.position = XTinguishController.Instance.TransportPosition + (new Vector3(2, 0, 0) * _playerIndex);
        yield return new WaitForSeconds(1);
        _spaceman.gameObject.SetActive(true);
    }

    /// <summary>
    /// Disables player objects when complete/dead
    /// </summary>
    private void LockPlayer_()
    {
        _rigidBody.velocity = Vector3.zero;
        _collider.enabled = false;
        _triggerCollider.enabled = false;
        _rigidBody.isKinematic = true;
        _healthBar.SetActive(false);
    }

    /// <summary>
    /// Checks if the player is dead
    /// </summary>
    /// <returns>Whether the player is dead</returns>
    internal bool IsDead()
    {
        return _isDead;
    }

    /// <summary>
    /// When the player fires the propulsion
    /// </summary>
    public void Propulsion()
    {
        _rigidBody.AddForce(new Vector3(0, PROPULSION_FORCE, 0));
    }

    /// <summary>
    /// Accessor for whether the player has "escaped" or died
    /// </summary>
    /// <returns>Whether the player is complete</returns>
    public bool IsComplete()
    {
        return _isComplete;
    }

    /// <summary>
    /// When the player collides with another object
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // was the object the fire/outside obstacle
        if (collision.gameObject.tag == "KickBack")
        {
            // decrease health
            _health -= 15;

            // calculate size and position
            var width = _health / 100f;
            var left = -1 * (((100 - _health) / 100) / 2);

            // displays the current health
            _healthBarFill.size = new Vector2(width, 1);
            _healthBarFill.transform.localPosition = new Vector3(left, 0, 0);

            // set color
            var r = _health <= 50 ? 1f : ((100 - _health) / 50);
            var g = _health >= 50 ? 1f : (_health / 50f);
            _healthBarFill.color = new Color(r, g, 0);

            // make the player flash red to show they were hit
            StartCoroutine(FlashRed_());

            // bounce back a bit
            _rigidBody.AddForce(collision.relativeVelocity * -2);

            // if the player has no more health left, make them die
            if (_health <= 0)
            {
                Die_(true);
            }
        }
    }

    /// <summary>
    /// The player has run out of time
    /// </summary>
    public void Timeout()
    {
        // if not complete, make player die
        if (!_isComplete)
            Die_(false);
    }

    /// <summary>
    /// The player has run out of health. They are no longer active
    /// </summary>
    private void Die_(bool check)
    {
        LockPlayer_();

        // if dead, score is zero
        _pointsCollected.Clear();

        // keep in the same position
        _rigidBody.constraints = RigidbodyConstraints2D.FreezePosition;

        // set status variables
        _isComplete = true;
        _isDead = true;

        // hide healthbar
        _healthBar.SetActive(false);

        if (check)
            // inform the controller that this player is complete
            XTinguishController.Instance.CheckForComplete();
    }

    /// <summary>
    /// Briefly flash the player red to show they have lost health
    /// </summary>
    IEnumerator FlashRed_()
    {
        // set to red
        _spaceman.color = Color.red;
        // wait briefly
        yield return new WaitForSeconds(.3f);
        // set colour back
        _spaceman.color = Color.white;
    }

    /// <summary>
    /// When the player enters a trigger
    /// </summary>
    /// <param name="collision">The object the player collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // was it a battery?
        if (collision.tag == "PowerUp")
        {
            // store the value of the battery
            var value = collision.GetComponent<BatteryScript>().GetValue();
            _pointsCollected.Add(value);

            // destroy the battery object
            Destroy(collision.gameObject);
        }
        // was it the exit door?
        else if (collision.tag == "AreaTrigger")
        {
            // we are currently in the exit zone
            _inDoorZone = true;
        }
    }

    /// <summary>
    /// When the player exits a trigger
    /// </summary>
    /// <param name="collision">The object the player stop colliding with</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // was it the exit door?
        if (collision.tag == "AreaTrigger")
        {
            // we have left the exit zone
            _inDoorZone = false;
        }
    }

    /// <summary>
    /// Gets a list of the battery values tha have been collected
    /// </summary>
    /// <returns>List of points</returns>
    public List<int> GetBatteryList()
    {
        return _pointsCollected;
    }
}
