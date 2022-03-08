using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DrinkSlideController : GenericController
{
    public static DrinkSlideController Instance;

    const float THROW_POWER = 1800f;
    const int NUM_THROWS_PER_ROUND = 3;
    const int NUM_ROUNDS = 3;
    const float ANGLE_CORRECTION = 90f;
    const int WINNING_STONE_POINTS = 150;
    const int IN_ZONE_POINTS = 30;
    const float MAXIMUM_DISTANCE = 3.5f;

    public Transform DrinkPrefab;
    public Transform TargetZone;
    public Vector3 StartPosition;
    public Sprite[] GlassShardSprites;
    public Vector3[] TargetPositions;
    public CameraLerp LerpControl;
    public ResultsPageScreen ResultsScreen;
    public TransitionFader EndFader;

    private Rigidbody2D _nextShot;
    List<DrinkSlideInputHandler> _players;
    float _rightCurve = 0;

    LineRenderer _line;

    Vector3 _direction = Vector3.zero;
    float _angle = 0;

    bool _showingRoundResults = false;
    bool _ended = false;
    bool _running = false;

    int _playerIndex = 0;
    int _throwIndex = 0;
    int _roundIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        _line = GetComponent<LineRenderer>();

        // TEMP
        _players = FindObjectsOfType<DrinkSlideInputHandler>().ToList();

        SpawnPlayers_();

        // initialise pause handler
        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        PauseGameHandler.Instance.Initialise(genericPlayers, QuitGame_);

        // fade in
        EndFader.GetComponentInChildren<Image>().sprite = PlayerManagerScript.Instance.GetFaderImage();
        EndFader.StartFade(1, 0, () => { PauseGameHandler.Instance.Pause(true, StartGame_); });
    }

    public bool GlassRunning()
    {
        return _running;
    }

    void StartGame_()
    {
        NextRound_();
    }

    void NextRound_()
    {
        DestroyDrinks_();

        _roundIndex++;

        Debug.Log(_roundIndex);

        if (_roundIndex < NUM_ROUNDS)
        {
            _playerIndex = 0;
            _throwIndex = 0;

            TargetZone.position = TargetPositions[_roundIndex];
            CreateDrink_();
        }
        else
        {
            StartCoroutine(Complete_());
        }
    }

    internal void UpdateSpin(int playerIndex, float spin)
    {
        Debug.Assert(playerIndex == _playerIndex, "Incorrect player was allowed to update spin");

        if (playerIndex == _playerIndex)
        {
            _nextShot.GetComponent<DrinkObjectScript>().UpdateSpin(spin);
        }
    }

    private void DrinksComplete_()
    {
        var drinks = FindObjectsOfType<DrinkObjectScript>();
        if (drinks.Count() > 0)
        {
            var ordered = drinks.OrderBy(d => Vector3.Distance(d.transform.position, TargetZone.position)).ToList();
            var winner = ordered.First().GetPlayerIndex();
            var index = 0;
            while (index < ordered.Count && ordered[index].GetPlayerIndex() == winner)
            {
                var distance = Vector3.Distance(ordered[index].transform.position, TargetZone.position);

                if (distance < MAXIMUM_DISTANCE)
                {
                    ordered[index].WinnerIndicator.SetActive(true);
                    ordered[index].WinnerIndicator.GetComponent<SpriteRenderer>().color = ColourFetcher.GetColour(winner);
                    _players[winner].AddPoints(WINNING_STONE_POINTS);
                }

                index++;
            }


            // points for being in zone
            foreach (var d in drinks)
            {
                if (d.InZone())
                {
                    d.InZoneIndicator.SetActive(true);
                    _players[d.GetPlayerIndex()].AddPoints(IN_ZONE_POINTS);
                }
            }
        }
    }

    void DestroyDrinks_()
    {
        var drinks = FindObjectsOfType<DrinkObjectScript>();

        // points for being in zone
        foreach (var d in drinks)
        {
            Destroy(d.gameObject);
        }

        StartCoroutine(DestroyWater_());
    }

    private IEnumerator DestroyWater_()
    {
        var water = GameObject.FindGameObjectsWithTag("Water").Select(s => s.GetComponent<SpriteRenderer>()).ToList();
        if (water.Count > 0)
        {
            var a = water.First().color.a;

            while (a > 0)
            {
                foreach (var w in water)
                {
                    var c = w.color;
                    w.color = new Color(c.r, c.g, c.b, a);
                    a -= 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // dispose all
            foreach (var w in water)
            {
                Destroy(w.gameObject);
            }
        }
    }

    /// <summary>
    /// Callback for when the player quits
    /// </summary>
    private void QuitGame_()
    {
        _ended = true;
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        PlayerManagerScript.Instance.CentralScene();
    }

    /// <summary>
    /// Can't pause once we get to the results section
    /// </summary>
    /// <returns>Whether the game can be paused at the current stage</returns>
    public override bool CanPause()
    {
        return !_ended;
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(DrinkSlideInputHandler));

            player.Spawn(null, Vector2.zero);
            var ih = player.GetComponent<DrinkSlideInputHandler>();

            _players.Add(ih);
        }
    }

    public void Fire(int playerIndex, float angle, float powerMultiplier)
    {
        System.Diagnostics.Debug.Assert(playerIndex == _playerIndex, "Incorrect player was allowed to fire");

        if (playerIndex == _playerIndex)
            Throw_(THROW_POWER * powerMultiplier, angle);
    }

    public void UpdatePointer(int playerIndex, Vector3 direction, float angle)
    {
        Debug.Assert(playerIndex == _playerIndex, "Incorrect player was allowed to update pointer");

        if (playerIndex == _playerIndex)
        {
            _direction = direction;
            _angle = angle;
        }
    }

    private void Update()
    {
        if (_line.enabled && _nextShot != null && _nextShot.gameObject.activeInHierarchy)
        {
            _line.positionCount = 2;
            var position = _nextShot.transform.position - (5 * _direction);
            //var midPos = ((position + _nextShot.transform.position) / 2) + new Vector3(_direction.y * _rightCurve, 0, 0);

            _line.SetPositions(new Vector3[] { _nextShot.transform.position, position });

            _nextShot.transform.eulerAngles = new Vector3(0, 0, _angle);
        }
    }

    public void UpdateCurve(int playerIndex, float curve)
    {
        Debug.Assert(playerIndex == _playerIndex, "Incorrect player was allowed to update curve");

        if (playerIndex == _playerIndex)
        {
            _rightCurve = curve;
        }
    }

    public void Throw_(float force, float angle)
    {
        angle += ANGLE_CORRECTION;

        _line.enabled = false;
        _line.SetPositions(new Vector3[] { });

        float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * force;
        float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * force;

        _nextShot.AddForce(new Vector2(xcomponent, ycomponent));

        _running = true;
        StartCoroutine(_nextShot.GetComponent<DrinkObjectScript>().SpinMovement());
        StartCoroutine(CheckForShotEnd_());
    }

    private IEnumerator CheckForShotEnd_()
    {;
        yield return new WaitForSeconds(1f);

        while (_nextShot != null && _nextShot.gameObject.activeInHierarchy && _nextShot.velocity.y > 0.01f)
        {
            yield return new WaitForSeconds(0.1f);

            // stop spin when too slow
            if (_nextShot != null && _nextShot.gameObject.activeInHierarchy && _nextShot.velocity.y < 0.5f)
                _running = false;
        }

        // make sure spin stops
        _running = false;

        yield return new WaitForSeconds(1f);

        NextShot_();
    }

    private void NextShot_()
    {
        _playerIndex++;
        if (_playerIndex >= _players.Count)
        {
            _playerIndex = 0;
            _throwIndex++;
        }

        if (_throwIndex >= NUM_THROWS_PER_ROUND)
            StartCoroutine(RoundResults_());
        else
            CreateDrink_();
    }

    private IEnumerator RoundResults_()
    {
        _showingRoundResults = true;

        DrinksComplete_();

        LerpControl.Position = TargetZone.position;
        LerpControl.Zoom = 3;

        yield return new WaitForSeconds(7);

        _showingRoundResults = false;

        LerpControl.Zoom = 10;
        LerpControl.Position = new Vector3(0, 0, -10);

        yield return new WaitForSeconds(2f);

        NextRound_();
    }

    private void CreateDrink_()
    {
        var item = Instantiate(DrinkPrefab, StartPosition, Quaternion.identity);
        _nextShot = item.GetComponent<Rigidbody2D>();
        var drinkScript = _nextShot.GetComponent<DrinkObjectScript>();
        drinkScript.Initialise(_playerIndex);
        _nextShot.GetComponent<SpriteRenderer>().sortingOrder = 1;

        _line.enabled = true;

        // enable next player
        foreach (var p in _players)
            p.IsActive(p.GetPlayerIndex() == _playerIndex);
    }

    public Sprite GetRandomGlassShard()
    {
        var index = UnityEngine.Random.Range(0, GlassShardSprites.Length);
        return GlassShardSprites[index];
    }

    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // sort the players by points scored
        var ordered = _players.Where(p => p.GetPoints() > 0).OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 100, 60, 15 };

        // add winning score points 
        for (int i = 0; i < ordered.Count(); i++)
        {
            if (ordered[i].GetPoints() > 0)
            {
                ordered[i].AddPoints(winnerPoints[i]);
                ordered[i].SetBonusPoints(winnerPoints[i]);
            }
        }

        // set the winner
        ordered.FirstOrDefault()?.Winner();
    }

    /// <summary>
    /// Completes the game and return to object
    /// </summary>
    IEnumerator Complete_()
    {
        _ended = true;
        AssignBonusPoints_();

        yield return new WaitForSeconds(3f);

        ResultsScreen.Setup();

        GenericInputHandler[] genericPlayers = _players.ToArray<GenericInputHandler>();
        ResultsScreen.SetPlayers(genericPlayers);

        ScoreStoreHandler.StoreResults(Scene.DrinkSlide, genericPlayers);

        yield return new WaitForSeconds(4 + genericPlayers.Length);

        // fade out
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }
}
