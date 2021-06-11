using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LandslideController : MonoBehaviour
{
    // status variables
    bool _active = true;

    // unity configuration
    public Text TxtCountdown;
    public RockSpawner[] RockSpawners;
    public Vector2[] PowerUpSpawnPositions;
    public Transform PowerUpPrefab;

    // constant config
    const int TIME_LIMIT = 120;

    // links to other scripts/components
    PlayerAnimation _animation;
    TimeLimit _playerLimit;

    List<Transform> _powerUpBoosts = new List<Transform>();

    // static instance
    public static LandslideController Instance;

    /// <summary>
    /// Called once on creation
    /// </summary>
    private void Start()
    {
        Instance = this;

        // initialise the timeout
        _playerLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _playerLimit.Initialise(TIME_LIMIT, PlayerTickCallback, PlayerTimeoutCallback);

        // start the timer
        _playerLimit.StartTimer();

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
            yield return new WaitForSeconds(Random.Range(5, 12));

            // spawn a boost at a random one of the positions
            SpawnPowerUp_(Random.Range(0, PowerUpSpawnPositions.Length));
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
            SpawnPowerUp_(Random.Range(0, PowerUpSpawnPositions.Length));
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
    /// Ends the game, shows results, returns to Central screen
    /// </summary>
    IEnumerator EndGame_()
    {
        // start spawning rocks
        foreach (var spawner in RockSpawners)
            spawner.Disable();

        yield return new WaitForSeconds(5);

        // get all checkpoints
        var checkpoints = GameObject.FindObjectsOfType<CheckpointScript>();
        foreach (var checkpoint in checkpoints)
        {
            // TODO: loop through all players
            int i = 0;
            var points = checkpoint.GetPlayerPoints(i);
            // TODO: Add points
        }

        // TODO: remove this
        SceneManager.LoadScene(1);
        //PlayerManagerScript.Instance.NextScene(Scene.GameCentral);
    }

    /// <summary>
    /// Checks if all players are complete
    /// </summary>
    internal void CheckForFinish()
    {
        bool finished = true;

        // TODO: add back in once proper controls are done
        //foreach(var player in _players)
        {
            // if (player.IsActive())
            //finished = false;
        }

        // if finished, end the game
        if (finished)
        {
            StartCoroutine(EndGame_());
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
}
