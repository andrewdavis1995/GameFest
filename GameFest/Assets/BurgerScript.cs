using System;
using System.Collections;
using UnityEngine;

public class BurgerScript : MonoBehaviour
{
    // constants
    const float SMOKE_THRESHOLD = .6f;

    // colours
    float[] r = { 1f, 1f };
    float[] g = { 1f, 1f };
    float[] b = { 1f, 1f };
    Color[] _smokeColours = { new Color(1, 1, 1, 0), new Color(1, 1, 1, 0) };

    // components
    SpriteRenderer _renderer;
    ParticleSystem _particles;

    // status variables
    bool _smoking = false;
    float _smokeRate = 0.15f;
    int _sideIndex = 0;
    Coroutine _smokeCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // get components
        _renderer = GetComponent<SpriteRenderer>();
        _particles = GetComponentInChildren<ParticleSystem>();

        // start changing the colour of the burger
        StartCoroutine(ColourRed_());
        StartCoroutine(ColourGreen_());
        StartCoroutine(ColourBlue_());
    }

    /// <summary>
    /// Flips the burger
    /// </summary>
    IEnumerator Flip_()
    {
        // stop smoking
        if (_smokeCoroutine != null)
            StopCoroutine(_smokeCoroutine);

        // flip the side index
        _sideIndex = (_sideIndex == 0) ? 1 : 0;

        // carry on with the smoke
        if (r[_sideIndex] < SMOKE_THRESHOLD)
        {
            _smokeCoroutine = StartCoroutine(Smoke_());
        }
        else
        {
            // if not smoking, we return to invisible smoke
            var main = _particles.main;
            main.startColor = _smokeColours[_sideIndex];
        }

        yield return new WaitForSeconds(1);
    }

    /// <summary>
    /// Changes the r aspect of the colour
    /// </summary>
    IEnumerator ColourRed_()
    {
        // wait briefly
        yield return new WaitForSeconds(1);

        // loop until zero
        while (r[_sideIndex] > 0)
        {
            r[_sideIndex] -= 0.01f;
            yield return new WaitForSeconds(_smokeRate * 2f);

            // when reaches a certain point, start smoking
            if (r[_sideIndex] < SMOKE_THRESHOLD && !_smoking)
            {
                _smokeRate *= 2;
                _smokeCoroutine = StartCoroutine(Smoke_());
            }
        }
    }

    /// <summary>
    /// Changes the g aspect of the colour
    /// </summary>
    IEnumerator ColourGreen_()
    {
        yield return new WaitForSeconds(1);

        // loop until zero
        while (g[_sideIndex] > 0)
        {
            g[_sideIndex] -= 0.01f;
            yield return new WaitForSeconds(_smokeRate * 1.17f);
        }
    }

    /// <summary>
    /// Changes the b aspect of the colour
    /// </summary>
    IEnumerator ColourBlue_()
    {
        yield return new WaitForSeconds(1);

        // loop until zero
        while (b[_sideIndex] > 0)
        {
            b[_sideIndex] -= 0.01f;
            yield return new WaitForSeconds(_smokeRate);
        }
    }

    /// <summary>
    /// Starts flipping the burger
    /// </summary>
    internal void Flip()
    {
        StartCoroutine(Flip_());
    }

    /// <summary>
    /// Cause smoke to appear from the burger
    /// </summary>
    IEnumerator Smoke_()
    {
        Debug.Log("Starting: " + _sideIndex);

        // we are now smoking
        _smoking = true;

        // get particles and ser colour
        var main = _particles.main;

        // loop while the burger is smoking
        while (_smoking)
        {
            // set the colour of the smoke
            main.startColor = _smokeColours[_sideIndex];

            yield return new WaitForSeconds(0.5f);

            // update the colour of the smoke
            _smokeColours[_sideIndex].a += 0.01f;
            _smokeColours[_sideIndex].r -= 0.011f;
            _smokeColours[_sideIndex].g -= 0.011f;
            _smokeColours[_sideIndex].b -= 0.011f;
        }
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    private void Update()
    {
        // update the colour of the burger
        _renderer.color = new Color(r[_sideIndex], g[_sideIndex], b[_sideIndex]);
    }
}
