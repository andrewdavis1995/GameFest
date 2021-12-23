using System.Collections;
using System.Linq;
using UnityEngine;

public class DropBombScript : MonoBehaviour
{
    public Transform Explosion;
    bool _active = false;
    public CircleCollider2D Trigger;
    public AudioSource BombNoise;

    /// <summary>
    /// Waits, then detonates
    /// </summary>
    public IEnumerator Detonate()
    {
        _active = true;
        yield return new WaitForSeconds(2);

        StartCoroutine(ExpandBoom_());

        BombNoise.Play();

        // stop it moving
        GetComponent<Rigidbody2D>().isKinematic = true;

        var trigger = GetComponents<CircleCollider2D>().Where(c => c.isTrigger).FirstOrDefault();

        // expand to destroy balls
        trigger.radius = 0.85f;
        yield return new WaitForSeconds(0.1f);

        Trigger.enabled = false;

        // create explosion
        yield return new WaitForSeconds(0.44f);

        _active = false;

        trigger.radius = 0f;
        yield return new WaitForSeconds(0.5f);

        // remove boom
        Destroy(gameObject);
    }

    /// <summary>
    /// Waits, then detonates
    /// </summary>
    public IEnumerator ExpandBoom_()
    {
        while (_active)
        {
            Explosion.transform.localScale += new Vector3(0.044f, 0.044f);
            yield return new WaitForSeconds(0.003f);
        }
    }
}
