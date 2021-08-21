using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

struct PlayerScores
{
    public PlayerControls Player { get; set; }
    public int[][] Scores { get; set; }
}

/// <summary>
/// Controller for the "Beach Bowles" game
/// </summary>
public class BeachBowlesController : MonoBehaviour
{
    // Unity items
    public Rigidbody2D Ball;
    public BowlsBallScript BallScript;
    public Transform Arrow;
    public Image ThrowStyleImage;
    public Sprite[] ThrowStyleSprites;
    public Image WindImage;

    // status variables
    bool _selectingDirection = false;
    bool _selectingDistance = false;
    bool _overarm = false;
    Vector2 _ballLocation = Vector2.zero;
    List<BowlZoneScript> _activeZones = new List<BowlZoneScript>();
    int _activePlayerIndex = 0;
    Vector2 _windDirection = new Vector2(0, 0);
    int _pointsThisThrow = 0;
    int _doubleFactor = 1;
    bool _doubleHitThisThrow = false;
    bool _centreHitThisThrow = false;

    int _throwIndex = 0;
    int _roundIndex = 0;

    // constants for the arrow/start position
    const float SWING_AMOUNT = 40f;
    const float SWING_SPEED = 1.1f;
    const float SWING_DELAY = 0.01f;
    const float MOVE_FREEDOM = 13f;

    // constants for the ball movement
    const float FIRE_FORCE = 3000;
    const float DRAG_SKY = 0.7f;
    const float DRAG_GROUND = 1.4f;
    const float CAMERA_OFFSET = -9f;

    // static instance that can be accessed from other scripts
    public static BeachBowlesController Instance;

    // Start is called before the first frame update
    void Start()
    {
        // store static instance
        Instance = this;

        // get the location of the ball at the start
        _ballLocation = Ball.transform.localPosition;

        // move everything to starting position
        ResetPositions_();
    }

    /// <summary>
    /// Called from the ball script when the ball stops moving
    /// </summary>
    internal void BallStopped()
    {
        _pointsThisThrow += _activeZones.Count > 0 ? _activeZones.LastOrDefault().PointValue : 0;
        StartCoroutine(ShowPoints_());
    }

    /// <summary>
    /// Shows the points 
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowPoints_()
    {
        yield return new WaitForSeconds(1);

        // TODO: Show in UI
        Debug.Log(_pointsThisThrow);

        yield return new WaitForSeconds(2);

        // move everything back to their original locations
        ResetPositions_();
    }

    /// <summary>
    /// Moves all items back to their starting places
    /// </summary>
    void ResetPositions_()
    {
        // clear statuses
        _overarm = false;
        ThrowStyleImage.sprite = ThrowStyleSprites[_overarm ? 1 : 0];

        _activeZones.Clear();

        // info about this round
        _pointsThisThrow = 0;
        _doubleHitThisThrow = false;
        _centreHitThisThrow = false;

        // move items back
        Ball.transform.localPosition = _ballLocation;
        Arrow.localScale = new Vector3(1, 1, 1);
        Ball.transform.eulerAngles = new Vector3(0, 0, 0);
        Ball.GetComponent<BowlsBallScript>().ResetBall();

        // start moving arrow again
        StartCoroutine(SelectDirection());
    }

    /// <summary>
    /// Called when the centre stick is hit by the ball
    /// </summary>
    public void CentreStickHit()
    {
        _centreHitThisThrow = false;
        _pointsThisThrow += 500;
    }

    /// <summary>
    /// Called when the Stick of Double points is hit
    /// </summary>
    public void StickOfDoublePointsHit()
    {
        _doubleFactor *= 2;
        _doubleHitThisThrow = true;
    }

    /// <summary>
    /// Shows the arrow and rotates it to allow the user to select direction
    /// </summary>
    IEnumerator SelectDirection()
    {
        SetWind_(); // TODO: Move to start of new round

        // show the arrow
        Arrow.eulerAngles = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(0.1f);  // ensure the ball has reset before showing arrow
        Arrow.gameObject.SetActive(true);

        _selectingDirection = true;
        float eulerAngles = 0;

        // loop until confirm selected
        while (_selectingDirection)
        {
            // swing one direction
            for (var i = eulerAngles; i < SWING_AMOUNT && _selectingDirection; i++)
            {
                Arrow.eulerAngles += new Vector3(0, 0, SWING_SPEED);
                yield return new WaitForSeconds(SWING_DELAY);
            }

            // swing back
            eulerAngles = SWING_AMOUNT;
            for (var i = eulerAngles; i > -SWING_AMOUNT && _selectingDirection; i--)
            {
                Arrow.eulerAngles -= new Vector3(0, 0, SWING_SPEED);
                yield return new WaitForSeconds(SWING_DELAY);
            }

            // back to start
            eulerAngles = -SWING_AMOUNT;
        }

        // start selecting distance
        StartCoroutine(SelectDistance());
    }

    /// <summary>
    /// Sets the wind direction
    /// </summary>
    private void SetWind_()
    {
        float xWind = UnityEngine.Random.Range(-400f, 400f) / 1000f;
        float yWind = 0;

        _windDirection = new Vector2(xWind, yWind);
        Debug.Log("Wind set as " + _windDirection.x + ", " + _windDirection.y);

        var size = Math.Abs(xWind);
        var flip = xWind > 0;

        WindImage.rectTransform.eulerAngles = flip ? new Vector3(0, 0, 180) : Vector3.zero;
        WindImage.rectTransform.localScale = new Vector3(size, 1, 1);
    }

    /// <summary>
    /// Moves the arrow up and down to allow the user to select power
    /// </summary>
    private IEnumerator SelectDistance()
    {
        _selectingDistance = true;

        // loop until confirm selected
        while (_selectingDistance)
        {
            // decrease size
            for (var i = 1f; i > 0.1f && _selectingDistance; i -= 0.01f)
            {
                Arrow.localScale = new Vector3(1, i, 1);
                yield return new WaitForSeconds(SWING_DELAY);
            }

            // increase size
            for (var i = 0.1f; i < 1 && _selectingDistance; i += 0.01f)
            {
                Arrow.localScale = new Vector3(1, i, 1);
                yield return new WaitForSeconds(SWING_DELAY);
            }
        }

        // when done, hide arrow and 
        Arrow.gameObject.SetActive(false);
        StartCoroutine(Fire_());
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Move to proper inputs
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmPressed_();
        }
        if (Input.GetKeyDown(KeyCode.P) && _selectingDirection)
        {
            _overarm = !_overarm;
            ThrowStyleImage.sprite = ThrowStyleSprites[_overarm ? 1 : 0];
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft_();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight_();
        }

        // follow the ball
        Camera.main.transform.localPosition = Ball.transform.localPosition - new Vector3(CAMERA_OFFSET, 0, 5);
    }

    /// <summary>
    /// When the confirm button is pressed
    /// </summary>
    private void ConfirmPressed_()
    {
        // clear status variables
        _selectingDirection = false;
        _selectingDistance = false;
    }

    /// <summary>
    /// When the user chooses to move left
    /// </summary>
    private void MoveLeft_()
    {
        // only allow moving when choosing direction
        if (_selectingDirection && Ball.transform.localPosition.x > -MOVE_FREEDOM)
            Ball.transform.Translate(new Vector3(-0.1f, 0, 0));
    }

    /// <summary>
    /// When the user chooses to move right
    /// </summary>
    private void MoveRight_()
    {
        // only allow moving when choosing direction
        if (_selectingDirection && Ball.transform.localPosition.x < MOVE_FREEDOM)
            Ball.transform.Translate(new Vector3(0.1f, 0, 0));
    }

    /// <summary>
    /// Fires the ball
    /// </summary>
    private IEnumerator Fire_()
    {
        // rotate the ball
        Ball.transform.eulerAngles = Arrow.eulerAngles;

        // wait for the rotation to apply - required, annoyingly
        yield return new WaitForSeconds(0.01f);

        // throw the ball
        Ball.AddRelativeForce(Vector2.up * FIRE_FORCE * Arrow.localScale.y);

        // delay before setting the ball as active, so the velocity check does not apply immediately
        yield return new WaitForSeconds(0.1f);
        BallScript.Started(_overarm, Arrow.localScale.y, _windDirection);
    }

    /// <summary>
    /// When the ball enters a scoring zone
    /// </summary>
    /// <param name="zone">The zone that was entered</param>
    public void ZoneEntered(BowlZoneScript zone)
    {
        // add this to the list of zones that the ball is in
        _activeZones.Add(zone);
    }

    /// <summary>
    /// When the ball leaves a scoring zone
    /// </summary>
    /// <param name="zone">The zone that was left</param>
    public void ZoneLeft(BowlZoneScript zone)
    {
        // remove from the active zone list
        _activeZones.Remove(zone);
    }
}
