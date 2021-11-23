using UnityEngine;

/// <summary>
/// Input handler for Cart Attack mini-game
/// </summary>
public class CartAttackInputHandler : GenericInputHandler
{
    CarControllerScript _carController;
    
    bool _active = false;

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
    }
    
    /// <summary>
    /// Sets the car to use for this player
    /// </summary>
    /// <param id="car">The car to use</param>
    public void SetCarController(CarController car)
    {
        _carController = car;
    }

    // Update is called once per frame
    void Update()
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

        // TODO: Move to inputsystem controls
        if(_active)
        {
            if (Input.GetKey(KeyCode.Space))
                _carController.Boost();

            _carController.SetAccelerationValue(y);
            _carController.SetSteeringValue(x);
        }
    }
}
