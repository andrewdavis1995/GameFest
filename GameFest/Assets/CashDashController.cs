using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CashDashController : GenericController
{
    // configuration
    private const int GAME_TIMEOUT = 180;
    private const int MAX_POINTS = 1250;
    public int[] POSITIONAL_POINTS = { 50, 20, 5, 0 };

    public RectTransform[] OffScreenDisplays;
    public UpperTransportController UpperTransport;
    public Transform PlayerPrefab;
    public Vector3[] StartPositions;
    public CameraFollow CameraFollowScript;
    public Collider2D[] BvColliders;
    public Sprite KeyIcon;
    public MediaJamWheel[] MediaJamWheels;
    public MediaJamWheel[] MediaJamWheelsUpper;
    public Sprite[] DisabledImages;
    public Sprite[] FlailImages;
    public GameObject[] BvKeysLeft;
    public GameObject[] BvKeysRight;
    public TransitionFader EndFader;
    public ResultsPageScreen ResultsScreen;

    public static CashDashController Instance;

    // time out
    TimeLimit _overallLimit;
    TimeLimit _pointCountdown;

    public Text TxtCountdown;
    public Text TxtPoints;

    int _remainingPoints;

    List<CashDashInputHandler> _players = new List<CashDashInputHandler>();
    int _completedPlayers = 0;

    /// <summary>
    /// Called once on startup
    /// </summary>
    private void Start()
    {
        Instance = this;

        var players = SpawnPlayers_();

        // assign players to the camera
        CameraFollowScript.SetPlayers(players, FollowDirection.Up);

        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _overallLimit.Initialise(GAME_TIMEOUT, OnTimeLimitTick, OnTimeUp);
        _pointCountdown = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _pointCountdown.Initialise(MAX_POINTS, OnPointsTick, null, 0.1f);

        HideUnusedItems_();

        EndFader.GetComponentInChildren<Image>().sprite = PlayerManagerScript.Instance.GetFaderImage();
        EndFader.StartFade(1, 0, FadeInComplete);

        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        PauseGameHandler.Instance.Initialise(genericPlayers);
    }

    /// <summary>
    /// Called once fully faded in
    /// </summary>
    private void FadeInComplete()
    {
        PauseGameHandler.Instance.Pause(true, StartGame_);
    }

    /// <summary>
    /// Get how many points are left
    /// </summary>
    /// <returns>The points that are left to win at this moment</returns>
    public int RemainingPoints()
    {
        return _remainingPoints;
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    private void StartGame_()
    {
        _overallLimit.StartTimer();
        _pointCountdown.StartTimer();

        UpperTransport.StartNoteMovement();
    }

    /// <summary>
    /// Called every 0.1 seconds
    /// </summary>
    /// <param name="seconds">How many points are left</param>
    private void OnPointsTick(int points)
    {
        _remainingPoints = points;
        TxtPoints.text = points.ToString();
    }

    /// <summary>
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        // show results
        StartCoroutine(EndGame_());
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    private IEnumerator EndGame_()
    {
        CameraFollowScript.enabled = false;

        // kill timers
        _overallLimit.Abort();
        _pointCountdown.Abort();

        yield return new WaitForSeconds(2f);

        StartCoroutine(Complete_());
    }

    /// <summary>
    /// Check if there are any players left
    /// </summary>
    internal void CheckForCompletion()
    {
        var complete = _players.All(p => p.Complete());

        if (complete)
            StartCoroutine(EndGame_());
    }

    /// <summary>
    /// Called each second
    /// </summary>
    /// <param name="seconds">How many seconds are left</param>
    void OnTimeLimitTick(int seconds)
    {
        // display countdown from 10 to 0
        TxtCountdown.text = seconds <= 10 ? seconds.ToString() : "";
    }

    /// <summary>
    /// Hides the unused keys (for players who are not taking part)
    /// </summary>
    private void HideUnusedItems_()
    {
        // for all indexes after the number of players, hide keys
        for (int i = PlayerManagerScript.Instance.GetPlayerCount(); i < BvKeysLeft.Length; i++)
        {
            BvKeysLeft[i].SetActive(false);
            BvKeysRight[i].SetActive(false);
            OffScreenDisplays[i].gameObject.SetActive(false);
            MediaJamWheels[i].gameObject.SetActive(false);
            MediaJamWheelsUpper[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Spawns the player movement objects to be used in this game
    /// </summary>
    /// <returns>List of spawned items</returns>
    private List<Transform> SpawnPlayers_()
    {
        var playerTransforms = new List<Transform>();

        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(CashDashInputHandler));
            var ih = player.GetComponent<CashDashInputHandler>();
            _players.Add(ih);

            // create the "visual" player at the start point
            var playerTransform = player.Spawn(PlayerPrefab, StartPositions[index]);

            ih.SetOffScreenDisplay(OffScreenDisplays[index]);

            var platforms = FindObjectsOfType<MediaJamWheel>();
            var matchingPlatforms = platforms.Where(t => LayerMask.LayerToName(t.gameObject.layer) == ("Player" + (index + 1) + "A"));
            var nonPlayerPlatforms = platforms.Where(t => LayerMask.LayerToName(t.gameObject.layer) != ("Player" + (index + 1) + "A"));

            ih.SetMediaJamPlatforms(platforms.ToList());
            playerTransforms.Add(playerTransform);

            foreach(var p in nonPlayerPlatforms)
            {
                Physics2D.IgnoreCollision(p.Platform.GetComponent<Collider2D>(), playerTransform.GetComponent<Collider2D>());
            }

            index++;
        }

        return playerTransforms;
    }

    /// <summary>
    /// Gets the points for finishing (based on position)
    /// </summary>
    /// <returns>Points to award</returns>
    internal int GetPositionalPoints()
    {
        var points = POSITIONAL_POINTS[_completedPlayers];
        _completedPlayers++;
        return points;
    }

    /// <summary>
    /// Show the results window, and then return to menu
    /// </summary>
    private IEnumerator Complete_()
    {
        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        ResultsScreen.Setup();
        ResultsScreen.SetPlayers(genericPlayers.ToArray());

        ScoreStoreHandler.StoreResults(Scene.CashDash, _players.ToArray());
        var ordered = _players.Where(p => p.GetPoints() > 0).OrderByDescending(p => p.GetPoints()).ToList();
        ordered.FirstOrDefault()?.Winner();

        yield return new WaitForSeconds(4 + _players.Count);

        // fade out
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        // when no more players, move to the central page
        PlayerManagerScript.Instance.CentralScene();
    }
}
