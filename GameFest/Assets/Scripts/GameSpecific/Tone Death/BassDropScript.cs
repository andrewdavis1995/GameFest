using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BassDropScript : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public Transform Blast;
    public CircleCollider2D Collider;

    static float SPEED = 1200f;
    public float DAMAGE = 60f;
    public float MAX_SIZE = 40f;

    Rigidbody2D _rigidBody;

    /// <summary>
    /// Fires a bass drop item
    /// </summary>
    public void Fire()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.AddForce(transform.up * SPEED);
    }

    /// <summary>
    /// When this item collides wwith another
    /// </summary>
    /// <param name="collision">The item that was collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Player":
                // detonate & damage
                collision.transform.GetComponent<ToneDeathMovement>().DamageDone(DAMAGE);
                StartCoroutine(Denonate_());
                break;
            case "Ground":
                // detonate
                StartCoroutine(Denonate_());
                break;
        }
    }

    /// <summary>
    /// Ignores the enemy who fired the bass drop
    /// </summary>
    /// <param name="boxCollider2D">The player to ignore</param>
    internal void IgnoreShooter(BoxCollider2D boxCollider2D)
    {
        // ignore collision
        var collider = GetComponent<CircleCollider2D>();
        Physics2D.IgnoreCollision(boxCollider2D, collider, true);

        // temporarily ignore all
        StartCoroutine(IgnoreAll_());
    }

    /// <summary>
    /// Ignores all colliders for 1 second
    /// </summary>
    private IEnumerator IgnoreAll_()
    {
        Collider.enabled = false;
        yield return new WaitForSeconds(1);
        Collider.enabled = true;
    }

    /// <summary>
    /// Sets how much damage is done by this projectile
    /// </summary>
    /// <param name="damage"></param>
    public void SetDamage(float damage)
    {
        DAMAGE = damage;
    }

    /// <summary>
    /// Detonates the bass drop
    /// </summary>
    IEnumerator Denonate_()
    {
        // hide ball
        Renderer.enabled = false;
        transform.eulerAngles = new Vector3(0, 0, 0);
        _rigidBody.freezeRotation = true;
        Blast.gameObject.SetActive(true);

        // grow blast
        for (float i = 0f; i < MAX_SIZE; i += 1f)
        {
            Blast.localScale = new Vector3(i, Blast.localScale.y, 0);
            yield return new WaitForSeconds(0.01f);
        }

        // destroy object
        Destroy(gameObject);
    }
}
