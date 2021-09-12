using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the flow of the "Shop Drop" mini game
/// </summary>
public class ShopDropController : GenericController
{
    // configuration
    public Vector2[] StartPositions;
    public float LeftBound;
    public float RightBound;
    public float BallDropHeight;
    private float _ballDelayUpper = 3f;
    private const int GAME_TIMEOUT = 90;

    // objects
    public Transform[] Trolleys;
    public TextMesh CountdownTimer;

    // links to other scripts
    public FoodFetcher Fetcher;
    public CameraMovement CameraScript;
    public ReceiptScript Receipt;

    // prefabs
    public Transform PlayerPrefab;
    public Transform BallPrefab;

    // time out
    TimeLimit _overallLimit;

    // state variables
    private bool _gameRunning = false;

    // easy access
    public static ShopDropController Instance;

    // all game controls - paddles, pegs, walls, zones etc.
    public GameObject GameArea;

    // results configuration
    int _playerResultIndex = 0;
    public Vector2[] CameraResultPositions;

    List<ShopDropInputHandler> _players = new List<ShopDropInputHandler>();

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
        _overallLimit.Initialise(GAME_TIMEOUT, OnTimeLimitTick, OnTimeUp);

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
        // disables all controls
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

        // hide game controls
        GameArea.SetActive(false);

        // show results
        ShowPlayerEndData();
    }

    /// <summary>
    /// Displays the receipt and all points information
    /// </summary>
    /// <returns></returns>
    IEnumerator DisplayPlayerResult_()
    {
        // show the receipt
        Receipt.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);

        // get the food that the player has collected
        var data = PlayerManagerScript.Instance.GetPlayers()[_playerResultIndex].GetComponent<ShopDropInputHandler>().GetFood();
        // group it by food
        var grouped = data.GroupBy(g => g.Food).OrderBy(g => g.Sum(i => i.Points)).Reverse();

        var index = 0;
        // loop through the grouped data
        foreach(var value in grouped)
        {
            // display the points for this item
            var points = value.Sum(v => v.Points);
            Receipt.SetText(index++, value.Key, points, value.Count());
            if(index >= Receipt.ReceiptTexts.Length)
            {
                Receipt.ResetTexts();
                index = 0;
            }
            yield return new WaitForSeconds(1);
        }

        // clear the rest
        ClearRemainingReceiptLines_(index);

        // display the total points for this player
        DisplayTotal_(data);
        yield return new WaitForSeconds(2);

        // carry on
        MoveToNextPlayer_();
    }

    /// <summary>
    /// Displays the total points from a user
    /// </summary>
    /// <param name="data">The data for this player</param>
    private void DisplayTotal_(List<ShopDropBallScript> data)
    {
        // get the total number of points
        var totalPoints = data.Sum(d => d.Points);

        // show total
        Receipt.TotalText.text = totalPoints.ToString();

        // display total title
        Receipt.TotalHeader.text = "Total: ";
    }

    /// <summary>
    /// Moves to the next players result
    /// </summary>
    private void MoveToNextPlayer_()
    {
        // move to next player
        _playerResultIndex++;

        // if there are players left, show their data
        if (_playerResultIndex < PlayerManagerScript.Instance.GetPlayers().Count)
        {
            Receipt.ResetTexts();
            Receipt.gameObject.SetActive(false);
            ShowPlayerEndData();
        }
        else
        {
            StartCoroutine(Complete_());
        }
    }

    /// <summary>
    /// Adds bonus points and out of the game
    /// </summary>
    /// <returns></returns>
    IEnumerator Complete_()
    {
        AssignBonusPoints_();

        yield return new WaitForSeconds(2);

        // when no more players, move to the central page
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // sort the players by points scored
        var ordered = _players.OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 150, 60, 10 };

        // add winning score points 
        for (int i = 0; i < ordered.Count(); i++)
        {
            ordered[i].AddPoints(winnerPoints[i]);
            ordered[i].SetBonusPoints(winnerPoints[i]);
        }
    }

    /// <summary>
    /// Clears the unused values - sets the value to ""
    /// </summary>
    /// <param name="index">The index of the textmesh to clear</param>
    private void ClearRemainingReceiptLines_(int index)
    {
        // hide the remaining texts
        for (; index < Receipt.ReceiptTexts.Length; index++)
        {
            Receipt.SetText(index, "", 0, 0);
        }
    }

    /// <summary>
    /// Called when the camera reaches the correct location
    /// </summary>
    void ResultCallback()
    {
        StartCoroutine(DisplayPlayerResult_());
    }

    /// <summary>
    /// Moves to the next player
    /// </summary>
    void ShowPlayerEndData()
    {
        CameraScript.StartMovement(CameraResultPositions[_playerResultIndex], 1.6f);
        CameraScript.SetCallback(ResultCallback);
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
    /// <param name="seconds">How many seconds are left</param>
    void OnTimeLimitTick(int seconds)
    {
        // display countdown from 10 to 0
        CountdownTimer.text = seconds <= 10 ? seconds.ToString() : "";
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
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, _ballDelayUpper));
            _ballDelayUpper -= 0.0115f;
        }
    }

    /// <summary>
    /// Creates a ball to drop from the top
    /// </summary>
    void CreateBall_()
    {
        // x position
        var left = UnityEngine.Random.Range(LeftBound, RightBound);

        // create a ball
        var ball = Instantiate(BallPrefab, new Vector2(left, BallDropHeight), Quaternion.identity);

        // assign a food to the ball
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

            // create the "visual" player at the start point - only used for turning
            player.Spawn(PlayerPrefab, StartPositions[index]);
            var inputHandler = player.GetComponent<ShopDropInputHandler>();
            inputHandler.AssignPaddles(index);
            _players.Add(inputHandler);
            index++;
        }
    }
}
