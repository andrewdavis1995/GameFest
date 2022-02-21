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

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        //_rigidBody.AddForce(transform.up * 20);
        _rigidBody.velocity = transform.up * SPEED;
    }

    private void Update()
    {
        // transform.Translate(Vector2.up * Time.deltaTime * SPEED * _direction);
    }

    public IEnumerator SetBounces(int bounces)
    {
        yield return new WaitForSeconds(0.1f);
        _bouncesRemaining = bounces;
    }
    
    public void SetDamage(float damage)
    {
        BULLET_DAMAGE = damage;
    }

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
                if(enemy.IsShootable())
                {
                    ToneDeathController.Instance.AssignHitPoints(BULLET_HIT_SCORE, _shooterIndex);
                    enemy.Damage(BULLET_DAMAGE);
                }
                Destroy(gameObject);
                break;
            }
            case "Bullet":
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

    internal IEnumerator IgnorePlayer(BoxCollider2D boxCollider2D, int playerIndex)
    {
        _shooterIndex = playerIndex;
    
        var script = GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(boxCollider2D, script, true);
        yield return new WaitForSeconds(0.25f);
        if (script != null && script.isActiveAndEnabled)
            Physics2D.IgnoreCollision(boxCollider2D, script, false);
    }

    private void Bounce_(Collision2D collision)
    {
        // flip
        var dir = _rigidBody.velocity;
        var norm = collision.contacts[0].normal;
        var vel = Vector3.Reflect(dir, norm);

        _rigidBody.velocity = vel;
        transform.eulerAngles *= -1;

        _bouncesRemaining--;
    }
}
