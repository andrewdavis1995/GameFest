using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls the flow of "Cart Attack"
/// </summary>
public class CartAttackController : MonoBehaviour
{
    public Collider2D[] Checkpoints;
    public CarControllerScript[] Cars;
    
    List<CartAttackInputHandler> _players = new List<CartAttackInputHandler>();

    public static CartAttackController Instance;

    TimeLimit _raceTimer;

    // Called once on startup
    private void Start()
    {
        _raceTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _raceTimer.Initialise(90, raceTimerTick_, raceTimerComplete_, 1f);

        Instance = this;

        _players = SpawnPlayers_();

        // TODO: Move to after SpawnPlayers
        HideUnusedElements_(_players.Count, Cars.Length);

        // TODO: this moves to after the countdown lights
        StartRace_();
    }

    /// <summary>
    /// Enables racers, starts timer and begins the race
    /// </summary>
    private void StartRace_()
    {
        // enable all players
        foreach (var player in _players)
        {
            player.StartRace();
        }

        // start timer
        _raceTimer.StartTimer();
    }

    /// <summary>
    /// Callback for when the timer runs out
    /// </summary>
    private void raceTimerComplete_()
    {
        // disable all players
        foreach (var player in _players)
        {
            player.SetActiveState(false);
        }

        // TODO: end the game
    }

    /// <summary>
    /// Callback for when the timer ticks
    /// </summary>
    private void raceTimerTick_(int time)
    {
        // TODO: show in UI
        Debug.Log(time);
    }

    /// <summary>
    /// Creates the necessary controls for players
    /// </summary>
    /// <returns>List of created players</returns>
    List<CartAttackInputHandler> SpawnPlayers_()
    {
        var list = new List<CartAttackInputHandler>();

        int index = 0;

        // TODO: replace with calls to create input handler
        list = FindObjectsOfType<CartAttackInputHandler>().ToList();
        list[index].SetCarController(Cars[index]);

        return list;
    }

    /// <summary>
    /// Hides cars and UI elements that are not needed (due to there not being the full 4 players playing)
    /// </summary>
    /// <param id="index">The index to start at</param>
    /// <param id="index">The maximum number of items to go up to</param>
    void HideUnusedElements_(int index, int maximum)
    {
        // hide unused cars
        for(; index < maximum; index++)
        {
            // hide car
            Cars[index].gameObject.SetActive(false);
        }
    }
}
