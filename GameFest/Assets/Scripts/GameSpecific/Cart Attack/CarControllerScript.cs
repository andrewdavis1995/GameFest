using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum PowerUp { None, SpeedBoost, ReverseSteering, PinDrop }

/// <summary>
/// Class to control car movement (Cart Attack)
/// </summary>
public class CarControllerScript : MonoBehaviour
{
    const int MAX_LAP_POINTS = 350;
    const int LOWEST_LAP_POINTS = 20;
    const int ACCURACY_BONUS = 30;
    public static float ACCURACY_BONUS_THRESHOLD = 0.85f;
    const float BOOST_FACTOR = 1.2f;
    const float BOOST_DURATION = 1.6f;
    const float ROCKET_BOOSTER_OFFSET = -0.9f;
    const float FLIP_DURATION = 5f;

    List<List<Tuple<Vector3, bool>>> _lapDrawings = new List<List<Tuple<Vector3, bool>>>();
    List<int> _lapScores = new List<int>();
    List<float> _lapAccuracies = new List<float>();
    List<int> _lapTimes = new List<int>();

    // configurable parameters about car movement
    public float AccelerationFactor = 30.0f;
    public float TurnFactor = 3.5f;
    public float DriftFactor = 0.95f;
    public float MaxSpeed = 30f;
    public float MaxDrag = 3f;
    public float AccelerationSpeed = 3f;

    // status variables for car movement
    float _accelerationInput = 0;
    float _steeringInput = 0;
    float _rotationAngle = 0;
    int _checkpointIndex = 0;
    int _trailPositions = 0;
    float _lapPoints = 0;
    int _playerIndex = 0;
    bool _powerUpCycle = false;
    int _flipSteeringRequests = 0;
    DateTime _lapStart;

    List<Collider2D> _outOfBoundsZone = new List<Collider2D>();

    // link to car elements
    public TrailRenderer[] Trails;
    public TrailRenderer DrawTrail;
    public Transform PinPrefab;
    public BoxCollider2D CollisionCollider;
    public BoxCollider2D TriggerCollider;
    public Transform RocketBooster;
    public SpriteRenderer BodyRenderer;
    public SpriteRenderer DriverRenderer;
    public SpriteRenderer BaseRenderer;
    public SpriteRenderer LeftWheelRenderer;
    public SpriteRenderer RightWheelRenderer;
    public SpriteRenderer BackWheelsRenderer;
    public SpriteRenderer GlassRenderer;
    public Transform ControlsFlippedIcon;
    public AudioSource LapSound;
    public AudioSource PowerUpSound;
    public AudioSource MotorSound;
    public AudioSource SkidSound;

    Action<int> _addPointsCallback;
    PowerUp _activePowerUp = PowerUp.None;
    Rigidbody2D _carRigidBody;

    TimeLimit _lapTimer;

    /// <summary>
    /// Called when the script starts up
    /// </summary>
    private void Awake()
    {
        _lapTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _lapTimer.Initialise(MAX_LAP_POINTS - LOWEST_LAP_POINTS, lapTimerTick_, null, 0.1f);

        _carRigidBody = GetComponent<Rigidbody2D>();
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
        _lapStart = DateTime.Now;
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
    /// Applies the power up that was collected
    /// </summary>
    public void ApplyPowerUp()
    {
        if (!_powerUpCycle)
        {
            switch (_activePowerUp)
            {
                case PowerUp.SpeedBoost:
                    StartCoroutine(Boost_());
                    break;
                case PowerUp.ReverseSteering:
                    StartCoroutine(ReverseSteering_());
                    break;
                case PowerUp.PinDrop:
                    StartCoroutine(DropPin_());
                    break;
            }

            // no power up left
            _activePowerUp = PowerUp.None;

            // hide power up icon
            CartAttackController.Instance.CarStatuses[_playerIndex].HidePowerUpIcon();
        }
    }

    /// <summary>
    /// Causes a brief increase in speed
    /// </summary>
    IEnumerator Boost_()
    {
        RocketBooster.gameObject.SetActive(true);
        MaxSpeed *= BOOST_FACTOR;

        // show booster
        for (float i = 0; i >= ROCKET_BOOSTER_OFFSET; i -= 0.01f)
        {
            RocketBooster.localPosition = new Vector3(0, i, 0.1f);
            yield return new WaitForSeconds(0.01f);
        }

        // enforce the boost for 2 seconds
        yield return new WaitForSeconds(BOOST_DURATION);

        // hide booster
        for (float i = ROCKET_BOOSTER_OFFSET; i <= 0; i += 0.1f)
        {
            RocketBooster.localPosition = new Vector3(0, i, 0.1f);
            yield return new WaitForSeconds(0.1f);
        }

        // return to normal speed
        MaxSpeed /= BOOST_FACTOR;
        RocketBooster.gameObject.SetActive(true);
    }

    /// <summary>
    /// Drops a pin
    /// </summary>
    IEnumerator DropPin_()
    {
        // briefly delay
        yield return new WaitForSeconds(0.25f);

        // drop pin at current point
        var created = Instantiate(PinPrefab, transform.position, Quaternion.identity);
        created.GetComponentsInChildren<SpriteRenderer>()[1].color = ColourFetcher.GetColour(_playerIndex);

        // can pass through briefly (gives time for car to drive away)
        created.GetComponent<Collider2D>().isTrigger = true;
        yield return new WaitForSeconds(0.1f);
        created.GetComponent<Collider2D>().isTrigger = false;
    }

    /// <summary>
    /// Causes a brief increase in speed
    /// </summary>
    IEnumerator ReverseSteering_()
    {
        // tell controller to flip player steering
        CartAttackController.Instance.FlipSteering(_playerIndex);

        for (int i = 0; i < FLIP_DURATION; i++)
        {
            yield return new WaitForSeconds(1);
        }

        // tell controller to stop flipping player steering
        CartAttackController.Instance.UnflipSteering(_playerIndex);
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

        if (_flipSteeringRequests > 0)
            ControlsFlippedIcon.eulerAngles += new Vector3(0, 0, 2f);
    }

    /// <summary>
    /// Checks if new positions have been added to the drawing (colour) trail
    /// </summary>
    void CheckNewTrail_()
    {
        // check if there are more positions than there previously were
        if (DrawTrail.positionCount > _trailPositions)
        {
            // get all positions
            Vector3[] trailPositions = new Vector3[DrawTrail.positionCount];
            DrawTrail.GetPositions(trailPositions);

            // add the new ones to the list, along with whether they were in bounds or not
            for (var i = _trailPositions; i < DrawTrail.positionCount; i++)
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
        MotorSound.volume = _accelerationInput / 50f;

        // get the forward speed
        float velocityVsUp = Vector2.Dot(transform.up, _carRigidBody.velocity);

        // limit speed
        if (velocityVsUp > MaxSpeed && _accelerationInput > 0)
            return;

        // limit reverse speed
        if ((velocityVsUp < -MaxSpeed * 0.5f) && _accelerationInput < 0)
            return;

        // adjust drag based on acceleration input
        if (_accelerationInput == 0)
            _carRigidBody.drag = Mathf.Lerp(_carRigidBody.drag, MaxDrag, Time.fixedDeltaTime * AccelerationSpeed);
        else
            _carRigidBody.drag = Mathf.Lerp(_carRigidBody.drag, 0f, Time.fixedDeltaTime * 2);

        // apply force
        var engineForceVector = transform.up * _accelerationInput * AccelerationFactor;
        _carRigidBody.AddForce(engineForceVector, ForceMode2D.Force);
    }

    /// <summary>
    /// A player has triggered a power up to flip this players steering direction
    /// </summary>
    public void FlipSteeringStarted()
    {
        _flipSteeringRequests++;
        ControlsFlippedIcon.gameObject.SetActive(true);
    }

    /// <summary>
    /// A power up to flip this players steering direction has ended
    /// </summary>
    public void FlipSteeringStopped()
    {
        _flipSteeringRequests--;
        ControlsFlippedIcon.gameObject.SetActive(_flipSteeringRequests > 0);
    }

    /// <summary>
    /// Gets the laps completed for this player
    /// </summary>
    /// <returns>The lap data</returns>
    public List<List<Tuple<Vector3, bool>>> GetLaps()
    {
        return _lapDrawings;
    }

    /// <summary>
    /// Gets the times for the laps completed for this player
    /// </summary>
    /// <returns>The lap data</returns>
    public List<int> GetLapTimes()
    {
        return _lapTimes;
    }

    /// <summary>
    /// Gets the scores for the laps completed for this player
    /// </summary>
    /// <returns>The lap data</returns>
    public List<int> GetLapScores()
    {
        return _lapScores;
    }

    /// <summary>
    /// Gets the accuracy ratings for the laps completed for this player
    /// </summary>
    /// <returns>The lap data</returns>
    public List<float> GetLapAccuracies()
    {
        return _lapAccuracies;
    }

    /// <summary>
    /// Handle the steering/change in direction of car
    /// </summary>
    void ApplySteering_()
    {
        // check if we are moving fast enough to turn
        float minSpeed = (_carRigidBody.velocity.magnitude / 8);
        minSpeed = Mathf.Clamp01(minSpeed);

        // change direction if steering is flipped
        var flipFactor = (_flipSteeringRequests > 0) ? -1 : 1;

        // rotate the car
        _rotationAngle -= (_steeringInput * TurnFactor * minSpeed * flipFactor);
        _carRigidBody.MoveRotation(_rotationAngle);
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
        SkidSound.volume = emit ? 0.2f : 0f;
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
        Vector2 forwardVelocity = transform.up * Vector2.Dot(_carRigidBody.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(_carRigidBody.velocity, transform.right);

        // correct the velocity
        _carRigidBody.velocity = forwardVelocity + rightVelocity * DriftFactor;
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
        LapSound.pitch += 0.1f;
        LapSound.Play();

        // add points based on time and accuracy of _lapDrawings
        var onTrack = _lapDrawings.Last().Count(p => p.Item2);
        var offTrack = _lapDrawings.Last().Count(p => !p.Item2);

        var score = onTrack / (float)(onTrack + offTrack);
        _lapAccuracies.Add(score);
        _lapScores.Add((int)(score * _lapPoints));
        _addPointsCallback((int)(score * _lapPoints));

        // add bonus points for staying in the lines
        if (score > ACCURACY_BONUS_THRESHOLD)
        {
            _addPointsCallback(ACCURACY_BONUS);
        }

        // back to first checkpoint
        _checkpointIndex = 0;

        CartAttackController.Instance.CarStatuses[_playerIndex].SetLapCount(_lapDrawings.Count);

        var lapEnd = DateTime.Now;
        var duration = lapEnd - _lapStart;

        var ms = (duration.Minutes * 1000 * 60) + (duration.Seconds * 1000) + duration.Milliseconds;
        _lapTimes.Add(ms);

        // check if this was the fastest lap - store if it is
        CartAttackController.Instance.CheckFastestLap(_playerIndex, ms);

        // new item on lap drawing list
        _lapDrawings.Add(new List<Tuple<Vector3, bool>>());

        // clear trail
        DrawTrail.Clear();
        _trailPositions = 0;

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

    /// <summary>
    /// Called when a trigger is entered
    /// </summary>
    /// <param id="collision">The trigger that was entered</param>
    void OnTriggerEnter2D(Collider2D collider)
    {
        // if collided with power up, pick it up
        if (collider.gameObject.tag == "PowerUp")
        {
            PowerUpSound.Play();

            if (_activePowerUp == PowerUp.None)
            {
                // start selecting a power up
                StartCoroutine(SetPowerUp_());
            }

            // destroy power up
            collider.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Selects a random power up
    /// </summary>
    /// <param id="collision">The trigger that was entered</param>
    IEnumerator SetPowerUp_()
    {
        // check a power up is not happening
        if (!_powerUpCycle)
        {
            _powerUpCycle = true;

            var numOptions = (Enum.GetNames(typeof(PowerUp)).Length) - 1;

            // generate a random value
            var iterations = UnityEngine.Random.Range(15, 30);

            var index = 0;

            // flick through the options
            for (int i = 0; i < iterations; i++)
            {
                index = i % numOptions;

                // update UI with image
                CartAttackController.Instance.CarStatuses[_playerIndex].UpdatePowerUpIcon(index);

                // wait before flickering
                yield return new WaitForSeconds(0.15f);
            }

            index = iterations % numOptions;

            // set the active power up
            _activePowerUp = (PowerUp)((index) + 1);
            CartAttackController.Instance.CarStatuses[_playerIndex].UpdatePowerUpIcon(index);

            _powerUpCycle = false;
        }
    }
}
