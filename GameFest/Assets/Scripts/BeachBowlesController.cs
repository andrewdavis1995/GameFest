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
    public TransitionFader EndFader;
    public SwooshController SwooshControls;
    public Animator PlayerAnimator;
    public Animator PlayerShadowAnimator;
    public Animator[] PlayerBGAnimator;
    public Animator[] PlayerBGShadowAnimator;
    public RuntimeAnimatorController[] PlayerAnimatorControllers;
    public GameObject PlayerUi;
    public Text PlayerUiBehindLeader;
    public Text PlayerUiPlayerName;
    public Image PlayerUiPlayerColour;
    public GameObject PlayerCam;
    public ResultsPageScreen ResultsScreen;

    // score controls
    public BeachScoreDisplayScript[] RoundControls;
    public BeachScoreDisplayScript RoundTotalScore;
    public OverallBeachDisplayScoreScript[] PlayerScores;

    // cameras
    public Camera GameplayCamera;
    public Camera UmpireCamera;
    public Camera PlayerCamera;
    private float _cameraZoom;

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
    float _cameraOffset;
    bool _showingCharacter = false;

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

    const int NUMBER_OF_ROUNDS = 3;
    
    const int OUT_OF_BOUNDS_SCORE = -9999;

    // static instance that can be accessed from other scripts
    public static BeachBowlesController Instance;

    // Start is called before the first frame update
    void Start()
    {
        // store static instance
        Instance = this;

        _cameraZoom = GameplayCamera.orthographicSize;

        // Initialise the game
        SpawnPlayers_();
        SetWind_(0, 100);

        InitialisePlayerPreviews_();

        // fade in
        EndFader.StartFade(1, 0, FadeInComplete);
        _playerLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _playerLimit.Initialise(30, DisplayCountdown, PlayerTimeoutCallback);

        // get the location of the ball at the start
        _ballLocation = Ball.transform.localPosition;
        _cameraLocation = GameplayCamera.transform.localPosition;

        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        PauseGameHandler.Instance.Initialise(genericPlayers);

        InitialiseScoreDisplays_();
    }

    /// <summary>
    /// Stes up the players in the player preview page
    /// </summary>
    private void InitialisePlayerPreviews_()
    {
        SetAnimators_("Idle");
        for (int i = _players.Count - 1; i < PlayerBGAnimator.Length; i++)
        {
            PlayerBGAnimator[i].gameObject.SetActive(false);
            PlayerBGShadowAnimator[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sets the specified animator to the specified state
    /// </summary>
    /// <param name="animator">The animator to update</param>
    /// <param name="state">The trigger to set on the animator</param>
    private void SetAnimator_(Animator animator, string state)
    {
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Celebrate");
        animator.SetTrigger(state);
    }

    /// <summary>
    /// Sets all animators for the player preview to specified state
    /// </summary>
    /// <param name="state">The trigger to set on the animators</param>
    private void SetAnimators_(string state)
    {
        SetAnimator(PlayerAnimator, state);
        SetAnimator(PlayerShadowAnimator, state);

        // set background players display animator
        foreach (var anim in PlayerBGAnimator)
        {
            SetAnimator(anim, state);
        }

        // set background player shadow display animator
        foreach (var anim in PlayerBGShadowAnimator)
        {
            SetAnimator(anim, state);
        }
    }


    /// <summary>
    /// Displays the countdown clock for time remaining to take the shot
    /// </summary>
    /// <param name="time">Remaining time</param>
    private void DisplayCountdown(int time)
    {
        TxtShotCountDown.text = TextFormatter.GetTimeString(time);
    }

    /// <summary>
    /// Called when the player runs out of time to take their shot
    /// </summary>
    private void PlayerTimeoutCallback()
    {
        // reset the state
        _cancelled = true;
        _selectingDirection = false;
        _selectingDistance = false;

        // stop the movement of the arrow
        StopAllCoroutines();

        // initialise the score displays
        var startIndex = _roundIndex * 3;
        for (int i = 0; i < 3; i++)
        {
            PlayerScores[_activePlayerIndex].IndividualBreakDowns[startIndex + i].Cancelled();
            RoundControls[i].Cancelled();
        }

        // move to the next player
        _throwIndex = 2;
        StartCoroutine(TimeOutReset_());
    }

    /// <summary>
    /// Handle a player timing out
    /// </summary>
    /// <returns></returns>
    IEnumerator TimeOutReset_()
    {
        // TODO: Show message
        yield return new WaitForSeconds(2);
        NextShot_();
    }

    /// <summary>
    /// Callback for when the fade image has fully faded out
    /// </summary>
    void FadeInComplete()
    {
        PauseGameHandler.Instance.Pause(true, StartGame);
    }

    /// <summary>
    /// Start the game
    /// </summary>
    void StartGame()
    {
        DisplayActivePlayer_();
        ResetPositions_();

        PlayerUi.SetActive(true);
        PlayerUiPlayerColour.color = ColourFetcher.GetColour(_activePlayerIndex);
        PlayerUiPlayerName.text = _players[_activePlayerIndex].GetPlayerName();
        PlayerUiBehindLeader.text = "First shot";

        PlayerCamera.enabled = true;
        GameplayCamera.enabled = false;
        _showingCharacter = true;
    }

    /// <summary>
    /// Start zooming in on the vall
    /// </summary>
    internal void ZoomOnBall()
    {
        StartCoroutine(ZoomOnBall_());
    }

    /// <summary>
    /// Zoomes in on the ball as it slows
    /// </summary>
    /// <returns></returns>
    private IEnumerator ZoomOnBall_()
    {
        for (int i = 0; i < 60; i++)
        {
            _cameraOffset += 0.07f;
            GameplayCamera.orthographicSize -= 0.14f;
            yield return new WaitForSeconds(0.02f);
        }
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
        var points = _activeZones.Count > 0 ? _activeZones.LastOrDefault().PointValue : 0;

        // don't lose points if the ball hit either stick
        if (points < 0 && (_centreHitThisThrow || _doubleHitThisThrow)) points = 0;

        // add points and display them
        _pointsThisThrow += points;
        StartCoroutine(ShowPoints_());
    }

    /// <summary>
    /// Checks if the ball went out of bounds, and behaves accordingly
    /// </summary>
    void CheckOutOfBounds_(int index)
    {
        // -OUT_OF_BOUNDS_SCORE is used for zones outwith playing area
        if (_pointsThisThrow == OUT_OF_BOUNDS_SCORE && !_doubleHitThisThrow && !_centreHitThisThrow)
        {
            _doubleFactor = 1;
            _pointsThisThrow = 0;

            // display that the ball went out of bounds
            RoundControls[_throwIndex].OutOfBounds();
            PlayerScores[_activePlayerIndex].IndividualBreakDowns[index].OutOfBounds();

            // Remove points from previous throws in this round
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
        // get the value for all scores
        var roundScore = 0;
        foreach (var v in RoundControls)
            roundScore += v.GetValue();

        // double the score if appropriate
        roundScore *= _doubleFactor;

        // display the value
        RoundTotalScore.SetValue(roundScore);
        PlayerScores[_activePlayerIndex].RoundTotalScores[_roundIndex].SetValue(roundScore);
    }

    /// <summary>
    /// Shows the points 
    /// </summary>
    IEnumerator ShowPoints_()
    {
        yield return new WaitForSeconds(1);

        var index = _roundIndex * 3 + _throwIndex;

        // display the value in the UI
        PlayerScores[_activePlayerIndex].IndividualBreakDowns[index].SetValue(_pointsThisThrow);
        RoundControls[_throwIndex].SetValue(_pointsThisThrow);

        // check if the ball went out of bounds
        CheckOutOfBounds_(index);

        // display the double icon
        if (_doubleHitThisThrow)
        {
            RoundControls[_throwIndex].Double();
            PlayerScores[_activePlayerIndex].IndividualBreakDowns[index].Double();
        }

        // update score dislays
        UpdateRoundTotalScore_();
        UpdateTotalScores_();

        yield return new WaitForSeconds(2);

        // move everything back to their original locations
        NextShot_();
    }

    /// <summary>
    /// Move to the next shot
    /// </summary>
    private void NextShot_()
    {
        SwooshControls.DoSwoosh(null, SetupNextShot_);
    }

    void SetupNextShot_()
    {
        _cancelled = false;

        PlayerCam.SetActive(false);
        SetAnimators_("Idle");

        ResetPositions_();
        _throwIndex++;

        // if that was the third shot, move to the next player
        if (_throwIndex > 2)
            NextPlayer_();
        else
            _playerLimit.StartTimer();
    }

    /// <summary>
    /// Displays scores for each player
    /// </summary>
    private void UpdateTotalScores_()
    {
        // loop through all players
        foreach (var player in PlayerScores)
        {
            // add up all points
            var score = 0;
            foreach (var control in player.RoundTotalScores)
            {
                score += control.GetValue();
            }

            // display points
            player.TxtTotalScore.text = score.ToString();
        }
    }

    /// <summary>
    /// Move to the next player/round
    /// </summary>
    private void NextPlayer_()
    {
        _showingCharacter = true;
        PlayerCamera.enabled = true;
        GameplayCamera.enabled = false;

        _throwIndex = 0;

        _activePlayerIndex++;

        // that was the last player, so we move to the new round
        if (_activePlayerIndex == _players.Count)
        {
            _activePlayerIndex = 0;
            _roundIndex++;

            // change the wind
            SetWind_(_roundIndex * 140, _roundIndex * 250);

            // end game when no more rounds
            if (_roundIndex >= NUMBER_OF_ROUNDS)
            {
                Complete();
            }
        }

        // reset the score displays for the current round
        foreach (var score in RoundControls)
            score.ResetControl();

        RoundTotalScore.ResetControl();
        _doubleFactor = 1;

        // display the new active player
        DisplayActivePlayer_();

        StartCoroutine(ShowPlayerUi_());
    }

    /// <summary>
    /// Shows the UI on the player preview
    /// </summary>
    private IEnumerator ShowPlayerUi_()
    {
        // wait for the swoosh to finish
        yield return new WaitForSeconds(0.9f);

        var activeScore = 0;
        var highScore = 0;
        var index = 0;

        // loop through all players
        foreach (var player in PlayerScores)
        {
            // add up all points
            var score = 0;
            foreach (var control in player.RoundTotalScores)
            {
                score += control.GetValue();
            }

            // display points
            player.TxtTotalScore.text = score.ToString();

            // if this is the current player, store their score
            if (index == _activePlayerIndex)
            {
                activeScore = score;
            }

            // keep a record of the highest score
            if (score > highScore)
            {
                highScore = score;
            }

            index++;
        }

        // display player info
        var leaderText = (highScore - activeScore) <= 0 ? "Current Leader" : (highScore - activeScore) + " points behind leader";
        PlayerUi.SetActive(true);
        PlayerUiPlayerColour.color = ColourFetcher.GetColour(_activePlayerIndex);
        PlayerUiPlayerName.text = _players[_activePlayerIndex].GetPlayerName();
        PlayerUiBehindLeader.text = leaderText;
    }

    /// <summary>
    /// The game is complete
    /// </summary>
    private void Complete()
    {
        // get points for each player
        for (int i = 0; i < PlayerScores.Length && i < _players.Count; i++)
        {
            var score = 0;
            foreach (var control in PlayerScores[i].RoundTotalScores)
            {
                score += control.GetValue();
            }

            // add points for each player
            _players[i].AddPoints(score);
        }

        // sort the players by points scored
        var ordered = _players.OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 200, 80, 20 };

        // add winning score points 
        for (int i = 0; i < ordered.Count(); i++)
        {
            ordered[i].AddPoints(winnerPoints[i]);
        }

        StartCoroutine(Complete_());
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

        // display the player
        PlayerAnimator.runtimeAnimatorController = PlayerAnimatorControllers[_players[_activePlayerIndex].GetCharacterIndex()];
        PlayerShadowAnimator.runtimeAnimatorController = PlayerAnimatorControllers[_players[_activePlayerIndex].GetCharacterIndex()];

        // display other players in the background
        var currentIndex = _activePlayerIndex + 1;
        if (currentIndex >= _players.Count) currentIndex = 0;

        for (int i = 0; i < PlayerBGAnimator.Length; i++)
        {
            PlayerBGAnimator[i].runtimeAnimatorController = PlayerAnimatorControllers[_players[currentIndex].GetCharacterIndex()];
            PlayerBGShadowAnimator[i].runtimeAnimatorController = PlayerAnimatorControllers[_players[currentIndex].GetCharacterIndex()];

            currentIndex++;
            if (currentIndex >= _players.Count) currentIndex = 0;
        }

        SetAnimators_("Idle");

        // display player active colour
        for (int i = 0; i < PlayerScores.Count(); i++)
        {
            PlayerScores[i].ImgActiveOverlay.SetActive(i == _activePlayerIndex);
        }
    }

    /// <summary>
    /// Moves all items back to their starting places
    /// </summary>
    void ResetPositions_()
    {
        // clear statuses
        _overarm = false;
        UpdateThrowStyleDisplay_();

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

        // reset camera
        GameplayCamera.orthographicSize = _cameraZoom;
        _cameraOffset = CAMERA_OFFSET;

        // start moving arrow again
        StartCoroutine(SelectDirection());
    }

    /// <summary>
    /// Called when the centre stick is hit by the ball
    /// </summary>
    public void CentreStickHit()
    {
        if (_centreHitThisThrow) return;

        PlayerCelebration_();

        _centreHitThisThrow = true;
        _pointsThisThrow += 80;
    }

    private void PlayerCelebration_()
    {
        PlayerCam.SetActive(true);
        SetAnimators_("Celebrate");
    }

    /// <summary>
    /// Called when the Stick of Double points is hit
    /// </summary>
    public void StickOfDoublePointsHit()
    {
        PlayerCelebration_();

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
        // get a random value and direction for the wind
        var value = UnityEngine.Random.Range(min, max);
        var flip = UnityEngine.Random.Range(0, 2) == 1;

        float xWind = value / 1000f;
        if (flip)
            xWind *= -1;

        // set wind strength and direction
        _windDirection = new Vector2(xWind, 0);

        // display the image with suitable size
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
            GameplayCamera.transform.localPosition = Ball.transform.localPosition - new Vector3(_cameraOffset, 0, 5);
        }
    }

    /// <summary>
    /// Called when the triangle button is pressed
    /// </summary>
    /// <param name="playerIndex">The index of the player</param>
    public void TrianglePressed(int playerIndex)
    {
        // only valid if the direction is being selected, and the current player is the one taking the shot
        if (_selectingDirection && playerIndex == _activePlayerIndex)
        {
            // toggle the icon
            _overarm = !_overarm;
            UpdateThrowStyleDisplay_();
        }
    }

    /// <summary>
    /// Updates the throw style indicator/icon, and the max power line
    /// </summary>
    private void UpdateThrowStyleDisplay_()
    {
        ThrowStyleImage.sprite = ThrowStyleSprites[_overarm ? 1 : 0];

        // change the "MAX DISTANCE" indicator
        var yPos = _overarm ? 125.0822f : 95f;
        MaxPowerLine.transform.localPosition = new Vector3(MaxPowerLine.transform.localPosition.x, yPos, 0);
    }

    /// <summary>
    /// When the confirm button is pressed
    /// </summary>
    public void ConfirmPressed(int index)
    {
        if (index == _activePlayerIndex && !_cameraPreview)
        {
            if (_showingCharacter)
            {
                _showingCharacter = false;
                PlayerCamera.enabled = false;
                GameplayCamera.enabled = true;

                _playerLimit.StartTimer();

                PlayerUi.SetActive(false);
            }
            else
            {
                // clear status variables
                _selectingDirection = false;
                _selectingDistance = false;
            }
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

    /// <summary>
    /// Display the upper part of the course briefly
    /// </summary>
    /// <param name="index">The index of the player</param>
    public void CameraPreview(int index)
    {
        // only valid if the current player is active, and there is not a preview in progress
        if (index == _activePlayerIndex && !_cameraPreview)
        {
            StartCoroutine(CameraPreview_());
        }
    }

    /// <summary>
    /// Moves the camera through the course
    /// </summary>
    private IEnumerator CameraPreview_()
    {
        MaxPowerLine.gameObject.SetActive(true);
        _cameraPreview = true;

        // move up quickly
        while (GameplayCamera.transform.localPosition.y < CameraZoomPosition.y)
        {
            GameplayCamera.transform.Translate(new Vector3(0, CAMERA_MOVE_SPEED * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        // slowly go up through the top part
        for (int i = 0; i < (_overarm ? 380 : 150); i++)
        {
            GameplayCamera.transform.Translate(new Vector3(0, 10 * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        // move down quickly
        while (GameplayCamera.transform.localPosition.y > _cameraLocation.y)
        {
            GameplayCamera.transform.Translate(new Vector3(0, -CAMERA_MOVE_SPEED * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        GameplayCamera.transform.localPosition = _cameraLocation;
        _cameraPreview = false;
        MaxPowerLine.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Show the results window, and then return to menu
    /// </summary>
    private IEnumerator Complete_()
    {
        ResultsScreen.Setup();
        ResultsScreen.SetPlayers(_players);

        yield return new WaitForSeconds(4 + _players.Length);

        // fade out
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        // when no more players, move to the central page
        PlayerManagerScript.Instance.NextScene(Scene.GameCentral);
    }
    
}
