using System.Collections;
using UnityEngine;

public enum RockAction { LosePowerUp, KnockBack, KnockDown };

public class RockScript : MonoBehaviour
{
    // constraints
    public const float MIN_ROCK_SIZE = 0.3f;
    public const float MAX_ROCK_SIZE = 1f;
    public const float GIANT_ROCK_SIZE = 2.4f;

    // configuration of the rock
    RockAction _action;
    float _sizeFactor;

    // Unity configuration
    [SerializeField]
    Rigidbody2D Rigidbody;
    [SerializeField]
    Collider2D PlayerTrigger;
    [SerializeField]
    CircleCollider2D CircleCollider;

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
        Initialise(_sizeFactor);
    }

    /// <summary>
    /// Sets the state of the rock - random
    /// </summary>
    public void Initialise(float size)
    {
        _sizeFactor = size;
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
        if (collision.gameObject.tag == "AreaTrigger")
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
        if (collision.gameObject.tag.Contains("Player"))
        {
            var playerScript = collision.gameObject.GetComponent<PlayerClimber>();

            bool performAction = false;

            // do not affect the player if they are already disabled
            if (playerScript != null && playerScript.IsActive())
            {
                // check if this rock is specific to a certain player
                if (gameObject.name.Contains("Rock") && gameObject.name.Length == 5)
                {
                    // get the player index
                    var playerIndex = int.Parse(gameObject.name.Replace("Rock", ""));

                    // only affect player if this player did NOT spawn it
                    if (playerIndex != playerScript.GetPlayerIndex())
                        performAction = true;
                }
                else
                    performAction = true;
            }

            // apply the action if appropriate
            if (performAction)
            {
                // handle the tock action
                PerformAction_(collision.transform);

                // disable collisions between this player and the rock so that the event does not occur
                Physics2D.IgnoreCollision(collision, PlayerTrigger);

                // when hit player loses power up
                playerScript.DecreasePowerUpLevel();
            }
        }
        // when the rock hits a point, make sure it falls down a hole
        else if(collision.gameObject.tag == "RockStop")
        {
            // slow the rock
            Rigidbody.velocity.Set(0, Rigidbody.velocity.y);

            // destroy the object after a while
            StartCoroutine(HandleDestruction_());
        }
    }

    /// <summary>
    /// Temporarily disable the collider, then destroy the collider
    /// </summary>
    IEnumerator HandleDestruction_()
    {
        CircleCollider.enabled = false;
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Perform an action to the player that the rock collided with
    /// </summary>
    /// <param name="player">The player that the rock hit</param>
    private void PerformAction_(Transform player)
    {
        switch (_action)
        {
            // fall through in each case
            case RockAction.KnockDown:
                // disable movement
                player.GetComponent<PlayerClimber>().Disable(_sizeFactor * 4);
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
