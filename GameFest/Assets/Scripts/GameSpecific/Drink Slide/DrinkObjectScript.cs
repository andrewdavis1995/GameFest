using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script for the glasses in Drink Slide
/// </summary>
public class DrinkObjectScript : MonoBehaviour
{
    const float MAX_DAMAGE = 10f;
    const float WATER_BOOST = 1.4f;
    const float TIMEOUT = 45f;

    int _playerIndex;
    float _health = 100f;
    float _spin = 0f;

    public Transform GlassShardPrefab;
    public Transform SpillPrefab;
    public SpriteRenderer Renderer;
    public GameObject InZoneIndicator;
    public GameObject WinnerIndicator;
    public TextMesh[] Texts;

    public Rigidbody2D Rigidbody;

    SpriteRenderer[] _allRenderers;

    TargetScript _inZone = null;
    bool _falling = false;

    public void Initialise(int playerIndex)
    {
        _playerIndex = playerIndex;
        Renderer.color = ColourFetcher.GetColour(_playerIndex);
    }

    private void Start()
    {
        _allRenderers = GetComponentsInChildren<SpriteRenderer>();
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
                _inZone = collision.GetComponentInParent<TargetScript>();
                break;
            case "Water":
                if ((Rigidbody.velocity.x + Rigidbody.velocity.y) < 7f)
                    Rigidbody.velocity *= WATER_BOOST;
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "PowerUp":
                _inZone = null;
                break;
        }
    }

    /// <summary>
    /// Moves the ball as the "wind" blows it
    /// </summary>
    public IEnumerator SpinMovement()
    {
        // continue while running/rolling
        while (!_falling && Rigidbody != null && Rigidbody.gameObject.activeInHierarchy)
        {
            var _windStrength = new Vector2(1, 0);
            Rigidbody.AddForce(_windStrength * 5 * new Vector2(_spin, 0) * Rigidbody.velocity.y);
            transform.eulerAngles -= new Vector3(0, 0, Rigidbody.velocity.y * _spin);
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Fall_()
    {
        _falling = true;

        var a = 1f;
        var col = Renderer.color;
        while (a > 0 && transform.localScale.x > 0)
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

    internal IEnumerator FadeTexts()
    {
        while (Texts[0].color.a > 0)
        {
            Texts[0].color = new Color(1, 1, 1, Texts[0].color.a - 0.1f);
            Texts[1].color = new Color(1, 1, 1, Texts[1].color.a - 0.1f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    internal void UpdateSpin(float spin)
    {
        _spin = spin;
    }

    internal int GetPlayerIndex()
    {
        return _playerIndex;
    }

    public IEnumerator DrinkTimeout()
    {
        yield return new WaitForSeconds(TIMEOUT);

        try
        {
            if (gameObject != null && gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
            }
        }
        // make sure it is destroyed
        catch (Exception) { }
    }

    IEnumerator DestroyGlass_()
    {
        foreach (var r in _allRenderers)
            r.enabled = false;

        StartCoroutine(SpillDrink_());

        // spawn glass shards
        for (int i = 0; i < 15; i++)
        {
            var created = Instantiate(GlassShardPrefab, transform.position, Quaternion.identity);
            created.GetComponent<GlassShardScript>().Create();
        }

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

    internal TargetScript InZone()
    {
        return _inZone;
    }

    public Rigidbody2D GetRigidBody()
    {
        return Rigidbody;
    }
}
