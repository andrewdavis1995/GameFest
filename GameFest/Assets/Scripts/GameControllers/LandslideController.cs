using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LandslideController : GenericController
{

    // status variables
    bool _active = true;
    List<Transform> _powerUpBoosts = new List<Transform>();
    int _resultsPlayerIndex = 0;

    // unity configuration
    public Text TxtCountdown;
    public RockSpawner[] RockSpawners;
    public Vector2[] PowerUpSpawnPositions;
    public Transform PowerUpPrefab;
    public Transform PlayerPrefab;
    public Vector2 StartPosition;
    public CameraFollow FollowScript;
    public CameraZoomFollow FollowZoomScript;
    public CameraMovement CameraMovement;
    public Transform Sign;
    public TextMesh SignText;

    // constant config
    const int TIME_LIMIT = 300;
    const int ROCK_HIT_POINTS = 20;

    // links to other scripts/components
    PlayerAnimation _animation;
    TimeLimit _overallLimit;
    TimeLimit _endingTimer;
    List<LandslideInputHandler> _players = new List<LandslideInputHandler>();
    List<LandslideInputHandler> _resultPositions = new List<LandslideInputHandler>();

    // static instance
    public static LandslideController Instance;

    /// <summary>
    /// Called once on creation
    /// </summary>
    private void Start()
    {
        Instance = this;

        var players = SpawnPlayers_();

        FollowScript.SetPlayers(players, FollowDirection.Right);
        FollowZoomScript.SetPlayers(players, FollowDirection.Right);

        // initialise the timeout
        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _overallLimit.Initialise(TIME_LIMIT, PlayerTickCallback, PlayerTimeoutCallback);

        // initialise the timeout after a player completes
        _endingTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _endingTimer.Initialise(10, EndGameTimeoutTick, EndGameTimeout);

        // start the timer
        _overallLimit.StartTimer();

        // start spawning rocks
        foreach (var spawner in RockSpawners)
            spawner.Enable();

        // spawn initial points
        SpawnPowerUpBoostsInitial_();

        // start randomly spawning points
        StartCoroutine(SpawnPowerUpBoosts_());
    }

    // create a power up boost from time to time
    private IEnumerator SpawnPowerUpBoosts_()
    {
        while (_active)
        {
            // wait some time
            yield return new WaitForSeconds(UnityEngine.Random.Range(5, 12));

            // spawn a boost at a random one of the positions
            SpawnPowerUp_(UnityEngine.Random.Range(0, PowerUpSpawnPositions.Length));
        }
    }

    /// <summary>
    /// Removes a boost from the list of active boosts
    /// </summary>
    /// <param name="transform">The boost to remove</param>
    public void RemoveBoost(Transform transform)
    {
        _powerUpBoosts.Remove(transform);
    }

    // create a power up boost at each spawn points
    private void SpawnPowerUpBoostsInitial_()
    {
        // spawn a boost at each position
        for (int i = 0; i < PowerUpSpawnPositions.Length; i++)
            // spawn a boost at a random one of the positions
            SpawnPowerUp_(UnityEngine.Random.Range(0, PowerUpSpawnPositions.Length));
    }

    /// <summary>
    /// Called once when the time limit has fully expired
    /// </summary>
    private void PlayerTimeoutCallback()
    {
        _active = false;
        StartCoroutine(EndGame_());
    }

    /// <summary>
    /// Called each time the time limit ticks - each second in this case
    /// </summary>
    /// <param name="seconds">How many seconds are remaining</param>
    private void PlayerTickCallback(int seconds)
    {
        // display a countdown for the last 10 seconds
        TxtCountdown.text = seconds <= 10 ? seconds.ToString() : "";
    }

    /// <summary>
    /// Called once when the time limit has fully expired
    /// </summary>
    private void EndGameTimeout()
    {
        Complete();
    }

    /// <summary>
    /// Disables all players and starts showing results
    /// </summary>
    void Complete()
    {
        // hide coutdown
        TxtCountdown.text = "";

        // check all players to see if they are complete
        foreach (var player in _players)
        {
            // only the complete players should celebrate
            string animation = "Idle";
            if (player.IsComplete())
                animation = "Celebrate";

            // complete the player
            player.Finish();
            player.SetAnimationTrigger(animation);
        }

        // end the game
        StartCoroutine(EndGame_());
    }

    /// <summary>
    /// Called each time the time limit ticks - each second in this case
    /// </summary>
    /// <param name="seconds">How many seconds are remaining</param>
    private void EndGameTimeoutTick(int seconds)
    {
        // display a countdown for the last 10 seconds
        TxtCountdown.text =seconds.ToString();
    }

    /// <summary>
    /// Ends the game, shows results, returns to Central screen
    /// </summary>
    IEnumerator EndGame_()
    {
        // stop spawning rocks
        foreach (var spawner in RockSpawners)
            spawner.Disable();

        // brief pause
        yield return new WaitForSeconds(2);

        // get all checkpoints
        var checkpoints = FindObjectsOfType<CheckpointScript>();
        foreach (var checkpoint in checkpoints)
        {
            // loop through players
            foreach (var player in _players)
            {
                // get the points for each player for this checkpoint
                var playerIndex = player.GetPlayerIndex();
                var points = checkpoint.GetPlayerPoints(playerIndex);
                player.AddPoints(points);
            }
        }

        // disable the script which follows the leader
        FollowScript.Disable();

        // re-order the player
        _resultPositions = _players.OrderByDescending(p => p.GetEndPosition().x).ToList();

        // show results
        ShowPlayerResult_();
    }

    /// <summary>
    /// Shows the results of the current player
    /// </summary>
    void ShowPlayerResult_()
    {
        CameraMovement.SpeedAdjustment = 35;
        CameraMovement.SetCallback(ResultCallback_);
        CameraMovement.StartMovement(_resultPositions[_resultsPlayerIndex].GetEndPosition(), 5f);
    }

    /// <summary>
    /// When showing the results has occurred
    /// </summary>
    void ResultCallback_()
    {
        StartCoroutine(ShowResult_());
    }

    /// <summary>
    /// Shows the player results, then moves to the next player
    /// </summary>
    private IEnumerator ShowResult_()
    {
        yield return new WaitForSeconds(1);

        // set sign position and text
        Sign.gameObject.SetActive(true);

        Sign.transform.position = _resultPositions[_resultsPlayerIndex].GetEndPosition() + new Vector2(-2, 5);
        SignText.text = _resultPositions[_resultsPlayerIndex].GetPoints().ToString();
        
        // display briefly
        yield return new WaitForSeconds(5);

        // hide the sign and go to the next player
        Sign.gameObject.SetActive(false);
        NextPlayerResults_();
    }

    /// <summary>
    /// Moves to the next player results
    /// </summary>
    private void NextPlayerResults_()
    {
        _resultsPlayerIndex++;

        // checks if all players are done
        if (_resultsPlayerIndex >= _players.Count)
        {
            // assign bonuses
            AssignBonusPoints_();

            // done
            PlayerManagerScript.Instance.CentralScene();
        }
        else
        {
            // next player
            ShowPlayerResult_();
        }
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private List<Transform> SpawnPlayers_()
    {
        var playerTransforms = new List<Transform>();

        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(LandslideInputHandler));

            // create the "visual" player at the start point
            var spawned = player.Spawn(PlayerPrefab, StartPosition + new Vector2(-0.15f * index++, 0));
            playerTransforms.Add(spawned);
            _players.Add(player.GetComponentInChildren<LandslideInputHandler>());
        }

        return playerTransforms;
    }

    /// <summary>
    /// Assign points to a player after one of their rocks hit a player
    /// </summary>
    /// <param name="playerIndex">The index of the player that the rock belonged to</param>
    internal void RockHitPoints_(int playerIndex)
    {
        _players.Where(p => p.GetPlayerIndex() == playerIndex).First().AddPoints(ROCK_HIT_POINTS);
    }

    /// <summary>
    /// Checks if all players are complete
    /// </summary>
    internal void CompleteGame()
    {
        // stop the overall time
        _overallLimit.Abort();

        // if there is still a player playing, give them 10 seconds to complete
        if(_players.Any(p => !p.IsComplete()))
        {
            _endingTimer.StartTimer();
        }
        else
        {
            // otherwise, just end the game
            Complete();
            _endingTimer.Abort();
        }
    }

    /// <summary>
    /// Spawns a giant rock
    /// </summary>
    public void SpawnGiantRock(int playerIndex)
    {
        // spawn a giant rock
        foreach (var spawner in RockSpawners)
            spawner.SpawnGiantRock(playerIndex);
    }

    /// <summary>
    /// Spawns a lot of rocks
    /// </summary>
    /// <param name="playerIndex">The player that triggered it</param>
    public void RockBarage(int playerIndex)
    {
        // spawn rocks at each spawner
        foreach (var spawner in RockSpawners)
            spawner.RockBarage(playerIndex);
    }

    /// <summary>
    /// Spawns a lot of rocks (little ones)
    /// </summary>
    /// <param name="playerIndex">The player that triggered it</param>
    public void RockBarageSmall(int playerIndex)
    {
        // spawn rocks at each spawner
        foreach (var spawner in RockSpawners)
            spawner.RockBarageSmall(playerIndex);
    }

    /// <summary>
    /// Spawn a power up at the specified position
    /// </summary>
    /// <param name="spawnPointIndex">The index of the spawn point at which to spawn the object</param>
    void SpawnPowerUp_(int spawnPointIndex)
    {
        // create the object at the specified point
        var position = PowerUpSpawnPositions[spawnPointIndex];

        // only spawn it if there is not currently one in that space
        if (!(_powerUpBoosts.Any(p => p.transform.position.x == position.x)))
        {
            // add to the list of boosts
            var boosted = Instantiate(PowerUpPrefab, position, Quaternion.identity);
            _powerUpBoosts.Add(boosted);
        }
    }

    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // sort the players by points scored
        var ordered = _players.OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 170, 75, 15 };

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
}
