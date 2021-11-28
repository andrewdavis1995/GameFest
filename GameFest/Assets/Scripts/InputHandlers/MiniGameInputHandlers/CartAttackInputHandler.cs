using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Input handler for Cart Attack mini-game
/// </summary>
public class CartAttackInputHandler : GenericInputHandler
{
    CarControllerScript _carController;

    bool _active = false;
    bool _vehicleSelected = false;

    /// <summary>
    /// Called once when the script begins
    /// </summary>
    private void Awake()
    {
        // TODO: remove once hooked up with proper controller and game system
        _carController = GetComponent<CarControllerScript>();
    }

    /// <summary>
    /// Sets whether the car can move
    /// </summary>
    /// <param id="state">Whether the car can move</param>
    public void SetActiveState(bool state)
    {
        _active = state;

        if (!_active)
        {
            _carController.SetAccelerationValue(0);
            _carController.SetSteeringValue(0);
        }
    }

    /// <summary>
    /// Does the necessary actions at the start of the race
    /// </summary>
    public void StartRace()
    {
        SetActiveState(true);
        _carController.StartRace();
    }

    /// <summary>
    /// Gets the laps completed for this player
    /// </summary>
    /// <returns>The lap data</returns>
    public List<List<Tuple<Vector3, bool>>> GetLaps()
    {
        return _carController.GetLaps();
    }
    /// <summary>
    /// Gets the times for the laps completed for this player
    /// </summary>
    /// <returns>The lap data</returns>
    public List<int> GetLapTimes()
    {
        return _carController.GetLapTimes();
    }

    /// <summary>
    /// Gets the scores for the laps completed for this player
    /// </summary>
    /// <returns>The lap data</returns>
    public List<int> GetLapScores()
    {
        return _carController.GetLapScores();
    }

    /// <summary>
    /// Gets the accuracy ratings for the laps completed for this player
    /// </summary>
    /// <returns>The lap data</returns>
    public List<float> GetLapAccuracies()
    {
        return _carController.GetLapAccuracies();
    }

    /// <summary>
    /// Sets the car to use for this player
    /// </summary>
    /// <param id="car">The car to use</param>
    public void SetCarController(CarControllerScript car)
    {
        _carController = car;
        _carController.Initialise(AddPoints, GetPlayerIndex());
    }

    /// <summary>
    /// Check if the vehicle selection is complete for this player
    /// </summary>
    /// <returns>Whether they have selected a vehicle</returns>
    public bool VehicleSelected()
    {
        return _vehicleSelected;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Move to inputsystem controls

        // if vehicle selection in progress, update UI
        if (CartAttackController.Instance.VehicleSelection.GetActiveState())
        {
            var direction = 0;

            if (Input.GetKeyDown(KeyCode.A))
                direction = -1;
            if (Input.GetKeyDown(KeyCode.D))
                direction = 1;

            if (direction != 0)
                CartAttackController.Instance.VehicleSelection.UpdateDisplay(GetPlayerIndex(), direction);

            if (Input.GetKeyDown(KeyCode.Return))
            {
                _vehicleSelected = true;
                CartAttackController.Instance.VehicleSelection.Complete(GetPlayerIndex());
                CartAttackController.Instance.CheckVehicleSelectionComplete();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _vehicleSelected = false;
                CartAttackController.Instance.VehicleSelection.Incomplete(GetPlayerIndex());
            }
            }
        else
        {
            if (_active)
            {
                var x = 0;
                var y = 0;

                if (Input.GetKey(KeyCode.A))
                    x = -1;
                if (Input.GetKey(KeyCode.D))
                    x = 1;
                if (Input.GetKey(KeyCode.W))
                    y = 1;
                if (Input.GetKey(KeyCode.S))
                    y = -1;

                if (Input.GetKey(KeyCode.Space))
                    _carController.ApplyPowerUp();

                _carController.SetAccelerationValue(y);
                _carController.SetSteeringValue(x);
            }
        }
    }

    /// <summary>
    /// A player has triggered a power up to flip this players steering direction
    /// </summary>
    public void FlipSteeringStarted()
    {
        _carController.FlipSteeringStarted();
    }

    /// <summary>
    /// A power up to flip this players steering direction has ended
    /// </summary>
    public void FlipSteeringStopped()
    {
        _carController.FlipSteeringStopped();
    }
}
