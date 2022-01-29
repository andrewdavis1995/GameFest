using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the behaviour of the carts
/// </summary>
public class MineDropItem : MonoBehaviour
{
    SpriteRenderer _renderer;

    /// <summary>
    /// Called once on startup
    /// </summary>
    void Start()
    {
        // get components
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = new Color(1, 1, 1, 0);

        // randomise size and rotation
        var random = Random.Range(0.35f, 1f);
        transform.localScale *= random;
        transform.eulerAngles = new Vector3(0, 0, Random.Range(-90, 90));

        // fade them in
        StartCoroutine(FadeIn_());
    }

    /// <summary>
    /// Quickly fade the sprite in
    /// </summary>
    IEnumerator FadeIn_()
    {
        float a = 0f;
        while (a <= 1f)
        {
            // change colour
            _renderer.color = new Color(1, 1, 1, a);
            yield return new WaitForSeconds(0.01f);
            a += 0.1f;
        }
    }

    /// <summary>
    /// The item is complete
    /// </summary>
    public void Finished()
    {
        StartCoroutine(FadeOut_());
    }

    /// <summary>
    /// Quickly fade the sprite out
    /// </summary>
    IEnumerator FadeOut_()
    {
        float a = 1f;
        while (a >= 0f)
        {
            // change colour
            _renderer.color = new Color(1, 1, 1, a);
            yield return new WaitForSeconds(0.01f);
            a -= 0.1f;
        }
        Destroy(gameObject);
    }
}
