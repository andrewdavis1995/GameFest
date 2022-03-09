using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DrinkSlideController : GenericController
{
    public static DrinkSlideController Instance;

    public const int WINNING_STONE_POINTS = 60;
    public const int IN_ZONE_POINTS = 5;
    const int GAME_TIMEOUT = 200;

    public Transform DrinkPrefab;
    public TargetScript[] TargetZones;
    public Vector3[] StartPositions;
    public Sprite[] GlassShardSprites;
    public CameraLerp LerpControl;
    public ResultsPageScreen ResultsScreen;
    public TransitionFader EndFader;

    TimeLimit _countdownTimer;

    public Text[] PlayerNames;
    public Text[] PlayerScores;
    public Text TxtCountdown;

    private int[] _targetScores = new int[] { 0, 0, 0, 0 };
    private int[] _currentScores = new int[] { 0, 0, 0, 0 };

    List<DrinkSlideInputHandler> _players;
    float _rightCurve = 0;

    LineRenderer _line;

    Vector3 _direction = Vector3.zero;
    float _angle = 0;

    bool _ended = false;
    bool _running = false;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        _line = GetComponent<LineRenderer>();

        // TEMP
        _players = FindObjectsOfType<DrinkSlideInputHandler>().ToList();

        SpawnPlayers_();

        SetupUI_();

        // initialise pause handler
        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        PauseGameHandler.Instance.Initialise(genericPlayers, QuitGame_);

        _countdownTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _countdownTimer.Initialise(GAME_TIMEOUT, CountdownTick_, CountdownDone_, 1f);

        // fade in
        EndFader.GetComponentInChildren<Image>().sprite = PlayerManagerScript.Instance.GetFaderImage();
        EndFader.StartFade(1, 0, () => { PauseGameHandler.Instance.Pause(true, StartGame_); });
    }

    /// <summary>
    /// Callback for each second of the countdown
    /// </summary>
    /// <param name="time">How much time is left</param>
    private void CountdownTick_(int time)
    {
        TxtCountdown.text = time <= 10 ? time.ToString() : "";
    }

    /// <summary>
    /// Callback for when countdown expires
    /// </summary>
    private void CountdownDone_()
    {
        EndGame_();
    }

    void EndGame_()
    {
        StartCoroutine(Complete_());
    }

    private void SetupUI_()
    {
        int index = 0;
        for (; index < _players.Count; index++)
        {
            PlayerNames[index].text = _players[index].GetPlayerName();
            PlayerScores[index].text = "0";
        }
        for (; index < PlayerNames.Length; index++)
        {
            PlayerNames[index].GetComponentInParent<Image>().gameObject.SetActive(false);
        }
    }

    void StartGame_()
    {
        _running = true;
        foreach (var p in _players)
            p.Enable();

        StartCoroutine(ControlTargets_());
        _countdownTimer.StartTimer();
    }

    internal void AddPointsWinningStone(int winner)
    {
        _players[winner].AddPoints(WINNING_STONE_POINTS);
    }

    internal void AddPointsInZone(int winner)
    {
        _players[winner].AddPoints(IN_ZONE_POINTS);
    }

    internal void RefreshScores()
    {
        for (int i = 0; i < _players.Count; i++)
        {
            _targetScores[i] = _players[i].GetPoints();
        }
    }

    IEnumerator ControlTargets_()
    {
        yield return new WaitForSeconds(2f);

        while (_running)
        {
            var available = TargetZones.Where(t => !t.gameObject.activeInHierarchy).ToList();
            if (available.Count > 0)
            {
                var ran = available[UnityEngine.Random.Range(0, available.Count)];
                ran.Activate();
            }
            yield return new WaitForSeconds(20);
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

    private void Update()
    {
        for (int i = 0; i < _players.Count; i++)
        {
            // increment score while not up to date
            if (_currentScores[i] < _targetScores[i])
            {
                _currentScores[i]++;
                PlayerScores[i].text = _currentScores[i].ToString();
            }
        }
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
