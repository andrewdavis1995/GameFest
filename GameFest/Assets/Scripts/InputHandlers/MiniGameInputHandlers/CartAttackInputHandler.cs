using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input handler for Cart Attack mini-game
/// </summary>
public class CartAttackInputHandler : GenericInputHandler
{
    CarControllerScript _carController;

    bool _active = false;
    bool _vehicleSelected = false;

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

    /// <summary>
    /// Override controller for L1 button
    /// </summary>
    /// <param name="ctx">The input action</param>
    public override void OnL1()
    {
        if (!PauseGameHandler.Instance.IsPaused())
        {
            // if vehicle selection in progress, update UI
            if (CartAttackController.Instance.VehicleSelection.GetActiveState() && !_vehicleSelected)
            {
                CartAttackController.Instance.VehicleSelection.UpdateDisplay(GetPlayerIndex(), -1);
            }
        }

        base.OnL1();
    }

    /// <summary>
    /// Override controller for R1 button
    /// </summary>
    /// <param name="ctx">The input action</param>
    public override void OnR1()
    {
        if (!PauseGameHandler.Instance.IsPaused())
        {
            // if vehicle selection in progress, update UI
            if (CartAttackController.Instance.VehicleSelection.GetActiveState() && !_vehicleSelected)
            {
                CartAttackController.Instance.VehicleSelection.UpdateDisplay(GetPlayerIndex(), 1);
            }
        }

        base.OnR1();
    }

    /// <summary>
    /// Override controller for circle button
    /// </summary>
    /// <param name="ctx">The input action</param>
    public override void OnCircle()
    {
        base.OnCircle();
        if (PauseGameHandler.Instance.IsPaused()) return;

        // if vehicle selection in progress, update UI
        if (CartAttackController.Instance.VehicleSelection.GetActiveState())
        {
            _vehicleSelected = false;
            CartAttackController.Instance.VehicleSelection.Incomplete(GetPlayerIndex());
        }
    }

    /// <summary>
    /// Override controller for L2 button
    /// </summary>
    /// <param name="ctx">The input action</param>
    public override void OnL2(InputAction.CallbackContext ctx)
    {
        if (!CartAttackController.Instance.VehicleSelection.GetActiveState())
        {
            if (_active)
            {
                var y = ctx.ReadValue<float>();
                _carController.SetAccelerationValue(-y);
            }
        }
    }

    /// <summary>
    /// Override controller for R2 button
    /// </summary>
    /// <param name="ctx">The input action</param>
    public override void OnR2(InputAction.CallbackContext ctx)
    {
        if (!CartAttackController.Instance.VehicleSelection.GetActiveState() && CartAttackController.Instance.IsRunning())
        {
            if (_active)
            {
                var y = ctx.ReadValue<float>();
                _carController.SetAccelerationValue(y);
            }
        }
    }

    /// <summary>
    /// Override controller for cross button
    /// </summary>
    /// <param name="ctx">The input action</param>
    public override void OnCross()
    {
        base.OnCross();
        if (PauseGameHandler.Instance.IsPaused()) return;

        // if vehicle selection in progress, update UI
        if (CartAttackController.Instance.VehicleSelection.GetActiveState() && !PauseGameHandler.Instance.IsPaused())
        {
            _vehicleSelected = true;
            CartAttackController.Instance.VehicleSelection.Complete(GetPlayerIndex());
            CartAttackController.Instance.CheckVehicleSelectionComplete();
        }
    }
    /// <summary>
    /// Override controller for triangle button
    /// </summary>
    /// <param name="ctx">The input action</param>
    public override void OnTriangle()
    {
        _carController.ApplyPowerUp();
    }

    /// <summary>
    /// Override controller for moving the left stick/arrows
    /// </summary>
    /// <param name="ctx">The input action</param>
    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        if (!CartAttackController.Instance.VehicleSelection.GetActiveState())
        {
            if (_active)
            {
                var x = ctx.ReadValue<Vector2>().x;
                _carController.SetSteeringValue(x);

                // can accelerate with arrows if on PC Keyboard
                if (device is Keyboard)
                {
                    var y = ctx.ReadValue<Vector2>().y;
                    _carController.SetAccelerationValue(y);
                }

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
