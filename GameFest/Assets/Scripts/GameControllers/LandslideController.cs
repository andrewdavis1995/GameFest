using System;
using System.Collections;
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

    // constant config
    const int TIME_LIMIT = 120;

    // links to other scripts/components
    PlayerAnimation _animation;
    TimeLimit _playerLimit;

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
        foreach(var spawner in RockSpawners)
            spawner.Enable();
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
}
