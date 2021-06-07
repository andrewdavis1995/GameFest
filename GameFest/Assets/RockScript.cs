using UnityEngine;

public enum RockAction { LosePowerUp, KnockBack, KnockDown };

public class RockScript : MonoBehaviour
{
    // constraints
    const float MIN_ROCK_SIZE = 0.3f;
    const float MAX_ROCK_SIZE = 1f;

    // configuration of the rock
    RockAction _action;
    float _sizeFactor;

    // Unity configuration
    [SerializeField]
    Rigidbody2D Rigidbody;
    [SerializeField]
    Collider2D PlayerTrigger;

    /// <summary>
    /// Called on startup
    /// </summary>
    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Sets the state of the rock - random
    /// </summary>
    public void Initialise(float minSize = MIN_ROCK_SIZE, float maxSize = MAX_ROCK_SIZE)
    {
        _sizeFactor = Random.Range(MIN_ROCK_SIZE, MAX_ROCK_SIZE);
        transform.localScale *= _sizeFactor;
        // give random force to keep things interesting
        Rigidbody.AddForce(new Vector2(Random.Range(-5, 5), Random.Range(-1, 20)));
        Rigidbody.mass *= Random.Range(0.95f, 1.1f);

        // TODO: assign random bounciness
        // TODO: change image randomly

        SetRockAction_();
    }

    /// <summary>
    /// Sets the action that this rock causes to the player, based on its size
    /// </summary>
    private void SetRockAction_()
    {
        // if big enough, the rock will knock player over
        if (_sizeFactor > 0.5f)
            _action = RockAction.KnockDown;
        // if medium sized, player is knocked back, but not disabled
        else if (_sizeFactor > 0.35f)
            _action = RockAction.KnockBack;
        // if small, power up is lost
        else if (_sizeFactor >= 0.3f)
            _action = RockAction.LosePowerUp;
    }

    /// <summary>
    /// When the rock collides with an object
    /// </summary>
    /// <param name="collision">The item that collided with the rock</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if the collision came from hitting the end point, destroy the rock
        if(collision.gameObject.tag == "AreaTrigger")
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// When the rock collides with an object - player trigger event
    /// </summary>
    /// <param name="collision">The item that collided with the rock</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if the collision came from a player, handle it
        if (collision.gameObject.tag.Contains("Player") && collision.gameObject.GetComponent<PlayerClimber>().IsActive())
        {
            // handle the tock action
            PerformAction_(collision.transform);

            // disable collisions between this player and the rock so that the event does not occur
            Physics2D.IgnoreCollision(collision, PlayerTrigger);
        }
    }

    /// <summary>
    /// Perform an action to the player that the rock collided with
    /// </summary>
    /// <param name="player">The player that the rock hit</param>
    private void PerformAction_(Transform player)
    {
        switch(_action)
        {
            // fall through in each case
            case RockAction.KnockDown:
                // disable movement
                player.GetComponent<PlayerClimber>().Disable();
                goto case RockAction.KnockBack;
            case RockAction.KnockBack:
                // knockback
                player.GetComponent<Rigidbody2D>().AddForce(new Vector2(-1400 * _sizeFactor, 1000 * _sizeFactor));
                goto case RockAction.LosePowerUp;
            case RockAction.LosePowerUp:
                // TODO: set bonus health to zero
                break;
        }
    }
}
