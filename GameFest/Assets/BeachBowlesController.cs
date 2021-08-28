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
public class BeachBowlesController : GenericController
{
    // Unity items
    public Rigidbody2D Ball;
    public BowlsBallScript BallScript;
    public Transform Arrow;
    public Transform MaxPowerLine;
    public Image ThrowStyleImage;
    public Sprite[] ThrowStyleSprites;
    public Image WindImage;
    public Image ActivePlayerColour;
    public Text TxtActivePlayerName;
    public Text TxtShotCountDown;
    public Vector3 CameraZoomPosition;
    public TransitionFader EndFader;            // Fader for the end of game
    public SwooshController SwooshControls;

    // score controls
    public BeachScoreDisplayScript[] RoundControls;
    public BeachScoreDisplayScript RoundTotalScore;
    public OverallBeachDisplayScoreScript[] PlayerScores;

    // status variables
    bool _selectingDirection = false;
    bool _selectingDistance = false;
    bool _overarm = false;
    Vector3 _ballLocation = Vector3.zero;
    Vector3 _cameraLocation = Vector3.zero;
    List<BowlZoneScript> _activeZones = new List<BowlZoneScript>();
    int _activePlayerIndex = 0;
    Vector2 _windDirection = new Vector2(0, 0);
    int _pointsThisThrow = 0;
    int _doubleFactor = 1;
    bool _doubleHitThisThrow = false;
    bool _centreHitThisThrow = false;
    List<BeachBowlesInputHandler> _players = new List<BeachBowlesInputHandler>();
    bool _cameraPreview;
    TimeLimit _playerLimit;
    bool _cancelled = false;

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
    const float CAMERA_MOVE_SPEED = 75f;

    private const int NUMBER_OF_ROUNDS = 3;

    // static instance that can be accessed from other scripts
    public static BeachBowlesController Instance;

    // Start is called before the first frame update
    void Start()
    {
        // store static instance
        Instance = this;

        SpawnPlayers_();

        SetWind_(0, 100);

        // fade in
        EndFader.StartFade(1, 0, FadeInComplete);
        _playerLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _playerLimit.Initialise(30, DisplayCountdown, PlayerTimeoutCallback);

        // get the location of the ball at the start
        _ballLocation = Ball.transform.localPosition;
        _cameraLocation = Camera.main.transform.localPosition;

        InitialiseScoreDisplays_();
    }

    private void DisplayCountdown(int time)
    {
        TxtShotCountDown.text = TextFormatter.GetTimeString(time);
    }

    private void PlayerTimeoutCallback()
    {
        _cancelled = true;
        _selectingDirection = false;
        _selectingDistance = false;
        StopAllCoroutines();

        var startIndex = _roundIndex * 3;
        for (int i = 0; i < 3; i++)
        {
            PlayerScores[_activePlayerIndex].IndividualBreakDowns[startIndex + i].Cancelled();
            RoundControls[i].Cancelled();
        }
        _throwIndex = 2;
        StartCoroutine(TimeOutReset_());
    }

    IEnumerator TimeOutReset_()
    {
        // TODO: Show message
        yield return new WaitForSeconds(2);
        NextShot_();
    }

    void FadeInComplete()
    {
        PauseGameHandler.Instance.Pause(true, StartGame);
    }

    void StartGame()
    {
        DisplayActivePlayer_();
        // move everything to starting position
        ResetPositions_();
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private List<Transform> SpawnPlayers_()
    {
        var playerTransforms = new List<Transform>();

        // loop through all players
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(BeachBowlesInputHandler));
            // create the "visual" player at the start point
            player.Spawn(null, Vector3.zero);
            _players.Add(player.GetComponent<BeachBowlesInputHandler>());
        }

        return playerTransforms;
    }

    /// <summary>
    /// Sets up the score displays with the correct cololur and names, and hides the unused ones
    /// </summary>
    private void InitialiseScoreDisplays_()
    {
        // initialise each of the players displays
        for (int i = 0; i < _players.Count(); i++)
        {
            PlayerScores[i].gameObject.SetActive(true);
            PlayerScores[i].SetColour(_players[i].GetPlayerName(), _players[i].GetPlayerIndex());
        }

        // remove unused items
        for (int i = _players.Count(); i < PlayerScores.Count(); i++)
        {
            PlayerScores[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called from the ball script when the ball stops moving
    /// </summary>
    internal void BallStopped()
    {
        var points  = _activeZones.Count > 0 ? _activeZones.LastOrDefault().PointValue : 0;
        if (points < 0 && (_centreHitThisThrow || _doubleHitThisThrow)) points = 0;
        _pointsThisThrow += points;
        StartCoroutine(ShowPoints_());
    }

    /// <summary>
    /// Checks if the ball went out of bounds, and behaves accordingly
    /// </summary>
    void CheckOutOfBounds_(int index)
    {
        // -9999 is used for zones outwith playing area
        if (_pointsThisThrow == -9999 && !_doubleHitThisThrow && !_centreHitThisThrow)
        {
            _doubleFactor = 1;
            _pointsThisThrow = 0;

            RoundControls[_throwIndex].OutOfBounds();
            PlayerScores[_activePlayerIndex].IndividualBreakDowns[index].OutOfBounds();

            for (int i = 0; i < _throwIndex; i++)
            {
                RoundControls[i].Cancelled();
            }

            for (int i = index - _throwIndex; i < index; i++)
            {
                PlayerScores[_activePlayerIndex].IndividualBreakDowns[i].Cancelled();
            }
        }
    }

    /// <summary>
    /// Displays the total score for the round
    /// </summary>
    void UpdateRoundTotalScore_()
    {
        var roundScore = 0;
        foreach (var v in RoundControls)
            roundScore += v.GetValue();

        roundScore *= _doubleFactor;

        RoundTotalScore.SetValue(roundScore);
        PlayerScores[_activePlayerIndex].RoundTotalScores[_roundIndex].SetValue(roundScore);
    }

    /// <summary>
    /// Shows the points 
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowPoints_()
    {
        yield return new WaitForSeconds(1);

        var index = _roundIndex * 3 + _throwIndex;

        PlayerScores[_activePlayerIndex].IndividualBreakDowns[index].SetValue(_pointsThisThrow);
        RoundControls[_throwIndex].SetValue(_pointsThisThrow);
        CheckOutOfBounds_(index);

        if (_doubleHitThisThrow)
        {
            RoundControls[_throwIndex].Double();
            PlayerScores[_activePlayerIndex].IndividualBreakDowns[index].Double();
        }

        UpdateRoundTotalScore_();
        UpdateTotalScores_();

        yield return new WaitForSeconds(2);

        // move everything back to their original locations
        NextShot_();
    }

    private void NextShot_()
    {
        _cancelled = false;

        ResetPositions_();

        _throwIndex++;

        if (_throwIndex > 2)
            NextPlayer_();
    }

    /// <summary>
    /// Displays scores for each player
    /// </summary>
    private void UpdateTotalScores_()
    {
        foreach (var player in PlayerScores)
        {
            var score = 0;
            foreach (var control in player.RoundTotalScores)
            {
                score += control.GetValue();
            }
            player.TxtTotalScore.text = score.ToString();
        }
    }

    /// <summary>
    /// Move to the next player/round
    /// </summary>
    private void NextPlayer_()
    {
        _throwIndex = 0;

        SwooshControls.DoSwoosh(Camera.main, Camera.main, null);

        _activePlayerIndex++;

        if (_activePlayerIndex == _players.Count)
        {
            _activePlayerIndex = 0;
            _roundIndex++;

            SetWind_(_roundIndex * 140, _roundIndex * 250);

            if (_roundIndex >= NUMBER_OF_ROUNDS)
            {
                Complete();
            }
        }

        foreach (var score in RoundControls)
            score.ResetControl();

        RoundTotalScore.ResetControl();
        _doubleFactor = 1;

        DisplayActivePlayer_();
    }

    private void Complete()
    {
        for (int i = 0; i < PlayerScores.Length && i < _players.Count; i++)
        {
            var score = 0;
            foreach (var control in PlayerScores[i].RoundTotalScores)
            {
                score += control.GetValue();
            }

            _players[i].AddPoints(score);
        }

        var ordered = _players.OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 200, 80, 20 };

        for (int i = 0; i < ordered.Count(); i++)
        {
            ordered[i].AddPoints(winnerPoints[i]);
        }

        // TODO: Add points for winners
        EndFader.StartFade(0, 1, EndGame_);
    }

    /// <summary>
    /// Displays the name and colour of active player
    /// </summary>
    private void DisplayActivePlayer_()
    {
        var colour = ColourFetcher.GetColour(_players[_activePlayerIndex].GetPlayerIndex());
        TxtActivePlayerName.text = _players[_activePlayerIndex].GetPlayerName();
        ActivePlayerColour.color = colour;
        Arrow.GetComponentInChildren<SpriteRenderer>().color = colour;

        // display player active colour
        for (int i = 0; i < PlayerScores.Count(); i++)
        {
            PlayerScores[i].ImgActiveOverlay.SetActive(i == _activePlayerIndex);
        }
    }

    /// <summary>
    /// Ends the game and returns to the menu
    /// </summary>
    private void EndGame_()
    {
        PlayerManagerScript.Instance.NextScene(Scene.GameCentral);
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

        _playerLimit.Restart();
        _playerLimit.StartTimer();

        // start moving arrow again
        StartCoroutine(SelectDirection());
    }

    /// <summary>
    /// Called when the centre stick is hit by the ball
    /// </summary>
    public void CentreStickHit()
    {
        if (_centreHitThisThrow) return;

        _centreHitThisThrow = true;
        _pointsThisThrow += 80;
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
        if (!_cancelled)
            StartCoroutine(SelectDistance());
    }

    /// <summary>
    /// Sets the wind direction
    /// </summary>
    private void SetWind_(int min, int max)
    {
        var value = UnityEngine.Random.Range(min, max);
        var flip = UnityEngine.Random.Range(0, 2) == 1;

        float xWind = value / 1000f;
        if (flip)
            xWind *= -1;

        _windDirection = new Vector2(xWind, 0);

        var size = Math.Abs(xWind);

        WindImage.rectTransform.eulerAngles = xWind > 0 ? new Vector3(0, 0, 180) : Vector3.zero;
        WindImage.rectTransform.localScale = new Vector3(size, 1, 1);
    }

    /// <summary>
    /// Moves the arrow up and down to allow the user to select power
    /// </summary>
    private IEnumerator SelectDistance()
    {
        _selectingDistance = true;

        Arrow.localScale = new Vector3(1, 0.5f, 1);

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

        if (!_cancelled)
            StartCoroutine(Fire_());
    }

    // Update is called once per frame
    void Update()
    {
        if (!_cameraPreview)
        {
            // follow the ball
            Camera.main.transform.localPosition = Ball.transform.localPosition - new Vector3(CAMERA_OFFSET, 0, 5);
        }
    }

    public void TrianglePressed(int playerIndex)
    {
        if (_selectingDirection && playerIndex == _activePlayerIndex)
        {
            _overarm = !_overarm;
            ThrowStyleImage.sprite = ThrowStyleSprites[_overarm ? 1 : 0];

            var yPos = _overarm ? 125.0822f : 96.4267f;
            MaxPowerLine.transform.localPosition = new Vector3(MaxPowerLine.transform.localPosition.x, yPos, 0);
        }
    }

    /// <summary>
    /// When the confirm button is pressed
    /// </summary>
    public void ConfirmPressed(int index)
    {
        if (index == _activePlayerIndex && !_cameraPreview)
        {
            // clear status variables
            _selectingDirection = false;
            _selectingDistance = false;
        }
    }

    /// <summary>
    /// When the user chooses to move left
    /// </summary>
    public void MoveLeft(int index)
    {
        // only allow moving when choosing direction
        if (_selectingDirection && Ball.transform.localPosition.x > -MOVE_FREEDOM && index == _activePlayerIndex)
            Ball.transform.Translate(new Vector3(-0.1f, 0, 0));
    }

    /// <summary>
    /// When the user chooses to move right
    /// </summary>
    public void MoveRight(int index)
    {
        // only allow moving when choosing direction
        if (_selectingDirection && Ball.transform.localPosition.x < MOVE_FREEDOM && index == _activePlayerIndex)
            Ball.transform.Translate(new Vector3(0.1f, 0, 0));
    }

    /// <summary>
    /// Fires the ball
    /// </summary>
    private IEnumerator Fire_()
    {
        _playerLimit.Abort();

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

    public void CameraPreview(int index)
    {
        if (index == _activePlayerIndex && !_cameraPreview)
        {
            StartCoroutine(CameraPreview_());
        }
    }

    private IEnumerator CameraPreview_()
    {
        MaxPowerLine.gameObject.SetActive(true);
        _cameraPreview = true;
        while (Camera.main.transform.localPosition.y < CameraZoomPosition.y)
        {
            Camera.main.transform.Translate(new Vector3(0, CAMERA_MOVE_SPEED * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        // slowly go up
        for (int i = 0; i < (_overarm ? 380 : 150); i++)
        {
            Camera.main.transform.Translate(new Vector3(0, 10 * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        while (Camera.main.transform.localPosition.y > _cameraLocation.y)
        {
            Camera.main.transform.Translate(new Vector3(0, -CAMERA_MOVE_SPEED * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }
        Camera.main.transform.localPosition = _cameraLocation;
        _cameraPreview = false;
        MaxPowerLine.gameObject.SetActive(false);
    }
}
