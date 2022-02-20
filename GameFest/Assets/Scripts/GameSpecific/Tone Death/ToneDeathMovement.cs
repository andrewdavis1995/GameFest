using UnityEngine;

public class ToneDeathMovement : MonoBehaviour
{
    const int MAX_BULLET_COUNT = 10;

    public Vector3 BulletOffset;
    public Transform Pointer;
    public Transform BulletPrefab;
    public ParticleSystem Particles;

    float _health = 100f;
    int _bulletCount = MAX_BULLET_COUNT;
    bool _particlesFiring = false;

    /// <summary>
    /// Fires a bullet
    /// </summary>
    public void Shoot()
    {
        if (_bulletCount > 0 && !_particlesFiring)
        {
            //// shoot
            var bullet = Instantiate(BulletPrefab, transform.position + BulletOffset, Quaternion.identity);
            StartCoroutine(bullet.GetComponent<BulletScript>().SetBounces(1));
            StartCoroutine(bullet.GetComponent<BulletScript>().IgnorePlayer(GetComponent<BoxCollider2D>()));
            bullet.transform.Rotate(Pointer.eulerAngles);

            _bulletCount--;
        }
    }

    /// <summary>
    /// Fires a bullet
    /// </summary>
    /// <param name="state">The state to set the particles in</param>
    public void ToggleParticles(bool state)
    {
        _particlesFiring = state;

        var emission = Particles.emission;
        emission.enabled = state;
        Particles.gameObject.SetActive(state);

        // TODO: enable collider
    }

    /// <summary>
    /// Reset the bullet count for the player
    /// </summary>
    public void ResetBulletCount()
    {
        _bulletCount = MAX_BULLET_COUNT;
    }

    /// <summary>
    /// The player has lost health
    /// </summary>
    /// <param name="damage">The amount of damage that was done</param>
    public void DamageDone(float damage)
    {
        _health -= damage;
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
