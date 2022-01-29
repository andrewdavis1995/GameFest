using System;
using System.Collections;
using UnityEngine;

public class BurgerScript : MonoBehaviour
{
    // colours
    float[] r = { 1f, 1f };
    float[] g = { 1f, 1f };
    float[] b = { 1f, 1f };
    Color[] _smokeColours = { new Color(1, 1, 1, 0), new Color(1, 1, 1, 0) };

    // components
    public SpriteRenderer Renderer;
    public SpriteRenderer RendererUnder;
    public Transform[] CookedSliders;
    public Transform PerfectlyCooked;
    public GameObject CookedBar;
    public GameObject Glow;
    [SerializeField]
    SpriteRenderer _grillMark;
    Rigidbody2D _rigidBody;
    ParticleSystem _particles;
    public AudioSource SizzleSound;

    // status variables
    bool _smoking = false;
    bool _flipping = false;
    float _smokeRate = 0.1f;
    int _sideIndex = 0;
    Coroutine _smokeCoroutine;
    Coroutine _rCoroutine;
    Coroutine _gCoroutine;
    Coroutine _bCoroutine;
    BurgerType _type;

    // Start is called before the first frame update
    void Start()
    {
        // get components
        _rigidBody = GetComponent<Rigidbody2D>();
        _particles = GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    /// When the item becomes active
    /// </summary>
    private void OnEnable()
    {
        // start changing the colour of the burger
        StartCooking_();
    }

    /// <summary>
    /// Starts the colour/smoke changing on the new side after a flip
    /// </summary>
    public IEnumerator StartNewSide()
    {
        _rigidBody.isKinematic = false;

        // flip the side index
        _sideIndex = (_sideIndex == 0) ? 1 : 0;

        yield return new WaitForSeconds(1);
        var threshold = BurgerPatty.MaxCookedLevel(_type);

        // check if a burger exists
        if (gameObject.activeInHierarchy)
        {
            // carry on with the smoke
            if (r[_sideIndex] < threshold)
            {
                _smokeCoroutine = StartCoroutine(Smoke_());

                _smokeRate = 0.1f * 2.5f;
            }
            else
            {
                // if not smoking, we return to invisible smoke
                var main = _particles.main;
                main.startColor = _smokeColours[_sideIndex];

                _smokeRate = 0.1f;
            }

            StartCooking_();
        }
    }

    /// <summary>
    /// Starts the coroutines that changes the colour of the burger
    /// </summary>
    void StartCooking_()
    {
        _rCoroutine = StartCoroutine(ColourRed_());
        _gCoroutine = StartCoroutine(ColourGreen_());
        _bCoroutine = StartCoroutine(ColourBlue_());

        CookedBar.SetActive(true);
        SizzleSound.Play();
    }

    /// <summary>
    /// Flips the burger
    /// </summary>
    void Flip_(Action callback)
    {
        if (_flipping) return;

        CookedBar.SetActive(false);
        SizzleSound.Stop();

        // stop smoking
        if (_smokeCoroutine != null)
            StopCoroutine(_smokeCoroutine);

        _flipping = true;

        // stop coroutines
        if (_rCoroutine != null) StopCoroutine(_rCoroutine);
        if (_gCoroutine != null) StopCoroutine(_gCoroutine);
        if (_bCoroutine != null) StopCoroutine(_bCoroutine);

        // move the burger up (flip it)
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

        transform.localPosition = new Vector3(startX, transform.localPosition.y, transform.localPosition.z);

        // execute callbaack
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

        var threshold = BurgerPatty.MaxCookedLevel(_type);

        // loop until zero
        while (r[_sideIndex] > 0)
        {
            r[_sideIndex] -= 0.0075f;

            CookedSliders[_sideIndex].localPosition = new Vector3(CookedSliders[_sideIndex].localPosition.x, 0.5f + (0 - r[_sideIndex]), CookedSliders[_sideIndex].localPosition.z);

            yield return new WaitForSeconds(_smokeRate * 2f);

            // when reaches a certain point, start smoking
            if (r[_sideIndex] < threshold && !_smoking)
            {
                _smokeRate *= 2.5f;
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
            g[_sideIndex] -= 0.0075f;
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
            b[_sideIndex] -= 0.0075f;
            yield return new WaitForSeconds(_smokeRate);
        }
    }

    /// <summary>
    /// Starts flipping the burger
    /// </summary>
    internal void Flip(Action action)
    {
        // can only flip if active
        if (!gameObject.activeInHierarchy) return;

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

        // stop movement
        transform.localPosition = new Vector3(transform.localPosition.x, 6f, transform.localPosition.z);
        GetComponent<BoxCollider2D>().enabled = true;
        _rigidBody.velocity = Vector3.zero;

        // reset smoke
        _smoking = false;
        _smokeRate = 0.1f;
        _flipping = false;

        // stop audio (belt & braces)
        SizzleSound.Stop();

        // reset colour
        r = new float[] { 1f, 1f };
        g = new float[] { 1f, 1f };
        b = new float[] { 1f, 1f };
        _smokeColours = new Color[] { new Color(1, 1, 1, 0), new Color(1, 1, 1, 0) };
        Renderer.color = new Color(1, 1, 1);

        // reset indicator
        _sideIndex = 0;
        CookedSliders[0].localPosition = new Vector3(CookedSliders[0].localPosition.x, -0.5f, CookedSliders[0].localPosition.z);
        CookedSliders[1].localPosition = new Vector3(CookedSliders[1].localPosition.x, -0.5f, CookedSliders[1].localPosition.z);
        CookedBar.SetActive(false);

        // if not smoking, we return to invisible smoke
        var main = _particles.main;
        main.startColor = _smokeColours[_sideIndex];

        // hide unused elements
        _rigidBody.isKinematic = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Cause smoke to appear from the burger
    /// </summary>
    IEnumerator Smoke_()
    {
        // we are now smoking
        _smoking = true;

        // get particles and set colour
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
        return Renderer.color;
    }

    /// <summary>
    /// Returns the colour of the back of the burger
    /// </summary>
    /// <returns>What colour the burger is (how well done)</returns>
    internal Color GetBurgerColourBack()
    {
        return RendererUnder.color;
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
        RendererUnder.color = new Color(r[_sideIndex], g[_sideIndex], b[_sideIndex]);
        Renderer.color = new Color(r[1 - _sideIndex], g[1 - _sideIndex], b[1 - _sideIndex]);

        var a = (r[1 - _sideIndex] < 0.8f) ? 1 - r[1 - _sideIndex] : 0f;
        _grillMark.color = new Color(0, 0, 0, a);
    }

    /// <summary>
    /// Sets the type of the burger in use
    /// </summary>
    /// <param name="burgerIndex">Index of the burger</param>
    internal void SetBurgerType(int burgerIndex)
    {
        _type = (BurgerType)Enum.Parse(typeof(BurgerType), burgerIndex.ToString());

        var max = 0f;
        var min = 0f;
        switch (_type)
        {
            // beef
            case BurgerType.Beef:
                min = BurgerPatty.MIN_COOKED_LEVEL_BEEF;
                max = BurgerPatty.MAX_COOKED_LEVEL_BEEF;
                break;
            // chicken
            case BurgerType.Chicken:
                min = BurgerPatty.MIN_COOKED_LEVEL_CHICKEN;
                max = BurgerPatty.MAX_COOKED_LEVEL_CHICKEN;
                break;
            // veggie
            case BurgerType.Veggie:
                min = BurgerPatty.MIN_COOKED_LEVEL_VEGGIE;
                max = BurgerPatty.MAX_COOKED_LEVEL_VEGGIE;
                break;
        }

        // get perfectly cooked area position
        var size = max - min;
        var middle = (max + min) / 2;

        // set perfectly cooked area position
        PerfectlyCooked.localPosition = new Vector3(PerfectlyCooked.localPosition.x, 0.5f + (0 - middle), PerfectlyCooked.localPosition.z);
        PerfectlyCooked.localScale = new Vector3(1, size, 1);
    }

    /// <summary>
    /// Get the type of burger this is
    /// </summary>
    /// <returns>The type of burger</returns>
    public BurgerType GetBurgerType()
    {
        return _type;
    }
}
