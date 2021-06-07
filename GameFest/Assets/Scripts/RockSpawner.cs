using UnityEngine;
using System.Collections;

public class RockSpawner : MonoBehaviour
{
    // unity configuration
    [SerializeField]
    Transform RockPrefab;
    [SerializeField]
    Vector2 SpawnPosition;

    // const configuration
    const float MIN_PAUSE_TIME = 0.1f;
    const float MAX_PAUSE_TIME = 5f;

    // status
    bool _active;

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
    /// Start spawning
    /// </summary>
    public void Enable()
    {
        _active = true;

        // start spawning rocks
        StartCoroutine(SpawnRocks_());
    }

    /// <summary>
    /// Stop spawning
    /// </summary>
    public void Disable()
    {
        _active = false;
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
