using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script for the glasses in Drink Slide
/// </summary>
public class DrinkObjectScript : MonoBehaviour
{
    float MAX_DAMAGE = 10f;
    float WATER_BOOST = 1.5f;

    int _playerIndex;
    float _health = 100f;

    public Transform GlassShardPrefab;
    public Transform SpillPrefab;
    public SpriteRenderer Renderer;
    public GameObject InZoneIndicator;
    public GameObject WinnerIndicator;

    Rigidbody2D _rigidbody;

    bool _inZone = false;

    public void Initialise(int playerIndex)
    {
        _playerIndex = playerIndex;
        GetComponent<SpriteRenderer>().color = ColourFetcher.GetColour(_playerIndex);
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Damage(float damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            StartCoroutine(DestroyGlass_());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "AreaTrigger":
                StartCoroutine(Fall_());
                break;
            case "PowerUp":
                _inZone = true;
                break;
            case "Water":
                _rigidbody.velocity *= WATER_BOOST;
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "PowerUp":
                _inZone = false;
                break;
        }
    }

    IEnumerator Fall_()
    {
        var a = 1f;
        var col = Renderer.color;
        while (a > 0)
        {
            transform.eulerAngles += new Vector3(0, 0, 1);
            transform.localScale -= Vector3.one * 0.05f;
            a -= 0.1f;
            Renderer.color = new Color(col.r, col.g, col.b, a);
            yield return new WaitForSeconds(0.1f);
        }

        // all gone
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var damage = (collision.relativeVelocity.x + collision.relativeVelocity.y) * MAX_DAMAGE;

        // little touches are allowed
        if (damage < MAX_DAMAGE)
            return;

        _health -= damage;

        if (_health <= 0)
            StartCoroutine(DestroyGlass_());
    }

    internal int GetPlayerIndex()
    {
        return _playerIndex;
    }

    IEnumerator DestroyGlass_()
    {
        Renderer.enabled = false;

        StartCoroutine(SpillDrink_());

        // spawn glass shards
        for (int i = 0; i < 10; i++)
        {
            var created = Instantiate(GlassShardPrefab, transform.position, Quaternion.identity);
            created.GetComponent<GlassShardScript>().Create();
            // TODO: Add force
        }

        // TODO: spawn spilled drink
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    private IEnumerator SpillDrink_()
    {
        var spill = Instantiate(SpillPrefab, transform.position + new Vector3(0, 0, 0.001f), Quaternion.identity);
        var col = ColourFetcher.GetColour(_playerIndex);
        col.a = 0.5f;
        spill.GetComponent<SpriteRenderer>().color = col;

        spill.localScale = Vector3.zero;

        for (float s = 0; s < 0.5f; s += 0.05f)
        {
            yield return new WaitForSeconds(0.1f);
            spill.localScale = new Vector3(s, s, 1);
        }
    }

    internal bool InZone()
    {
        return _inZone;
    }
}
