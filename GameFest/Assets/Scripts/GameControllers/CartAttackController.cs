using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the flow of "Cart Attack"
/// </summary>
public class CartAttackController : MonoBehaviour
{
    const int FASTEST_LAP_BONUS = 30;

    public Collider2D[] Checkpoints;
    public CarControllerScript[] Cars;
    public SpriteRenderer StarterLights;
    public Sprite[] StarterLightSprites;
    public Text TxtRemainingTime;
    // public CarStatusDisplay[] CarStatuses;   // TODO: implement this UI first
    
    List<CartAttackInputHandler> _players = new List<CartAttackInputHandler>();

    public static CartAttackController Instance;

    TimeLimit _raceTimer;
    
    int _currentBestLap = Int32.MaxValue;
    int _currentBestLapPlayer = -1;

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
        StartCoroutine(StartRace_());
    }

    /// <summary>
    /// Enables racers, starts timer and begins the race
    /// </summary>
    private IEnumerator StartRace_()
    {
        // show countdown
        for(int i = 0; i < StarterLightSprites.Length; i++)
        {
            // wait, then update image
            yield return new WaitForSeconds(1);
            StarterLights.sprite = StarterLightSprites[i];
        }
    
        // enable all players
        foreach (var player in _players)
        {
            player.StartRace();
        }

        // start timer
        _raceTimer.StartTimer();

        yield return new WaitForSeconds(3);
        StarterLights.sprite = StarterLightSprites[0];
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

        StartCoroutine(ShowResults_());
    }
    
    /// <summary>
    /// Show the canvases and scores for each players
    /// </summary>
    IEnumerator ShowResults_()
    {
        // TODO: show canvases for eacy player
        yield return new WaitForSeconds(1);
        
        StartCoroutine(Complete_());
    }

    /// <summary>
    /// Callback for when the timer ticks
    /// </summary>
    private void raceTimerTick_(int time)
    {
        // show remaining time
        TxtRemainingTime.text = time.ToString();
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
    
    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // add bonus points for quickest lap
        if(_currentBestLapPlayer > -1)
        {
            _players[_currentBestLapPlayer].AddPoints(FASTEST_LAP_BONUS);
        }
    
        // sort the players by points scored
        var ordered = _players.Where(p => p.GetPoints() > 0).OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 150, 50, 20 };

        // add winning score points 
        for (int i = 0; i < ordered.Count(); i++)
        {
            if (ordered[i].GetPoints() > 0)
            {
                ordered[i].AddPoints(winnerPoints[i]);
                ordered[i].SetBonusPoints(winnerPoints[i]);
            }
        }

        // set the winner
        ordered.FirstOrDefault()?.Winner();
    }
    
    /// <summary>
    /// Checks if the recently completed lap is bigger than the current best time
    /// </summary>
    /// <param id="playerIndex">The index of the player who completed the lap</param>
    /// <param id="lapTime">The time taken to complete the lap</param>
    public void CheckFastestLap(int playerIndex, int lapTime)
    {
        // if faster than the current record, store this lap as the new record
        if(lapTime < _currentBestLap)
        {
            _currentBestLap = lapTime;
            _currentBestLapPlayer = playerIndex;
        }
    }
    
    /// <summary>
    /// Completes the game and return to object
    /// </summary>
    IEnumerator Complete_()
    {
        AssignBonusPoints_();

        yield return new WaitForSeconds(3f);

        // TODO: add this in
        //ResultsScreen.Setup();

        // TODO: add this in
        GenericInputHandler[] genericPlayers = _players.ToArray<GenericInputHandler>();
        //ResultsScreen.SetPlayers(genericPlayers);

        // store scores
        ScoreStoreHandler.StoreResults(Scene.CartAttack, genericPlayers);

        yield return new WaitForSeconds(4 + genericPlayers.Length);

        // fade out
        //EndFader.StartFade(0, 1, ReturnToCentral_);
        // TODO: replace this with the above
        ReturnToCentral_();
    }
    
    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        // TODO: Add back once full menu system done
        // PlayerManagerScript.Instance.CentralScene();
    }
}
