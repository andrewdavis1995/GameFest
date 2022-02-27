using System;
using UnityEngine;

public class ToneDeathMovement : MonoBehaviour
{
    const int MAX_BULLET_COUNT = 10;

    public Vector3 BulletOffset;
    public Transform Pointer;
    public Transform BulletPrefab;
    public ParticleSystem Particles;
    public ParticleSystemRenderer ParticlesInstrument;
    public SpriteRenderer HealthBarFill;
    public GameObject HealthBar;

    float _health = 100f;
    int _bulletCount = MAX_BULLET_COUNT;
    bool _particlesFiring = false;
    int _playerIndex;

    Action _deathCallback;

    /// <summary>
    /// Sets the index of the player linked with this movement
    /// </summary>
    /// <param name="index">Player Index</param>
    /// <param name="deathCallback">Function to call when player dies</param>
    public void Setup(int index, Action deathCallback)
    {
        _playerIndex = index;
        _deathCallback = deathCallback;
    }

    /// <summary>
    /// Fires a bullet
    /// </summary>
    public void Shoot()
    {
        if (_bulletCount > 0 && !_particlesFiring)
        {
            // shoot
            var bullet = Instantiate(BulletPrefab, transform.position + BulletOffset, Quaternion.identity);
            StartCoroutine(bullet.GetComponent<BulletScript>().SetBounces(1));
            StartCoroutine(bullet.GetComponent<BulletScript>().IgnorePlayer(GetComponent<BoxCollider2D>(), _playerIndex));
            bullet.GetComponent<BulletScript>().SetSpeed(10f);
            bullet.transform.Rotate(Pointer.eulerAngles);

            _bulletCount--;
        }
    }

    /// <summary>
    /// Sets the state of the particle beam
    /// </summary>
    /// <param name="state">The state to set the particles in</param>
    public void ToggleParticles(bool state)
    {
        _particlesFiring = state;

        // set particles state
        var emission = Particles.emission;
        emission.enabled = state;
        Particles.gameObject.SetActive(state);
    }

    /// <summary>
    /// Reset the bullet count for the player
    /// </summary>
    public void ResetBulletCount()
    {
        _bulletCount = MAX_BULLET_COUNT;
    }

    /// <summary>
    /// Updates the health bar
    /// </summary>
    void UpdateHealthImage_()
    {
        // calculate size and position
        var width = _health / 100f;
        var left = -1 * (((100 - _health) / 100) / 2);

        // displays the current health
        HealthBarFill.size = new Vector2(width, 1);
        HealthBarFill.transform.localPosition = new Vector3(left, 0, 0);

        // set color
        var r = _health <= 50 ? 1f : ((100 - _health) / 50);
        var g = _health >= 50 ? 1f : (_health / 50f);
        HealthBarFill.color = new Color(r, g, 0);
    }

    /// <summary>
    /// The player has lost health
    /// </summary>
    /// <param name="damage">The amount of damage that was done</param>
    public void DamageDone(float damage)
    {
        // update health
        _health -= damage;

        if (_health < 0) _health = 0f;
        UpdateHealthImage_();

        // die
        if (_health <= 0)
            _deathCallback?.Invoke();
    }

    /// <summary>
    /// Accessor for health
    /// </summary>
    /// <returns>The health of the player</returns>
    public float Health()
    {
        return _health;
    }
}
