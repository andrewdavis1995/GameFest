using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class RocketResultScript : MonoBehaviour
{
    private const int MHATHIA_POINTS = 100;

    // unity configuration
    public SpriteRenderer Rocket;
    public SpriteRenderer Player;
    public TextMesh TxtPlayerName;
    public TextMesh TxtScore;
    public Sprite[] CharacterImages;
    public SpriteRenderer[] Propulsion;

    // status variables
    bool _started = false;
    bool _complete = false;
    bool _mhathiaReached = false;
    List<int> _values = new List<int>();
    float _moveSpeed = 6f;
    bool _powerRemaining = false;
    int _score = 0;
    Action<int> _pointsCallback;
    int _playerIndex;
    float _mhathiaCentreX;
    float _mhathiaTriggerX;

    void Start()
    {
        var mhathia = GameObject.Find("Mhathia").transform;

        _mhathiaCentreX = mhathia.localPosition.x;
        _mhathiaTriggerX = _mhathiaCentreX - 60f;
    }

    // Update is called once per frame
    void Update()
    {
        // do nothing if complete - wait for others
        if (!_complete && _started && !_mhathiaReached)
        {
            // move the rocket
            transform.Translate(new Vector3(_moveSpeed * Time.deltaTime, 0));

            // if no longer accelerating, decrease speed
            if (!_powerRemaining)
            {
                _moveSpeed -= 0.03f;

                // stop once no more battery
                if (_moveSpeed <= 0f)
                {
                    _complete = true;
                    StartCoroutine(DelayBeforeComplete());
                }
            }

            if(!_mhathiaReached && transform.localPosition.x > _mhathiaTriggerX && _mhathiaTriggerX > 0)
            {
                _pointsCallback(MHATHIA_POINTS);
                _mhathiaReached = true;
            }
        }
        else if(_mhathiaReached && !_complete && _started)
        {
            // shrink
            transform.localScale -= new Vector3(0.05f* Time.deltaTime, 0.05f * Time.deltaTime, 0);
            var yOffset = 0f;
            switch(_playerIndex)
            {
                case 0: yOffset = -0.03f; break;
                case 1: yOffset = -0.015f; break;
            }
            transform.Translate(new Vector3(_moveSpeed * Time.deltaTime *1.5f, yOffset));

            if (transform.localPosition.x >= _mhathiaCentreX || transform.localScale.x <= 0)
            {
                _moveSpeed = 0;
                _complete = true;
                StartCoroutine(DelayBeforeComplete());
            }
        }
    }

    /// <summary>
    /// Delay slightly before completing
    /// </summary>
    private IEnumerator DelayBeforeComplete()
    {
        yield return new WaitForSeconds(2);
        XTinguishController.Instance.CheckResultsComplete();
    }

    /// <summary>
    /// Initialises the result display with all required data
    /// </summary>
    /// <param name="points">List of items the player picked up</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="playerIndex">Index of the player</param>
    /// <param name="died">Whether the player died</param>
    /// <param name="characterindex">Index of the character being used</param>
    internal void Initialise(List<int> points, string playerName, int playerIndex, bool died, int characterindex, Action<int> addPointsCallback)
    {
        Rocket.color = ColourFetcher.GetColour(playerIndex);
        _values = points;
        TxtPlayerName.text = playerName;
        Player.sprite = CharacterImages[characterindex];
        _pointsCallback = addPointsCallback;
        _playerIndex = playerIndex;

        // if the player died, disable the rocket
        if (died)
        {
            _moveSpeed = 0;
            Rocket.transform.eulerAngles = Vector3.zero;

            // hide it
            Rocket.gameObject.SetActive(false);
            TxtPlayerName.text = "";
            TxtScore.text = "";
            _complete = true;
        }

        if(points.Count == 0)
        {
            _moveSpeed = 0;
            _complete = true;
        }
    }

    /// <summary>
    /// Starts the process of moving the item forward, and depletes power
    /// </summary>
    public void StartMove()
    {
        _started = true;
        _powerRemaining = true;
        StartCoroutine(ProcessPower_());
    }

    /// <summary>
    /// Processes each of the stored power values sequentially
    /// </summary>
    IEnumerator ProcessPower_()
    {
        int index = 0;

        // random delay
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.25f));

        // loop through each power source
        foreach (var value in _values)
        {
            int currentValue = value;
            do
            {
                // deplete value
                currentValue -= 10;

                // display score
                _score += 10;
                TxtScore.text = _score.ToString();

                // wait briefly - speed should be adjustable
                yield return new WaitForSeconds(.3f);
            } while (currentValue > 0);

            index++;

            // random delay
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.25f));
        }

        // no longer has power - glide
        _powerRemaining = false;

        StartCoroutine(KillPower_());
    }

    /// <summary>
    /// Hides the propulsion graphics
    /// </summary>
    private IEnumerator KillPower_()
    {
        foreach (var p in Propulsion) p.color = new Color(p.color.r, p.color.g, p.color.b, 0.4f);
        yield return new WaitForSeconds(0.1f);
        foreach (var p in Propulsion) p.color = new Color(p.color.r, p.color.g, p.color.b, 0.9f);
        yield return new WaitForSeconds(0.2f);
        foreach (var p in Propulsion) p.color = new Color(p.color.r, p.color.g, p.color.b, 0.1f);
        yield return new WaitForSeconds(0.1f);
        foreach (var p in Propulsion) p.color = new Color(p.color.r, p.color.g, p.color.b, 0.7f);
        yield return new WaitForSeconds(0.2f);
        foreach (var p in Propulsion) p.color = new Color(p.color.r, p.color.g, p.color.b, 0.2f);
        yield return new WaitForSeconds(0.1f);
        foreach (var p in Propulsion) p.color = new Color(p.color.r, p.color.g, p.color.b, 0.8f);
        yield return new WaitForSeconds(0.2f);

        while(Propulsion[0].color.a > 0)
        {
            foreach (var p in Propulsion) p.color = new Color(p.color.r, p.color.g, p.color.b, p.color.a - 0.1f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Checks if the player result is complete
    /// </summary>
    /// <returns>Whether the player is complete</returns>
    internal bool IsComplete()
    {
        return _complete;
    }
}
