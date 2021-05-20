using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the flow of the "Shop Drop" mini game
/// </summary>
public class ShopDropController : MonoBehaviour
{
    // configuration
    public Vector2[] StartPositions;
    public float LeftBound;
    public float RightBound;
    public float BallDropHeight;

    // objects
    public Transform[] Trolleys;

    // links to other scripts
    public FoodFetcher Fetcher;

    // prefabs
    public Transform PlayerPrefab;
    public Transform BallPrefab;

    // time out
    TimeLimit _overallLimit;

    // state variables
    private bool _gameRunning = false;

    // easy access
    public static ShopDropController Instance;

    /// <summary>
    /// Runs at start
    /// </summary>
    void Start()
    {
        // store a static instance of this script
        Instance = this;

        // assign paddles to players
        SetPaddles_();

        // assign zones to players
        SetZones_();

        // create players
        SpawnPlayers_();

        // setup the timeout
        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _overallLimit.Initialise(90, OnTimeLimitTick, OnTimeUp);

        // start the game
        StartGame_();
    }

    /// <summary>
    /// Assigns players to each paddle
    /// </summary>
    void SetPaddles_()
    {
        int playerIndex = 0;

        // find all paddles
        PaddleScript[] paddles = GameObject.FindObjectsOfType<PaddleScript>();
        Debug.Log(paddles.Length);

        // loop through each paddle
        for (int i = 0; i < paddles.Length; i++)
        {
            // assign to current player, and set colour accordingly
            paddles[i].SetColour(playerIndex);

            // move to the next player, and loop around if at end
            playerIndex++;
            if (playerIndex >= PlayerManagerScript.Instance.GetPlayers().Count)
            {
                playerIndex = 0;
            }
        }
    }

    /// <summary>
    /// Assigns players to each paddle
    /// </summary>
    void SetZones_()
    {
        int playerIndex = 0;

        // find all paddles
        GameObject[] zones = GameObject.FindGameObjectsWithTag("AreaTrigger");
        zones = zones.OrderBy(p => UnityEngine.Random.Range(0, 10)).ToArray();

        // loop through each paddle
        for (int i = 0; i < zones.Length; i++)
        {
            // assign to current player, and set colour accordingly
            zones[i].name = "AREA_" + playerIndex;
            zones[i].GetComponentInChildren<SpriteRenderer>().color = ColourFetcher.GetColour(playerIndex);

            // move to the next player, and loop around if at end
            playerIndex++;
            if (playerIndex >= PlayerManagerScript.Instance.GetPlayers().Count)
            {
                playerIndex = 0;
            }
        }
    }

    /// <summary>
    /// Is the game active (can players move)?
    /// </summary>
    /// <returns>Whether the game is active</returns>
    public bool GameRunning()
    {
        return _gameRunning;
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    void StartGame_()
    {
        _gameRunning = true;

        // start creating balls
        StartCoroutine(GenerateBalls_());

        // start the timer
        _overallLimit.StartTimer();
    }

    /// <summary>
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        _gameRunning = false;

        // show results
        StartCoroutine(ShowResults());
    }

    /// <summary>
    /// Shows the results, one player at a time
    /// </summary>
    private IEnumerator ShowResults()
    {
        yield return new WaitForSeconds(2);

        // remove stray balls
        TidyUpBalls_();

        // when no more players, move to the central page
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Removes any balls which did not make it to a trolley
    /// </summary>
    private void TidyUpBalls_()
    {
        // find all balls
        var balls = FindObjectsOfType<ShopDropBallScript>();

        // if the parent of the ball is not set, destroy the ball
        foreach (var ball in balls)
            if (ball.transform.parent == null)
                Destroy(ball.gameObject);
    }

    /// <summary>
    /// Called each second
    /// </summary>
    /// <param name="seconds"></param>
    void OnTimeLimitTick(int seconds)
    {
        Debug.Log(seconds);
    }

    /// <summary>
    /// Runs throughout the game, controls the dropping of the balls
    /// </summary>
    private IEnumerator GenerateBalls_()
    {
        // create balls until time up
        while (_gameRunning)
        {
            CreateBall_();
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 3f));
        }
    }

    /// <summary>
    /// Creates a ball to drop from the top
    /// </summary>
    void CreateBall_()
    {
        var left = UnityEngine.Random.Range(LeftBound, RightBound);
        var ball = Instantiate(BallPrefab, new Vector2(left, BallDropHeight), Quaternion.identity);
        Fetcher.GetFood(ball.GetComponent<ShopDropBallScript>());
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // add the input handle, and assign all paddles
            player.SetActiveScript(typeof(ShopDropInputHandler));
            player.GetComponent<ShopDropInputHandler>().AssignPaddles(index);

            // create the "visual" player at the start point - only used for turning
            player.Spawn(PlayerPrefab, StartPositions[index]);
            index++;
        }
    }
}
