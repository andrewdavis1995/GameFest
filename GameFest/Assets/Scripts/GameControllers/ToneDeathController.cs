using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Instrument { None, Drums, Bass }

public class ToneDeathController : GenericController
{
    float FLOOR_HEIGHT = 10f;
    uint FLOOR_COUNT = 3;
    const float ELEVATOR_SPEED = 0.25f;
    const int OVERALL_LEVEL_TIMEOUT = 120;
    const int ELEVATOR_TIMEOUT = 30;
    const int INSTRUMENT_TIMEOUT = 10;

    public static ToneDeathController Instance;

    public Transform PlayerPrefab;
    public Vector3[] StartPositions;
    public ElevatorScript[] Elevators;
    public Sprite[] ElevatorDoors;
    public Sprite[] DisabledImages;
    public Material[] InstrumentMaterials;
    public AudioClip[] AudioTracks;

    public float INSTRUMENT_ELEVATOR_POSITION = 0f;

    List<ToneDeathInputHandler> _players;
    int _elevatorIndex = 0;
    bool _selectingInstrument = true;
    public bool InstrumentRunOff = false;

    // timers
    TimeLimit _levelTimer;
    TimeLimit _elevatorEndTimer;
    TimeLimit _instrumentSelectionTimer;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // initialise timers
        _levelTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _levelTimer.Initialise(OVERALL_LEVEL_TIMEOUT, levelTimerTick_, levelTimerComplete_, 1f);
        _elevatorEndTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _elevatorEndTimer.Initialise(ELEVATOR_TIMEOUT, elevatorTimerTick_, elevatorTimerComplete_, 1f);
        _instrumentSelectionTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _instrumentSelectionTimer.Initialise(INSTRUMENT_TIMEOUT, null, InstrumentTimeout_, 1f);

        // get the distance between floors
        FLOOR_HEIGHT = Elevators[1].transform.position.y - Elevators[0].transform.position.y;

        // TEMP
        _players = FindObjectsOfType<ToneDeathInputHandler>().ToList();
        foreach (var pl in _players)
        {
            var movement = Instantiate(PlayerPrefab, StartPositions[0], Quaternion.identity).GetComponent<PlayerMovement>();
            pl.InitialisePlayer(movement);
        }

        // TODO: Move to after pause menu is closed
        _instrumentSelectionTimer.StartTimer();

        // TODO
        //SpawnPlayers_();

        foreach (var sel in FindObjectsOfType<InstrumentSelection>())
            sel.Setup(_players.Count);
        foreach (var s in FindObjectsOfType<SpeakerScript>())
            s.Setup(_players.Count);
    }

    /// <summary>
    /// Starts the gameplay
    /// </summary>
    void StartGame_()
    {
        // start the timer for the level
        _levelTimer.StartTimer();
    }

    /// <summary>
    /// Callback for when the timer ticks
    /// </summary>
    private void levelTimerTick_(int time)
    {
        // TODO: show remaining time once it get below 20 seconds or so
        //Debug.Log(time + " left in level");
    }

    /// <summary>
    /// Callback for when the timer completes
    /// </summary>
    private void levelTimerComplete_()
    {
        KillIncompletePlayers_();
    }

    /// <summary>
    /// Kills any players who are not already complete
    /// </summary>
    void KillIncompletePlayers_()
    {
        // mark all players as complete and close elevator
        foreach (var p in _players)
        {
            if (!p.FloorComplete())
            {
                // if the player did not make it to the escalator, they are dead
                p.DamageDone(10000f);
            }
        }

        StartCoroutine(NextLevel_());
    }

    /// <summary>
    /// Callback for when the timer ticks
    /// </summary>
    private void elevatorTimerTick_(int time)
    {
        // TODO: show remaining time (above door)
        //Debug.Log(time + " left after other players");
    }

    /// <summary>
    /// Callback for when the timer completes
    /// </summary>
    private void elevatorTimerComplete_()
    {
        KillIncompletePlayers_();
    }

    /// <summary>
    /// Timeout occurred when waiting for instruments to be selected
    /// </summary>
    void InstrumentTimeout_()
    {
        // TODO: set remaining players

        var allInstruments = FindObjectsOfType<InstrumentSelection>().Where(i => !i.Set()).ToList();
        foreach(var p in _players)
        {
            // if the player hasn't selected an item, generate one
            if(!p.Movement.Disabled())
            {
                // generate a random value
                var i = UnityEngine.Random.Range(0, allInstruments.Count());
                p.SetInstrument(allInstruments[i]);
                allInstruments.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Checks if all players are complete, and moves to the next level if applicable
    /// </summary>
    public void CheckAllPlayersComplete()
    {
        // if all done, move elevator
        if (_players.All(p => p.FloorComplete() || p.Died()))
        {
            StartCoroutine(NextLevel_());
        }
        else
        {
            // start the elevator end timer (hurry other players)
            if (!_elevatorEndTimer.Running())
                _elevatorEndTimer.StartTimer();
        }
    }

    /// <summary>
    /// Checks if all players are complete, and moves to the next level if applicable
    /// </summary>
    public void CheckAllInstrumentsSelected()
    {
        _instrumentSelectionTimer.Abort();

        // if all done, move elevator
        if (_players.All(p => p.GetInstrument() != Instrument.None))
        {
            InstrumentRunOff = true;

            foreach (var p in _players)
            {
                p.Movement.AutoPilot(true, Elevators[0].transform.position.x - (Elevators[0].transform.localScale.x / 2) + (0.2f * p.GetPlayerIndex()));
                p.Movement.Move(new Vector2(-1, 0));
            }
        }
    }

    /// <summary>
    /// All players complete or dead. Move to next floor
    /// </summary>
    private IEnumerator NextLevel_()
    {
        _elevatorEndTimer.Abort();
        _levelTimer.Abort();

        // if everyone dead, end the game
        if (_players.All(p => p.Died()))
        {
            EndGame_();
        }
        else
        {
            // close doors
            for (var i = 0; i < ElevatorDoors.Count(); i++)
            {
                // loop through images
                Elevators[_elevatorIndex].Doors.sprite = ElevatorDoors[i];
                yield return new WaitForSeconds(.05f);
                foreach (var speaker in Elevators[_elevatorIndex].Speakers)
                {
                    speaker.volume -= 0.05f;
                }
            }

            // decrease speaker volume
            while (Elevators[_elevatorIndex].Speakers.Count() > 0 && Elevators[_elevatorIndex].Speakers[0].volume > 0)
            {
                foreach (var speaker in Elevators[_elevatorIndex].Speakers)
                {
                    speaker.volume -= 0.1f;
                    yield return new WaitForSeconds(0.05f);
                }
            }

            // hide all players (before moving up)
            foreach (var v in _players)
            {
                v.Hide();
            }

            // move elevator and camera up
            for (float i = 0; i < FLOOR_HEIGHT; i += ELEVATOR_SPEED)
            {
                Camera.main.transform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));
                Elevators[_elevatorIndex].Platform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));
                yield return new WaitForSeconds(.01f);
            }

            // show all players (behind doors)
            foreach (var v in _players)
            {
                v.Show();
            }

            // extra bit so they are above the floor
            Elevators[_elevatorIndex].Platform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));

            // open the top doors
            if (Elevators[_elevatorIndex].Top != null)
            {
                // loop through each image
                for (var i = ElevatorDoors.Count() - 1; i >= 0; i--)
                {
                    Elevators[_elevatorIndex].Top.Doors.sprite = ElevatorDoors[i];
                    yield return new WaitForSeconds(.05f);
                }
            }

            // make players leave elevator
            foreach (var v in _players)
            {
                v.PlayerExitElevator();
            }

            // next floor
            _elevatorIndex++;

            // end game if this was the last level
            if (_elevatorIndex >= FLOOR_COUNT)
                EndGame_();
            else
            {
                // start the timer for the level
                _levelTimer.StartTimer();
            }
        }
    }

    /// <summary>
    /// If players are selecting instruments
    /// </summary>
    /// <returns>Whether we are at the instrument selection stage</returns>
    public bool InstrumentSelect()
    {
        return _selectingInstrument;
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    private void EndGame_()
    {
        AssignBonusPoints_();
    
        // TODO: Temp. Move to fade out
        SceneManager.LoadScene((int)Scene.GameCentral);
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(ToneDeathInputHandler));

            // create the "visual" player at the start point
            var tr = player.Spawn(PlayerPrefab, StartPositions[index++]);
            var ih = player.GetComponent<ToneDeathInputHandler>();
            ih.InitialisePlayer(tr.GetComponent<PlayerMovement>());

            _players.Add(ih);
        }
    }

    /// <summary>
    /// Gets the audio track for the specified player
    /// </summary>
    /// <param name="index">The player to get audio for</param>
    /// <returns>The audio clip to use</returns>
    public AudioClip GetAudioTrack(int index)
    {
        return AudioTracks[(int)_players[index].GetInstrument() - 1];
    }
    
    /// <summary>
    /// Gets a list of transforms linked to the player movements
    /// </summary>
    /// <returns>List of player objects</returns>
    public List<Transform> GetPlayers()
    {
        return _players.Select(p => p.Movement.transform).ToList();
    }

    /// <summary>
    /// Checks if all players have had instruments selected
    /// </summary>
    internal void CheckInstrumentElevatorComplete()
    {
        if (_players.All(p => p.Movement.AutoPilot() == false))
        {
            foreach (var p in _players)
            {
                p.Movement.Reenable();
                p.EnterElevator();
            }

            // done. Close door
            StartCoroutine(NextLevel_());

            InstrumentRunOff = false;
            _selectingInstrument = false;

            // start the timer
            StartGame_();
        }
    }

    /// <summary>
    /// Assigns points to the player who hit an enemy
    /// </summary>
    /// <param name="points">The points to assign</param>
    /// <param name="playerIndex">The index of the player to assign points to</param>
    internal void AssignHitPoints(int points, int playerIndex)
    {
        if(playerIndex >= 0 && playerIndex < _players.Count)
        {
            // add points
            _players[playerIndex].AddPoints(points);
        }
    }   
    
    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // sort the players by points scored
        var ordered = _players.Where(p => p.GetPoints() > 0).OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 100, 40, 15 };

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
}
