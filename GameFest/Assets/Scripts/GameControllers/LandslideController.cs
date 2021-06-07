using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LandslideController : MonoBehaviour
{
    // status variables
    bool _active = true;

    // unity configuration
    [SerializeField]
    Transform RockPrefab;
    [SerializeField]
    Vector2 SpawnPosition;
    public Text TxtCountdown;

    // constant config
    const int TIME_LIMIT = 120;
    const float MIN_PAUSE_TIME = 0.1f;
    const float MAX_PAUSE_TIME = 5f;

    // links to other scripts/components
    PlayerAnimation _animation;
    TimeLimit _playerLimit;

    /// <summary>
    /// Called once on creation
    /// </summary>
    private void Start()
    {
        // initialise the timeout
        _playerLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _playerLimit.Initialise(TIME_LIMIT, PlayerTickCallback, PlayerTimeoutCallback);

        // start spawning rocks
        StartCoroutine(SpawnRocks_());
    }

    /// <summary>
    /// Called once when the time limit has fully expired
    /// </summary>
    private void PlayerTimeoutCallback()
    {
        _active = false;
        // TODO: show results
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
    /// Spawns rocks while the game is ongoing
    /// </summary>
    private IEnumerator SpawnRocks_()
    {
        // continue while the game is going on
        while (_active)
        {
            // wait a random amount of time, then spawn a rock
            yield return new WaitForSeconds(Random.Range(MIN_PAUSE_TIME, MAX_PAUSE_TIME));
            SpawnRock_();
        }
    }

    /// <summary>
    /// Creates a rock
    /// </summary>
    private void SpawnRock_()
    {
        // spawn the rock
        var rock = Instantiate(RockPrefab, SpawnPosition, Quaternion.identity);
        // randomise the size and other attributes of the rock
        rock.GetComponent<RockScript>().Initialise();
    }
}
