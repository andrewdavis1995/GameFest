using System;
using System.Collections;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    static float SPEED = 15f;
    int _bouncesRemaining = 0;
    Rigidbody2D _rigidBody;
    float BULLET_DAMAGE = 15f;
    int _shooterIndex = -1;
    int BULLET_HIT_SCORE = 10;

    // Called once at startup
    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.velocity = transform.up * SPEED;
    }

    /// <summary>
    /// Sets the speed of the bullet
    /// </summary>
    /// <param name="speed">The speed to set</param>
    public void SetSpeed(float speed)
    {
        SPEED = speed;

        // update velocity
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.velocity = transform.up * SPEED;
    }

    /// <summary>
    /// Sets the number of times this bullet can bounce
    /// </summary>
    /// <param name="bounces">The number of times the bullet can bounce before being destroyed</param>
    public IEnumerator SetBounces(int bounces)
    {
        yield return new WaitForSeconds(0.1f);
        _bouncesRemaining = bounces;
    }

    /// <summary>
    /// Sets the damage that is done by the bullet
    /// </summary>
    /// <param name="damage">The damage that is done</param>
    public void SetDamage(float damage)
    {
        BULLET_DAMAGE = damage;
    }

    /// <summary>
    /// When the bullet collides with another object
    /// </summary>
    /// <param name="collision">The item that was collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Player":
                // only apply damage if it wasn't fired by a player
                collision.gameObject.GetComponent<ToneDeathMovement>().DamageDone(_shooterIndex < 0 ? BULLET_DAMAGE : 0);
                Destroy(gameObject);
                break;
            case "Enemy":
            {
                var enemy = collision.gameObject.GetComponent<EnemyControl>();
                // only apply damage if enemy can be shot
                if(enemy != null && enemy.IsShootable())
                {
                    // damage the enemy
                    ToneDeathController.Instance.AssignHitPoints(BULLET_HIT_SCORE, _shooterIndex);
                    enemy.Damage(BULLET_DAMAGE);
                }
                Destroy(gameObject);
                break;
            }
            case "Bullet":
                // destroy when colliding with another bullet
                Destroy(gameObject);
                break;

            // can bounce off everything else
            default:
                if (_bouncesRemaining == 0)
                    Destroy(gameObject);
                else
                    Bounce_(collision);
                break;
        }
    }

    /// <summary>
    /// Temporarily disables collisions with the specified collider
    /// </summary>
    /// <param name="boxCollider2D">The collider to ignore</param>
    /// <param name="playerIndex">The index of the player who fired the bullet</param>
    internal IEnumerator IgnorePlayer(BoxCollider2D boxCollider2D, int playerIndex)
    {
        // store who fired the bullet
        _shooterIndex = playerIndex;
    
        // disable collider
        var script = GetComponent<Collider2D>();
        script.enabled = false;

        // briefly wait, then re-enable
        yield return new WaitForSeconds(0.1f);
        script.enabled = true;
    }

    /// <summary>
    /// Bounce the bullet - reverse direction
    /// </summary>
    /// <param name="collision">The object that the bullet collided with</param>
    private void Bounce_(Collision2D collision)
    {
        // flip
        var dir = _rigidBody.velocity;
        var norm = collision.contacts[0].normal;
        var vel = Vector3.Reflect(dir, norm);

        // change angle and direction
        _rigidBody.velocity = vel;
        transform.eulerAngles *= -1;

        _bouncesRemaining--;
    }
}
