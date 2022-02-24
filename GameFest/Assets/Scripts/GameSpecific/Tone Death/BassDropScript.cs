using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BassDropScript : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public Transform Blast;

    static float SPEED = 1200f;
    Rigidbody2D _rigidBody;
    float BULLET_DAMAGE = 15f;
    int BULLET_HIT_SCORE = 10;

    public void Fire()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.AddForce(transform.up * SPEED);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Player":
                // TODO: Detonate and lose points
                StartCoroutine(Denonate_());
                break;
            case "Ground":
                // TODO: Detonate
                StartCoroutine(Denonate_());
                break;
        }
    }

    internal void IgnoreShooter(BoxCollider2D boxCollider2D)
    {
        var collider = GetComponent<CircleCollider2D>();
        Physics2D.IgnoreCollision(boxCollider2D, collider, true);
    }

    public void SetDamage(float damage)
    {
        BULLET_DAMAGE = damage;
    }

    IEnumerator Denonate_()
    {
        Renderer.enabled = false;
        transform.eulerAngles = new Vector3(0, 0, 0);
        _rigidBody.freezeRotation = true;
        Blast.gameObject.SetActive(true);
        // TODO: grow size
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
