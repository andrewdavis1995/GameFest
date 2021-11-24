using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class to control car movement (Cart Attack)
/// </summary>
public class CarControllerScript : MonoBehaviour
{
    const int MAX_LAP_POINTS = 350;
    const int LOWEST_LAP_POINTS = 20;
    const int ACCURACY_BONUS = 25;

    List<List<Tuple<Vector3, bool>>> _lapDrawings = new List<List<Tuple<Vector3, bool>>>();

    // configurable parameters about car movement
    public float AccelerationFactor = 30.0f;
    public float TurnFactor = 3.5f;
    public float DriftFactor = 0.95f;
    public float MaxSpeed = 30f;
    public float MaxDrag = 3f;
    public float AccelerationSpeed = 3f;
    public float BoostFactor = 1.1f;

    // status variables for car movement
    float _accelerationInput = 0;
    float _steeringInput = 0;
    float _rotationAngle = 0;
    bool _boosting = false;
    int _checkpointIndex = 0;
    int _trailPositions = 0;
    float _lapPoints = 0;
    int _stopwatchTime = 0;
    int _playerIndex = 0;

    List<Collider2D> _outOfBoundsZone = new List<Collider2D>();

    // link to car elements
    Rigidbody2D carRigidBody;
    public TrailRenderer[] Trails;
    public TrailRenderer DrawTrail;
    Action<int> _addPointsCallback;

    TimeLimit _lapTimer;
    TimeStopwatch _lapStopwatch;

    /// <summary>
    /// Called when the script starts up
    /// </summary>
    private void Awake()
    {
        _lapTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _lapTimer.Initialise(MAX_LAP_POINTS - LOWEST_LAP_POINTS, lapTimerTick_, null, 0.1f);
        
        _lapStopwatch = (TimeStopwatch)gameObject.AddComponent(typeof(TimeStopwatch));
        _lapStopwatch.Initialise(lapStopwatchTick_, .001f);

        carRigidBody = GetComponent<Rigidbody2D>();
        _lapDrawings.Add(new List<Tuple<Vector3, bool>>());
    }

    /// <summary>
    /// Initialises the controller with necessary values
    /// </summary>
    /// <param id="pointsCallback">Function to call when points need to be added</param>
    /// <param id="playerIndex">The index of the player</param>
    public void Initialise(Action<int> pointsCallback, int playerIndex)
    {
        _addPointsCallback = pointsCallback;
        _playerIndex = playerIndex;
    }

    /// <summary>
    /// Starts the points ticking down for the lap
    /// </summary>
    void StartLapTimer_()
    {
        // points countdown 
        _lapTimer.Abort();
        _lapPoints = MAX_LAP_POINTS;
        _lapTimer.StartTimer();
        
        // time stopwatch
        _stopwatchTime = 0;
        _lapStopwatch.StartTimer();
    }

    /// <summary>
    /// Does the necessary actions at the start of the race
    /// </summary>
    public void StartRace()
    {
        StartLapTimer_();
    }

    /// <summary>
    /// Callback for the lap timer
    /// </summary>
    /// <param name="points">Remaining points</param>
    private void lapTimerTick_(int points)
    {
        _lapPoints = LOWEST_LAP_POINTS + points;
    }

    /// <summary>
    /// Callback for the lap stopwatch
    /// </summary>
    /// <param name="points">Current time</param>
    private void lapStopwatchTick_(int time)
    {
        _stopwatchTime = time;
    }

    /// <summary>
    /// Causes a brief increase in speed
    /// </summary>
    public void Boost()
    {
        if (!_boosting)
            StartCoroutine(Boost_());
    }
    
    /// <summary>
    /// Causes a brief increase in speed
    /// </summary>
    IEnumerator Boost_()
    {
        _boosting = true;
        MaxSpeed *= BoostFactor;
        
        // enforce the boost for 2 seconds
        yield return new WaitForSeconds(2);
        
        // return to normal speed
        _boosting = false;
        MaxSpeed /= BoostFactor;
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    private void FixedUpdate()
    {
        ApplyEngineForce_();
        KillOrthogonalVelocity_();
        ApplySteering_();
        CheckNewTrail_();
    }
    
    /// <summary>
    /// Checks if new positions have been added to the drawing (colour) trail
    /// </summary>
    void CheckNewTrail_()
    {
        // check if there are more positions than there previously were
        if(DrawTrail.positionCount > _trailPositions)
        {
            // get all positions
            Vector3[] trailPositions = new Vector3[DrawTrail.positionCount];
            DrawTrail.GetPositions(trailPositions);

            // add the new ones to the list, along with whether they were in bounds or not
            for(var i = _trailPositions; i < DrawTrail.positionCount; i++)
            {
                _lapDrawings.Last().Add(new Tuple<Vector3, bool>(trailPositions[i], CurrentlyInBounds_()));
            }
        }
    
        // store the current count of items
        _trailPositions = DrawTrail.positionCount;
    }
    
    /// <summary>
    /// Checks if the car is in the bounds of the track
    /// </summary>
    bool CurrentlyInBounds_()
    {
        return _outOfBoundsZone.Count == 0;
    }

    /// <summary>
    /// Turns the skid mark trails on or off
    /// </summary>
    /// <param id="state">The state to set the trail emitters to (true = ON, false = OFF)</param>
    void ToggleTrail(bool state)
    {
        foreach (var t in Trails)
            t.emitting = state;
    }

    /// <summary>
    /// Causes the car to drive forward
    /// </summary>
    void ApplyEngineForce_()
    {
        // get the forward speed
        float velocityVsUp = Vector2.Dot(transform.up, carRigidBody.velocity);

        // limit speed
        if (velocityVsUp > MaxSpeed && _accelerationInput > 0)
            return;

        // limit reverse speed
        if ((velocityVsUp < -MaxSpeed * 0.5f) && _accelerationInput < 0)
            return;

        // adjust drag based on acceleration input
        if (_accelerationInput == 0)
            carRigidBody.drag = Mathf.Lerp(carRigidBody.drag, MaxDrag, Time.fixedDeltaTime * AccelerationSpeed);
        else
            carRigidBody.drag = Mathf.Lerp(carRigidBody.drag, 0f, Time.fixedDeltaTime * 2);

        // apply force
        var engineForceVector = transform.up * _accelerationInput * AccelerationFactor;
        carRigidBody.AddForce(engineForceVector, ForceMode2D.Force);
    }

    /// <summary>
    /// Handle the steering/change in direction of car
    /// </summary>
    void ApplySteering_()
    {
        // check if we are moving fast enough to turn
        float minSpeed = (carRigidBody.velocity.magnitude / 8);
        minSpeed = Mathf.Clamp01(minSpeed);
        
        // rotate the car
        _rotationAngle -= (_steeringInput * TurnFactor * minSpeed);
        carRigidBody.MoveRotation(_rotationAngle);
    }

    /// <summary>
    /// Updates the value of how much steering is going on, based on value from input handler
    /// </summary>
    /// <param id="steerValue">The x-direction input (from -1 (left) to 1 (right))</param>
    public void SetSteeringValue(float steerValue)
    {
        _steeringInput = steerValue;
        
        // if steering, turn skid marks on
        var emit = (Math.Abs(_steeringInput) > 0.5f) && (_accelerationInput > 0.5f);
        ToggleTrail(emit);
    }

    /// <summary>
    /// Updates the value of how much acceleration is going on, based on value from input handler
    /// </summary>
    /// <param id="acceleration">The y-direction input (from -1 (bottom) to 1 (top))</param>
    public void SetAccelerationValue(float acceleration)
    {
        _accelerationInput = acceleration;
    }

    /// <summary>
    /// Stops the car from skidding around in the wrong direction
    /// </summary>
    void KillOrthogonalVelocity_()
    {
        // get directional velocities
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidBody.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidBody.velocity, transform.right);

        // correct the velocity
        carRigidBody.velocity = forwardVelocity + rightVelocity * DriftFactor;
    }

    /// <summary>
    /// Called when the car enters an out-of-bounds trigger
    /// </summary>
    /// <param id="collision">The trigger that was entered</param>
    public void OutOfBoundsEntered(Collider2D collision)
    {
        _outOfBoundsZone.Add(collision);
    }

    /// <summary>
    /// Called when the car leaves an out-of-bounds trigger
    /// </summary>
    /// <param id="collision">The trigger that was exited</param>
    public void OutOfBoundsExited(Collider2D collision)
    {
        _outOfBoundsZone.Remove(collision);
    }

    /// <summary>
    /// Clears the existing lap, and starts a new one
    /// </summary>
    internal void LapComplete_()
    {
        // add points based on time and accuracy of _lapDrawings
        var onTrack = _lapDrawings.Last().Count(p => p.Item2);
        var offTrack = _lapDrawings.Last().Count(p => !p.Item2);

        var score = onTrack / (float)(onTrack + offTrack);
        _addPointsCallback((int)(score * _lapPoints));

        // add bonus points for staying in the lines
        if(score > 0.9f)
        {
            _addPointsCallback(ACCURACY_BONUS);
        }

        // new item on lap drawing list
        _lapDrawings.Add(new List<Tuple<Vector3, bool>>());

        // clear trail
        DrawTrail.Clear();
        _trailPositions = 0;
        
        // back to first checkpoint
        _checkpointIndex = 0;
        
        // check if this was the fastest lap - store if it is
        CartAttackController.Instance.CheckFastestLap(_playerIndex, _lapStopwatch.GetCurrentTime());

        // restart lap timer
        StartLapTimer_();
    }

    /// <summary>
    /// Called when the car enters a checkpoint trigger
    /// </summary>
    /// <param id="collision">The trigger that was entered</param>
    internal void NextCheckpoint(Collider2D collider)
    {
        // check that the checkpoint is the one the car is supposed to be add 
        if (CorrectCheckpoint_(collider))
        {
            // move to next checkpoint
            _checkpointIndex++;

            // if this was the finish, end the lap
            if (collider.gameObject.tag == "Finish")
            {
                LapComplete_();
            }
        }
    }

    /// <summary>
    /// Checks if the car is at the correct checkpoint, or they've gone to an incorrect one
    /// </summary>
    /// <param id="collision">The trigger that was entered</param>
    private bool CorrectCheckpoint_(Collider2D collider)
    {
        // check the list for the current index and make sure that the one the car is at is the correct one
        return (collider == CartAttackController.Instance.Checkpoints[_checkpointIndex]);
    }
}
