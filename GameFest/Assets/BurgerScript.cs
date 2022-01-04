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
    [SerializeField]
    SpriteRenderer _renderer;
    [SerializeField]
    SpriteRenderer _grillMark;
    [SerializeField]
    SpriteRenderer _rendererUnder;
    Rigidbody2D _rigidBody;
    ParticleSystem _particles;

    // status variables
    bool _smoking = false;
    bool _flipping = false;
    float _smokeRate = 0.15f;
    int _sideIndex = 0;
    Coroutine _smokeCoroutine;
    Coroutine _rCoroutine;
    Coroutine _gCoroutine;
    Coroutine _bCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // get components
        _rigidBody = GetComponent<Rigidbody2D>();
        _particles = GetComponentInChildren<ParticleSystem>();

        // start changing the colour of the burger
        _rCoroutine = StartCoroutine(ColourRed_());
        _gCoroutine = StartCoroutine(ColourGreen_());
        _bCoroutine = StartCoroutine(ColourBlue_());
    }

    /// <summary>
    /// Starts the colour/smoke changing on the new side after a flip
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartNewSide()
    {
        _rigidBody.isKinematic = false;

        // flip the side index
        _sideIndex = (_sideIndex == 0) ? 1 : 0;

        yield return new WaitForSeconds(1);

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

        _rCoroutine = StartCoroutine(ColourRed_());
        _gCoroutine = StartCoroutine(ColourGreen_());
        _bCoroutine = StartCoroutine(ColourBlue_());

    }

    /// <summary>
    /// Flips the burger
    /// </summary>
    void Flip_(Action callback)
    {
        if (_flipping) return;

        // stop smoking
        if (_smokeCoroutine != null)
            StopCoroutine(_smokeCoroutine);

        _flipping = true;

        // stop coroutines
        if (_rCoroutine != null) StopCoroutine(_rCoroutine);
        if (_gCoroutine != null) StopCoroutine(_gCoroutine);
        if (_bCoroutine != null) StopCoroutine(_bCoroutine);

        StartCoroutine(MoveUp_(transform.localPosition + new Vector3(0, 10f), callback));
        _rigidBody.isKinematic = true;
        _smoking = false;
    }

    /// <summary>
    /// Moves the burger up - i.e. does the flipping motion
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private IEnumerator MoveUp_(Vector3 targetPosition, Action callback)
    {
        var startX = transform.localPosition.x;

        // continue until reached the top
        while (transform.localPosition.y < targetPosition.y)
        {
            yield return new WaitForSeconds(0.001f);
            transform.Translate(new Vector3(0, 0.6f, 0));
        }

        transform.eulerAngles = new Vector3(0, 0, 0);
        transform.localPosition = new Vector3(startX, transform.localPosition.y, transform.localPosition.z);

        callback?.Invoke();
    }

    /// <summary>
    /// Is the burger currently flipping?
    /// </summary>
    /// <returns>Whether the burger is in the process of being flipped</returns>
    internal bool Flipping()
    {
        return _flipping;
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
    internal void Flip(Action action)
    {
        Flip_(action);
    }

    /// <summary>
    /// When the burger collides with another collider
    /// </summary>
    /// <param name="collision">The collider it hit</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        _flipping = false;
    }

    /// <summary>
    /// Resets the state of the burger for use next time
    /// </summary>
    internal void ResetBurger()
    {
        StopAllCoroutines();

        // reset smoke
        _smoking = false;
        _smokeRate = 0.15f;
        _flipping = false;

        // reset colour
        r = new float[] { 1f, 1f };
        g = new float[] { 1f, 1f };
        b = new float[] { 1f, 1f };
        _smokeColours = new Color[] { new Color(1, 1, 1, 0), new Color(1, 1, 1, 0) };
        _renderer.color = new Color(1, 1, 1);

        // if not smoking, we return to invisible smoke
        var main = _particles.main;
        main.startColor = _smokeColours[_sideIndex];
    }

    /// <summary>
    /// Cause smoke to appear from the burger
    /// </summary>
    IEnumerator Smoke_()
    {
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
    /// Returns the colour of the burger
    /// </summary>
    /// <returns>What colour the burger is (how well done)</returns>
    internal Color GetBurgerColour()
    {
        return _renderer.color;
    }

    /// <summary>
    /// Returns the colour of the back of the burger
    /// </summary>
    /// <returns>What colour the burger is (how well done)</returns>
    internal Color GetBurgerColourBack()
    {
        return _rendererUnder.color;
    }

    /// <summary>
    /// Returns the colour of the grill marks
    /// </summary>
    /// <returns>What colour the burger is (how well done)</returns>
    internal Color GetBurgerColourGrill()
    {
        return _grillMark.color;
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    private void Update()
    {
        // update the colour of the burger
        _rendererUnder.color = new Color(r[_sideIndex], g[_sideIndex], b[_sideIndex]);
        _renderer.color = new Color(r[1 - _sideIndex], g[1 - _sideIndex], b[1 - _sideIndex]);

        var a = (r[1 - _sideIndex] < 0.8f) ? 1 - r[1 - _sideIndex] : 0f;
        _grillMark.color = new Color(0, 0, 0, a);
    }
}
