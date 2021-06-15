using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class XTinguishController : MonoBehaviour
{
    // configuration
    private const int GAME_TIMEOUT = 120;

    // unity configuration
    public TextMesh TxtCountdown;
    public Vector2 TopLeft;
    public Vector2 BottomRight;
    public Transform BatteryPrefab;
    public BoxCollider2D[] FireCollidersX;
    public BoxCollider2D[] FireCollidersY;
    public Vector2[] SpawerPositions;

    // static instance
    public static XTinguishController Instance;

    // status variables
    bool _active = false;
    List<ZeroGravityMovement> _players = new List<ZeroGravityMovement>();   // TODO: Convert to input handler

    // time out
    TimeLimit _overallLimit;

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

        // begin the game
        StartGame_();
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
        distribution.Add(new Tuple<int, int>(10, 19));
        distribution.Add(new Tuple<int, int>(20, 16));
        distribution.Add(new Tuple<int, int>(30, 15));
        distribution.Add(new Tuple<int, int>(40, 12));
        distribution.Add(new Tuple<int, int>(50, 11));
        distribution.Add(new Tuple<int, int>(60, 8));
        distribution.Add(new Tuple<int, int>(70, 7));
        distribution.Add(new Tuple<int, int>(80, 6));
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
            var spawned = Instantiate(BatteryPrefab, new Vector3(positionX, positionY, 0), Quaternion.identity);
            spawned.GetComponent<BatteryScript>().Initialise(value);

            // wait a random amount of time before spawning another
            yield return new WaitForSeconds(UnityEngine.Random.Range(1, 8));
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
    }

    /// <summary>
    /// Spawns a player object for each player
    /// </summary>
    void SpawnPlayers_()
    {
        // TODO: spawn a player for each player
    }

    /// <summary>
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        _active = false;

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
            fire.transform.localScale += new Vector3(.0015f, 0);
        }

        // make the top and bottom edges taller
        foreach (var fire in FireCollidersY)
        {
            fire.transform.localScale += new Vector3(0, .001f);
        }
    }

    /// <summary>
    /// Checks if all players are complete, and ends the game if so
    /// </summary>
    public void CheckForComplete()
    {
        var allComplete = _players.All(p => p.IsComplete());
        if(allComplete)
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
        // TODO: Show results
        yield return new WaitForSeconds(7);

        // TODO: Change to other scene management system
        SceneManager.LoadScene(1);
    }
}
