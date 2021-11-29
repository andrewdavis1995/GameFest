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
    const int FASTEST_LAP_BONUS = 50;

    public Collider2D[] Checkpoints;
    public CarControllerScript[] Cars;
    public SpriteRenderer StarterLights;
    public Sprite[] StarterLightSprites;
    public Sprite[] DriverSprites;
    public Text TxtRemainingTime;
    public Text TxtTotalPoints;
    public CartAttackPlayerUiScript[] CarStatuses;
    public GameObject[] PowerUps;
    public VehicleSelectionController VehicleSelection;
    public GameObject Leaderboard;
    public GameObject Gallery;
    public CameraLerp CameraLerpController;

    public DrawingDisplayScript[] GalleryFrames;
    public TrailRenderer[] TrailRenderers;

    List<CartAttackInputHandler> _players = new List<CartAttackInputHandler>();

    public static CartAttackController Instance;
    
    public ResultsPageScreen ResultsScreen;
    public TransitionFader Fader;

    TimeLimit _raceTimer;

    double _currentBestLap = Int32.MaxValue;
    int _currentBestLapPlayer = -1;
    bool _running = false;

    // Called once on startup
    private void Start()
    {
        // initialise race timer (countdown)
        _raceTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _raceTimer.Initialise(90, raceTimerTick_, raceTimerComplete_, 1f);

        Instance = this;

        // show vehicle selection UI
        VehicleSelection.SetActiveState(true);

        // spawn player objects
        _players = SpawnPlayers_();
        
        // setup pause functionality and pause
        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        PauseGameHandler.Instance.Initialise(genericPlayers);        
        PauseGameHandler.Instance.Pause(true, null);

        // hide unused items (not enough players to fill slots)
        HideUnusedElements_(_players.Count, Cars.Length);
        
        // fade in
        Fader.GetComponentInChildren<Image>().sprite = PlayerManagerScript.Instance.GetFaderImage();
        Fader.StartFade(1, 0, null);
    }

    /// <summary>
    /// Enables racers, starts timer and begins the race
    /// </summary>
    private IEnumerator StartRace_()
    {
        CameraLerpController.enabled = true;
        Leaderboard.SetActive(true);

        // show countdown
        for (int i = 0; i < StarterLightSprites.Length; i++)
        {
            // wait, then update image
            yield return new WaitForSeconds(1);
            StarterLights.sprite = StarterLightSprites[i];
        }

        _running = true;

        // enable all players
        foreach (var player in _players)
        {
            player.StartRace();
        }

        // start timer
        _raceTimer.StartTimer();

        yield return new WaitForSeconds(3);
        StarterLights.sprite = StarterLightSprites[0];

        // start process of spawning power ups
        StartCoroutine(SpawnPowerups());
    }
    
    /// <summary>
    /// Runs throughout the game, spawning power up bubbles every 7 seconds
    /// </summary>
    private IEnumerator SpawnPowerups()
    {
        while (_running)
        {
            // wait 7 seconds
            yield return new WaitForSeconds(7f);

            // spawn a random power up
            var r = UnityEngine.Random.Range(0, PowerUps.Length - 1);
            PowerUps[r].SetActive(true);
        }
    }

    /// <summary>
    /// Checks if all vehicle selection is complete
    /// </summary>
    internal void CheckVehicleSelectionComplete()
    {
        var complete = _players.All(p => p.VehicleSelected());
        var complete2 = _players.Any(p => !p.VehicleSelected());

        if (complete)
        {
            VehicleSelection.SetActiveState(false);
            VehicleSelection.VehicleSelectionUI.gameObject.SetActive(false);
            StartCoroutine(StartRace_());
        }
    }

    /// <summary>
    /// Callback for when the timer runs out
    /// </summary>
    private void raceTimerComplete_()
    {
        _running = false;

        // disable all players
        foreach (var player in _players)
        {
            player.SetActiveState(false);
        }

        StartCoroutine(ShowResults_());
    }
    
    /// <summary>
    /// Sets up gallery frames at start
    /// </summary>
    void InitialiseFrames_()
    {
        // hide frames
        for (int i = 0; i < GalleryFrames.Length; i++)
        {
            GalleryFrames[i].gameObject.SetActive(false);
            GalleryFrames[i].PictureSign.gameObject.SetActive(false);
            GalleryFrames[i].AccuracyBonusRosette.gameObject.SetActive(false);
            GalleryFrames[i].Rosette.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Show the canvases and scores for each players
    /// </summary>
    IEnumerator ShowResults_()
    {
        yield return new WaitForSeconds(2f);

        Leaderboard.SetActive(false);
        Gallery.SetActive(true);
        
        // hide all frames at start
        InitialiseFrames_();

        foreach (var player in _players)
        {
            // get data
            var laps = player.GetLaps();
            var scores = player.GetLapScores();
            var times = player.GetLapTimes();
            var accuracies = player.GetLapAccuracies();

            for (int i = 0; i < laps.Count; i++)
            {
                // initialise frames
                GalleryFrames[i].Rosette.color = ColourFetcher.GetColour(player.GetPlayerIndex());
                GalleryFrames[i].gameObject.SetActive(true);

                // get trail positions for this lap
                var x = 100 * (i + 1) + 3;
                var tuples = laps[i].Select(t => t.Item1 + new Vector3(-x, 0, 0)).ToList();
                tuples.RemoveAt(0);
                tuples.RemoveAt(tuples.Count - 1);

                // set position of trail
                TrailRenderers[i].transform.localPosition = new Vector3(tuples.Last().x + (100 * (i + 1)), tuples.Last().y, -0.1f);
                
                // set colour of trail
                TrailRenderers[i].startColor = ColourFetcher.GetColour(player.GetPlayerIndex());
                TrailRenderers[i].endColor = ColourFetcher.GetColour(player.GetPlayerIndex());
                
                // dsplay trail data
                TrailRenderers[i].Clear();
                TrailRenderers[i].AddPositions(tuples.ToArray());

                // create lap time string
                var lapTimeDisplay = "Not complete";
                if (i < times.Count)
                {
                    var ms = times[i];

                    // calculate time components
                    int seconds = (int)(ms / 1000f);
                    int milliseconds = (int)((ms - (seconds * 1000f)));

                    lapTimeDisplay = $"{seconds.ToString("00")}.{milliseconds.ToString("000")} seconds";
                }

                // display data on 
                GalleryFrames[i].TxtLapAccuracy.text = i >= accuracies.Count ? "" : Math.Round(accuracies[i] * 100, 1) + "%";
                GalleryFrames[i].TxtLapTime.text = lapTimeDisplay;
                GalleryFrames[i].TxtLapPoints.text = i >= scores.Count ? "" : scores[i].ToString();
                yield return new WaitForSeconds(1);
                
                // show gallery sign
                GalleryFrames[i].PictureSign.gameObject.SetActive(true);

                // check if there is data for this
                if (i < scores.Count)
                {
                    // display points for this lap on a rosette
                    yield return new WaitForSeconds(1);
                    GalleryFrames[i].Rosette.gameObject.SetActive(true);

                    // if accuracy bonus was earned, display a second rosette
                    if (accuracies[i] > CarControllerScript.ACCURACY_BONUS_THRESHOLD)
                    {
                        yield return new WaitForSeconds(1);
                        GalleryFrames[i].AccuracyBonusRosette.gameObject.SetActive(true);
                    }
                }

                yield return new WaitForSeconds(1);
            }

            // show total points
            TxtTotalPoints.text = "Total: " + player.GetPoints() + "  points";
            yield return new WaitForSeconds(3);
        }

        // wait before end
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

        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(CartAttackInputHandler));
            player.Spawn(null, Vector3.zero);

            // set up the input handler
            var pl = player.GetComponent<CartAttackInputHandler>();
            pl.SetCarController(Cars[index]);
            CarStatuses[index].Initialise(pl.GetPlayerName(), index);
            VehicleSelection.VehicleSelectionDisplays[index].TxtPlayerName.text = pl.GetPlayerName();
            Cars[index].DriverRenderer.sprite = DriverSprites[player.GetCharacterIndex()];
            
            // add to list
            list.Add(pl);
        }

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
        for (; index < maximum; index++)
        {
            // hide car
            Cars[index].gameObject.SetActive(false);
            CarStatuses[index].gameObject.SetActive(false);
            VehicleSelection.VehicleSelectionDisplays[index].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // add bonus points for quickest lap
        if (_currentBestLapPlayer > -1)
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
    public void CheckFastestLap(int playerIndex, double lapTime)
    {
        // if faster than the current record, store this lap as the new record
        if (lapTime < _currentBestLap)
        {
            _currentBestLap = lapTime;
            _currentBestLapPlayer = playerIndex;
        }

        // update UIs
        for (int i = 0; i < CarStatuses.Length; i++)
        {
            CarStatuses[i].SetBestLap(i == _currentBestLapPlayer, _currentBestLap);
        }
    }

    /// <summary>
    /// Completes the game and return to object
    /// </summary>
    IEnumerator Complete_()
    {
        // assign points for winner
        AssignBonusPoints_();
        yield return new WaitForSeconds(3f);

        // show results
        ResultsScreen.Setup();
        GenericInputHandler[] genericPlayers = _players.ToArray<GenericInputHandler>();
        ResultsScreen.SetPlayers(genericPlayers);

        // store scores
        ScoreStoreHandler.StoreResults(Scene.CartAttack, genericPlayers);

        // wait for a while to show results
        yield return new WaitForSeconds(4 + genericPlayers.Length);

        // fade out
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        PlayerManagerScript.Instance.CentralScene();
    }

    /// <summary>
    /// Flips all players steering direction (other than the player that triggered it)
    /// </summary>
    /// <param id="plTrigger">The index of the player who triggered the power up</param>
    public void FlipSteering(int plTrigger)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            // don't flip direction of player who triggered the behaviour
            if (i != plTrigger)
                _players[i].FlipSteeringStarted();
        }
    }

    /// <summary>
    /// Stops flipping all players steering direction (other than the player that triggered it)
    /// </summary>
    /// <param id="plTrigger">The index of the player who triggered the power up</param>
    public void UnflipSteering(int plTrigger)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            // don't update player who triggered the behaviour
            if (i != plTrigger)
                _players[i].FlipSteeringStopped();
        }
    }
}
