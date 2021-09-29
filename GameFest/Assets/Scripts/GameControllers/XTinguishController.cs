using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class XTinguishController : GenericController
{
    // configuration
    private const int GAME_TIMEOUT = 120;
    const float END_GAME_TIMEOUT = 5f;

    // unity configuration
    public TextMesh TxtCountdown;
    public Vector2 TopLeft;
    public Vector2 BottomRight;
    public Transform BatteryPrefab;
    public BoxCollider2D[] FireCollidersX;
    public BoxCollider2D[] FireCollidersY;
    public Transform PlayerPrefab;
    public Vector3[] SpawnPositions;
    public Vector3[] TransportPositions;
    public Vector3 ResultsSpawnPositionsTop;
    public Vector3 ResultCameraPosition;
    public Transform RocketPrefab;
    public CameraFollow CameraFollowScript;
    public CameraZoomFollow CameraZoomFollowScript;
    public Transform[] Rockets;
    public SpriteRenderer AlarmIndicator;
    public SpriteRenderer AlarmOverlay;
    public TransitionFader Fader;
    public ResultsPageScreen ResultsScreen;

    // fire encroachment
    private float _fireMoveX = 0.0015f;
    private float _fireMoveY = 0.001f;

    // battery spawn times
    float _minBatteryWait = 4;
    float _maxBatteryWait = 9;

    // end scene
    bool _fastFireMove = false;
    bool _ended = false;
    float _rocketSpeed = 0.1f;

    // static instance
    public static XTinguishController Instance;

    // status variables
    bool _active = false;
    List<XTinguishInputHandler> _players = new List<XTinguishInputHandler>();

    // time out
    TimeLimit _overallLimit;

    // links to other scripts
    List<RocketResultScript> _endRockets = new List<RocketResultScript>();

    // Start is called before the first frame update
    void Start()
    {
        // create static instance
        Instance = this;

        // initialise the timeout
        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _overallLimit.Initialise(GAME_TIMEOUT, OnTimeLimitTick, OnTimeUp);

        // spawn player objects
        SpawnPlayers_();

        // setup pause
        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        PauseGameHandler.Instance.Initialise(genericPlayers);

        // fade in
        Fader.StartFade(1, 0, FadeInComplete);
    }

    /// <summary>
    /// Callback for when the fade image has fully faded out
    /// </summary>
    void FadeInComplete()
    {
        PauseGameHandler.Instance.Pause(true, StartGame_);
    }

    /// <summary>
    /// Begins the game - everyone becomes active
    /// </summary>
    void StartGame_()
    {
        _active = true;

        // start the coroutine that spawns batteries throughout the game
        StartCoroutine(SpawnBatteries());

        // start the timer
        _overallLimit.StartTimer();
    }

    /// <summary>
    /// Gets the distribution of the items to spawn - how likely each is to be spawned
    /// </summary>
    /// <returns>A list of enum values and their % likelihood to be spawned</returns>
    List<Tuple<int, int>> GetDistribution()
    {
        List<Tuple<int, int>> distribution = new List<Tuple<int, int>>();
        distribution.Add(new Tuple<int, int>(10, 9));
        distribution.Add(new Tuple<int, int>(20, 10));
        distribution.Add(new Tuple<int, int>(30, 12));
        distribution.Add(new Tuple<int, int>(40, 16));
        distribution.Add(new Tuple<int, int>(50, 19));
        distribution.Add(new Tuple<int, int>(60, 12));
        distribution.Add(new Tuple<int, int>(70, 9));
        distribution.Add(new Tuple<int, int>(80, 7));
        distribution.Add(new Tuple<int, int>(90, 4));
        distribution.Add(new Tuple<int, int>(100, 2));

        return distribution;
    }

    /// <summary>
    /// Function that runs while the game is active, and creates batteries in random locations
    /// </summary>
    private IEnumerator SpawnBatteries()
    {
        // only do it while the game is active
        while (_active)
        {
            // get the position at which to spawn a battery (random)
            var positionX = UnityEngine.Random.Range(TopLeft.x, BottomRight.x);
            var positionY = UnityEngine.Random.Range(TopLeft.y, BottomRight.y);

            // get a random value of the battery - how much charge is left
            var value = SpawnItemDistributionFetcher<int>.GetRandomEnumValue(GetDistribution());

            // spawn a battery, and assign its value
            var spawned = Instantiate(BatteryPrefab, new Vector3(positionX, positionY, -3), Quaternion.identity);
            spawned.GetComponent<BatteryScript>().Initialise(value);

            // wait a random amount of time before spawning another
            yield return new WaitForSeconds(UnityEngine.Random.Range(_minBatteryWait, _maxBatteryWait));
        }
    }

    /// <summary>
    /// Called every second
    /// </summary>
    /// <param name="seconds">How many seconds are left</param>
    private void OnTimeLimitTick(int seconds)
    {
        // display the countdown
        TxtCountdown.text = seconds.ToString();

        if (seconds == 25)
        {
            StartCoroutine(FlashWarning_());
        }
    }

    /// <summary>
    /// Flashes the warning light on and off
    /// </summary>
    private IEnumerator FlashWarning_()
    {
        AlarmOverlay.enabled = true;

        var a = 0f;
        var c = AlarmOverlay.color;
        AlarmOverlay.color = new Color(c.r, c.g, c.b, a);

        while (!_ended)
        {
            for (; a <= 0.35f; a += 0.05f)
            {
                AlarmOverlay.color = new Color(c.r, c.g, c.b, a);
                AlarmIndicator.color = new Color(1, 1, 1, a);
                yield return new WaitForSeconds(0.05f);
            }
            for (; a >= 0; a -= 0.05f)
            {
                AlarmOverlay.color = new Color(c.r, c.g, c.b, a);
                AlarmIndicator.color = new Color(1, 1, 1, a);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    /// <summary>
    /// Spawns a player object for each player
    /// </summary>
    private List<Transform> SpawnPlayers_()
    {
        var playerTransforms = new List<Transform>();

        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(XTinguishInputHandler));

            Rockets[index].gameObject.SetActive(true);

            // create the "visual" player at the start point
            var spawned = player.Spawn(PlayerPrefab, SpawnPositions[index]);
            playerTransforms.Add(spawned);
            _players.Add(player.GetComponentInChildren<XTinguishInputHandler>());
            index++;
        }

        // update spawn time config
        _minBatteryWait = 3f / _players.Count;
        _minBatteryWait = 8 - _players.Count;

        return playerTransforms;
    }

    /// <summary>
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        _active = false;

        // end all active players when time is up
        foreach (var p in _players)
        {
            p.Timeout();
        }

        // show results
        StartCoroutine(EndGame_());
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    private void Update()
    {
        // make the left and right edges wider
        foreach (var fire in FireCollidersX)
        {
            fire.transform.localScale += new Vector3(_fireMoveX, 0);
        }

        // make the top and bottom edges taller
        foreach (var fire in FireCollidersY)
        {
            fire.transform.localScale += new Vector3(0, _fireMoveY);
        }
    }

    /// <summary>
    /// Checks if all players are complete, and ends the game if so
    /// </summary>
    public void CheckForComplete()
    {
        var allComplete = _players.All(p => p.IsComplete());
        if (allComplete)
        {
            // all players are complete, so end the game
            StartCoroutine(EndGame_());
        }
    }

    /// <summary>
    /// Displays results, then returns to central screen
    /// </summary>
    IEnumerator EndGame_()
    {
        // fire closes in more quickly
        _fireMoveX *= 15f;
        _fireMoveY *= 15f;

        // stop the timer
        _overallLimit.Abort();

        _fastFireMove = true;

        StartCoroutine(RocketsFly_());

        yield return new WaitForSeconds(END_GAME_TIMEOUT);

        _fastFireMove = false;

        _ended = true;
        AlarmOverlay.enabled = false;

        if (_players.Any(p => !p.Died()))
        {
            foreach (var v in FireCollidersX)
                v.gameObject.SetActive(false);
            foreach (var v in FireCollidersY)
                v.gameObject.SetActive(false);

            SpawnEndRockets_();

            // sets the players
            CameraFollowScript.SetPlayers(_endRockets.Select(r => r.transform).ToList(), FollowDirection.Right);
            CameraZoomFollowScript.SetPlayers(_endRockets.Where(p => p.Rocket.gameObject.activeInHierarchy).Select(r => r.transform).ToList(), FollowDirection.Right);

            // move the rockets
            foreach (var rocket in _endRockets)
            {
                rocket.StartMove();
            }
        }
        else
        {
            StartCoroutine(Complete_());
        }
    }

    // makes the rockets fly upwards
    private IEnumerator RocketsFly_()
    {
        while (_fastFireMove)
        {
            _rocketSpeed *= 1.01f;
            foreach (var r in Rockets)
            {
                var script = r.GetComponentInChildren<BackgroundRocketScript>();
                if (script != null)
                    script.TakeOff();
            }

            for (int i = 0; i < _players.Count; i++)
            {
                if (!_players[i].Died())
                    Rockets[i].Translate(new Vector3(0, _rocketSpeed, 0));
            }

            yield return new WaitForSeconds(0.01f);
        }
    }

    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // sort the players by points scored
        var ordered = _players.OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 180, 90, 20 };

        // add winning score points 
        for (int i = 0; i < ordered.Count(); i++)
        {
            if (ordered[i].GetPoints() > 0)
            {
                ordered[i].AddPoints(winnerPoints[i]);
                ordered[i].SetBonusPoints(winnerPoints[i]);
            }
        }
    }

    /// <summary>
    /// Spawns the rockets to be used to show the result
    /// </summary>
    void SpawnEndRockets_()
    {
        foreach (var p in _players)
        {
            var rocket = Instantiate(RocketPrefab, ResultsSpawnPositionsTop - (new Vector3(0, 4, 0) * p.GetPlayerIndex()), Quaternion.identity);
            var rocketScript = rocket.gameObject.GetComponent<RocketResultScript>();
            rocketScript.Initialise(p.GetBatteryList(), p.GetPlayerName(), p.GetPlayerIndex(), p.Died(), p.GetCharacterIndex(), p.AddPoints);
            _endRockets.Add(rocketScript);
        }
    }

    /// <summary>
    /// Callback function for when a player completes - checks if all complete
    /// </summary>
    public void CheckResultsComplete()
    {
        if (_endRockets.All(r => r.IsComplete()))
            StartCoroutine(Complete_());
    }

    /// <summary>
    /// Completes the game
    /// </summary>
    private IEnumerator Complete_()
    {
        AssignBonusPoints_();

        yield return new WaitForSeconds(1);

        ResultsScreen.Setup();
        GenericInputHandler[] genericPlayers = _players.ToArray<GenericInputHandler>();
        ResultsScreen.SetPlayers(genericPlayers);
        yield return new WaitForSeconds(4 + genericPlayers.Length);

        // fade out
        Fader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        // when no more players, move to the central page
        PlayerManagerScript.Instance.NextScene(Scene.GameCentral);
    }
}