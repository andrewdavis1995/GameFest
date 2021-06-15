using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroGravityMovement : MonoBehaviour
{
    const float MAX_SPEED = 4.5f;
    const float PROPULSION_FORCE = 16f;

    // links to unity objects
    Rigidbody2D _rigidBody;
    [SerializeField]
    SpriteRenderer _spaceman;
    [SerializeField]
    SpriteRenderer _extinguisher;
    [SerializeField]
    BoxCollider2D _collider;

    // status
    float _xMovement = 0;
    bool _inDoorZone = false;
    List<int> _pointsCollected = new List<int>();
    bool _isComplete = false;
    bool _isDead = false;

    // status
    float _health = 100f;

    /// <summary>
    /// Called when item is created
    /// </summary>
    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    // Called once per frame
    private void Update()
    {
        // only update the player if they are not finished
        if (!_isComplete)
        {
            // TODO: replace with input handler system
            if (Input.GetKey(KeyCode.Space))
                _rigidBody.AddForce(new Vector3(0, PROPULSION_FORCE, 0));

            bool moving = false;

            if (Input.GetKey(KeyCode.RightArrow))
            {
                moving = true;
                _xMovement += 0.1f;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                moving = true;
                _xMovement -= 0.1f;
            }

            if (_xMovement > MAX_SPEED) _xMovement = MAX_SPEED;
            if (_xMovement < -MAX_SPEED) _xMovement = -MAX_SPEED;

            if (!moving)
            {
                if (_xMovement > 0) _xMovement -= 0.1f;
                else if (_xMovement < 0) _xMovement += 0.1f;
            }
            else
            {
                _spaceman.flipX = _xMovement < 0;
                _extinguisher.flipX = _xMovement < 0;
            }
            // ######################################

            // This can be added in if we want to stop players moving when they are not boosting
            //if (_rigidBody.velocity.y > 0.005f || _rigidBody.velocity.y < -0.005f)
            //{
            //    transform.Translate(new Vector3(_xMovement * Time.deltaTime, 0));
            //    _spaceman.transform.eulerAngles = new Vector3(0, 0, -_xMovement * 7f);
            //}
            //else
            //{
            //    _spaceman.transform.eulerAngles = new Vector3(0, 0, 0);
            //}

            // TODO: Replace with Input Handler system 
            // if the player is in the door and chooses to bail, make them exit
            if (_inDoorZone && Input.GetKeyDown(KeyCode.T))
            {
                Escape_();
            }
            // ###########################################
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
    private void Escape_()
    {
        // TODO: go through the door
        _isComplete = true;
        enabled = false;

        // inform the controller that this player is finished
        XTinguishController.Instance.CheckForComplete();
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

            // make the player flash red to show they were hit
            StartCoroutine(FlashRed_());

            // if the player has no more health left, make them die
            if(_health <= 0)
            {
                Die_();
            }
        }
    }

    /// <summary>
    /// The player has run out of health. They are no longer active
    /// </summary>
    private void Die_()
    {
        // keep in the same position
        _rigidBody.constraints = RigidbodyConstraints2D.FreezePosition;
        _collider.enabled = false;

        // set status variables
        _isComplete = true;
        _isDead = true;

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
}
