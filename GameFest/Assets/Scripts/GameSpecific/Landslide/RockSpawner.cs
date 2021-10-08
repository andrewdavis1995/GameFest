using UnityEngine;
using System.Collections;

public class RockSpawner : MonoBehaviour
{
    // unity configuration
    [SerializeField]
    Transform RockPrefab;
    [SerializeField]
    Vector2 SpawnPosition;

    const int ROCK_BARAGE_QUANTITY = 10;

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
            //yield return new WaitForSeconds(Random.Range(MIN_PAUSE_TIME, MAX_PAUSE_TIME));
            yield return new WaitForSeconds(10f);
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
    void SpawnRock_()
    {
        // spawn the rock
        var rock = Instantiate(RockPrefab, SpawnPosition, Quaternion.identity);
        // randomise the size and other attributes of the rock
        rock.GetComponent<RockScript>().Initialise();
    }

    /// <summary>
    /// Spawn a lot of rocks
    /// </summary>
    public void RockBarage(int playerIndex)
    {
        // create 10 rocks
        for (int i = 0; i < ROCK_BARAGE_QUANTITY; i++)
        {
            // spawn the rock
            var rock = Instantiate(RockPrefab, SpawnPosition, Quaternion.identity);
            // make a big rock
            var rockScript = rock.GetComponent<RockScript>();
            rockScript.Initialise(RockScript.MIN_ROCK_SIZE / 1.1f, RockScript.MAX_ROCK_SIZE / 1.25f);

            // sets the player index of the rock
            rockScript.SetPlayerIndex(playerIndex);
        }
    }

    /// <summary>
    /// Spawn a lot of rocks (little ones)
    /// </summary>
    public void RockBarageSmall(int playerIndex)
    {
        // create 10 rocks
        for (int i = 0; i < ROCK_BARAGE_QUANTITY; i++)
        {
            // spawn the rock
            var rock = Instantiate(RockPrefab, SpawnPosition, Quaternion.identity);
            var rockScript = rock.GetComponent<RockScript>();
            rockScript.Initialise(RockScript.MIN_ROCK_SIZE / 1.4f, RockScript.MAX_ROCK_SIZE / 1.8f);

            // sets the player index of the rock
            rockScript.SetPlayerIndex(playerIndex);
        }
    }

    /// <summary>
    /// Creates a rock
    /// </summary>
    public void SpawnGiantRock(int playerIndex)
    {
        // spawn the rock
        var rock = Instantiate(RockPrefab, SpawnPosition, Quaternion.identity);
        var rockScript = rock.GetComponent<RockScript>();
        // make a big rock
        rockScript.Initialise(RockScript.GIANT_ROCK_SIZE);

        // sets the player index of the rock
        rockScript.SetPlayerIndex(playerIndex);
        rock.gameObject.name = "GiantRock" + playerIndex;
    }
}
