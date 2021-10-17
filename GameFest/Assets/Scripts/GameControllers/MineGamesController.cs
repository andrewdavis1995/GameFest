using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum MineSelectionState { None, GoldDestination, CoalDestination, GoldClaim };
public enum ButtonValues { Square, Triangle, Circle, Cross };

public class MineGamesController : GenericController
{
    const int NUM_ROUNDS = 2;
    const int ROUND_TIME = 15;

    public static MineGamesController Instance;

    public Transform PlayerPrefab;             // The prefab to create
    public Vector3[] StartPositions;           // Where the players should spawn
    public Vector2 PlatformPlayerPosition;     // Where the player should stand on the platform
    public Vector2 ReturnPlayerPosition;       // Where the player should stand on the platform
    public float RunOffX;                      // X Position to stop player at
    public Sprite[] PlayerIcons;               // Icons of the player
    public Collider2D RightWall;               // Right wall collider
    public float RockDropLeftBound;            // Furthest left that items can be dropped
    public float RockDropRightBound;           // Furthest right that items can be dropped
    public MineCart[] Carts;                   // The coal/gold carts
    public Transform[] Buckets;                // The buckets
    public float RockDropY;                    // Height to drop rocks from
    public Transform RockPrefab;               // Prefab of rocks to drop
    public CameraShakeScript CameraShake;      // Camera shake

    // UI
    public Text TxtActivePlayer;               // The text that displays the active player name
    public Text TxtActivePlayerCountdown;      // The text that displays the countdown for selecting zones
    public Text TxtRunaroundCountdown;         // The text that displays the countdown for running around
    public Image[] ColouredImages;             // The images that need to have their colour set to the players colour
    public Image ImgCharacterImage;            // The image that shows the character icon
    public Sprite[] ButtonImages;              // Icons for each button
    public TextMesh[] ScoreboardNames;         // Player names on scoreboard
    public TextMesh[] ScoreboardScores;        // Player scores on scoreboard
    public SpriteRenderer[] ActiveZones;       // Which zone each player is in
    public Sprite UnknownZoneSprite;           // Sprite to use when player is not in a zone
    public Text TxtCommentary;                 // Text to show current actions/help
    public Text TxtErrorMessage;               // Text to show error message
    public Image ImgCommentaryClaim;           // The (bigger) image that displays which zone the player claimed items are in
    public GameObject ResultsPopup;            // Popup to show results
    public MineResultScript[] ResultDisplays;  // Displays that show result of each round
    public Sprite[] DisabledImages;            // Sprites to use for disabled players

    public ResultsPageScreen ResultsScreen;
    public TransitionFader EndFader;

    List<MineGamesInputHandler> _players = new List<MineGamesInputHandler>();
    List<PlayerMovement> _playerMovements = new List<PlayerMovement>();
    int _activePlayerIndex = 0;
    int _previousPlayerIndex = -1;
    int _roundIndex = 0;
    MineSelectionState _selectionState = MineSelectionState.None;
    TimeLimit _zoneSelectionLimit;
    bool _timeoutOccurred = false;

    ButtonValues _goldZone;
    ButtonValues _goldClaimZone;
    ButtonValues _coalZone;

    // points
    int Correct_Points = 720;
    int Wrong_Points = 600;
    int Truth_Points = 120;

    private void Start()
    {
        Instance = this;

        SpawnPlayers_();

        SetupScoreboard_();

        // more points for more players
        Correct_Points /= _players.Count;
        Wrong_Points /= _players.Count;
        Truth_Points /= _players.Count;

        DisplayActivePlayer_();

        //initialise timer
        _zoneSelectionLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _zoneSelectionLimit.Initialise(30, ZoneSelectionCallback_, ZoneSelectionTimeoutCallback_);

        // fade in
        EndFader.GetComponentInChildren<Image>().sprite = PlayerManagerScript.Instance.GetFaderImage();
        EndFader.StartFade(1, 0, FadeInComplete);
    }


    /// <summary>
    /// Called once fully faded in
    /// </summary>
    private void FadeInComplete()
    {
        PauseGameHandler.Instance.Pause(true, StartGame_);
    }

    /// <summary>
    /// Callback for the zone selection timer - called once per second
    /// </summary>
    /// <param name="seconds">How many seconds are remaining</param>
    void ZoneSelectionCallback_(int seconds)
    {
        TxtActivePlayerCountdown.text = seconds.ToString();
    }

    /// <summary>
    /// Check the index of the active player
    /// </summary>
    /// <returns>Index of the active player</returns>
    public int ActivePlayerIndex()
    {
        return _activePlayerIndex;
    }

    /// <summary>
    /// Callback for the zone selection timer - called once timer expires
    /// </summary>
    void ZoneSelectionTimeoutCallback_()
    {
        _timeoutOccurred = true;
        var random = UnityEngine.Random.Range(0, 3);

        _goldZone = (ButtonValues)random;
        _goldClaimZone = (ButtonValues)random;
        _coalZone = (ButtonValues)(3 - random);
        TxtErrorMessage.text = "";

        _selectionState = MineSelectionState.None;

        // add items to carts
        Carts[random].SetContents(MineItemDrop.Gold);
        Carts[3 - random].SetContents(MineItemDrop.Coal);

        // show carts and enable movement
        StartCoroutine(MoveCartsOn());
        StartCoroutine(Runaround_());
    }

    /// <summary>
    /// Starts the game functionality
    /// </summary>
    void StartGame_()
    {
        PlatformSetup();
    }

    /// <summary>
    /// Initialises the scoreboard with player info
    /// </summary>
    private void SetupScoreboard_()
    {
        var index = 0;
        for (; index < _players.Count; index++)
        {
            ScoreboardNames[index].text = _players[index].GetPlayerName();
            ScoreboardScores[index].text = "0";
        }

        // hide unused elements
        for (; index < ScoreboardNames.Length; index++)
        {
            ScoreboardNames[index].gameObject.SetActive(false);
            ScoreboardScores[index].gameObject.SetActive(false);
            ActiveZones[index].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Make a player start running to the platform
    /// </summary>
    private void PlatformSetup()
    {
        foreach (var cart in Carts)
            cart.SetContents(MineItemDrop.None);

        TxtCommentary.text = _players[_activePlayerIndex].GetPlayerName() + " to the platform please!";

        // move carts off
        foreach (var cart in Carts)
        {
            cart.MoveOut();
        }

        StartCoroutine(CameraShake_());

        // run off the page
        _players[_activePlayerIndex].RunOff(RunOffX, RunOffCallback);

        // make previous player run back to ground floor
        if (_previousPlayerIndex >= 0)
        {
            _players[_previousPlayerIndex].RunOff(RunOffX, ReturnCallback);
        }
    }

    /// <summary>
    /// Make the camera shake
    /// </summary>
    private IEnumerator CameraShake_()
    {
        CameraShake.Enable();
        yield return new WaitForSeconds(3.5f);
        CameraShake.Disable();
    }

    /// <summary>
    /// Sets the display icon on the scoreboard for specified player
    /// </summary>
    /// <param name="playerIndex">The player that needs updated</param>
    /// <param name="imageIndex">The index of the icon to use</param>
    public void SetActiveIcon(int playerIndex, int imageIndex)
    {
        if (imageIndex > -1)
            ActiveZones[playerIndex].sprite = ButtonImages[imageIndex];
        else
            ActiveZones[playerIndex].sprite = UnknownZoneSprite;
    }

    /// <summary>
    /// Callback for when the player reaches the edge of the screen position
    /// </summary>
    void ReturnCallback()
    {
        _players[_previousPlayerIndex].RunOn(ReturnPlayerPosition, null);
    }

    /// <summary>
    /// Callback for when the player reaches the edge of the screen position
    /// </summary>
    void RunOffCallback()
    {
        _players[_activePlayerIndex].RunOn(PlatformPlayerPosition, RunOnCallback);
    }

    /// <summary>
    /// Callback for when the player reaches the platform position
    /// </summary>
    void RunOnCallback()
    {
        foreach (var cart in Carts)
            cart.SetContents(MineItemDrop.None);

        // enable players except the one that needs to run off
        foreach (var p in _players)
            if (p.GetPlayerIndex() != _activePlayerIndex)
                p.CanMove(true);

        _selectionState = MineSelectionState.GoldDestination;
        // start a timeout
        _zoneSelectionLimit.StartTimer();
        TxtCommentary.text = _players[_activePlayerIndex].GetPlayerName() + ":\nIn which cart would you like to place the GOLD?";
    }

    #region Callbacks for when destinations are selected
    /// <summary>
    /// A player has selected an input
    /// </summary>
    /// <param name="playerIndex">The player that selected the input</param>
    /// <param name="selection">The item that was selected</param>
    public void OptionSelected(int playerIndex, ButtonValues selection)
    {
        // if not the active player, ignore
        if (playerIndex == _activePlayerIndex)
        {
            // do the correct action based on the selection state
            switch (_selectionState)
            {
                case MineSelectionState.GoldDestination: GoldDestinationSelected_(selection); break;
                case MineSelectionState.CoalDestination: CoalDestinationSelected_(selection); break;
                case MineSelectionState.GoldClaim: GoldClaimSelected_(selection); break;
            }
        }
    }

    /// <summary>
    /// Called when the player selects destination for gold
    /// </summary>
    void GoldDestinationSelected_(ButtonValues selection)
    {
        _goldZone = selection;
        _selectionState = MineSelectionState.CoalDestination;
        Carts[(int)selection].SetContents(MineItemDrop.Gold);
        TxtCommentary.text = _players[_activePlayerIndex].GetPlayerName() + ":\nWhere would you like to place the COAL?";
    }

    /// <summary>
    /// Called when the player selects destination for coal
    /// </summary>
    void CoalDestinationSelected_(ButtonValues selection)
    {
        // can't put coal and gold in the same zone
        if (_goldZone != selection)
        {
            TxtErrorMessage.text = "";
            _coalZone = selection;
            _selectionState = MineSelectionState.GoldClaim;
            Carts[(int)selection].SetContents(MineItemDrop.Coal);

            TxtCommentary.text = _players[_activePlayerIndex].GetPlayerName() + ":\nTell the other players where you put the gold.\nYou get " + Truth_Points + " for telling the truth,\nor " + Wrong_Points + " for each player who picks\nthe cart that contains coal";
        }
        else
        {
            TxtErrorMessage.text = "Gold and coal cannot go into same cart";
        }
    }

    /// <summary>
    /// Called when the player selects destination for gold
    /// </summary>
    void GoldClaimSelected_(ButtonValues selection)
    {
        // stop the time limit
        _zoneSelectionLimit.Abort();

        // store the selection
        _goldClaimZone = selection;
        _selectionState = MineSelectionState.None;

        // show carts and enable movement
        StartCoroutine(MoveCartsOn());
        StartCoroutine(Runaround_());
    }

    /// <summary>
    /// Moves the carts on, with a brief delay for each
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveCartsOn()
    {
        StartCoroutine(RockFall_());

        StartCoroutine(CameraShake_());

        // move carts on
        foreach (var cart in Carts.Reverse())
        {
            cart.MoveIn();
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Make rocks fall from the roof
    /// </summary>
    private IEnumerator RockFall_()
    {
        // drop 40 rocks
        for (var i = 0; i < 40; i++)
        {
            var xPosition = UnityEngine.Random.Range(RockDropLeftBound, RockDropRightBound);
            var item = Instantiate(RockPrefab, new Vector3(xPosition, RockDropY, -18), Quaternion.identity);

            // disable collisions with carts
            foreach (var bucket in Buckets)
            {
                var colliders = bucket.GetComponentsInChildren<Collider2D>();
                foreach (var collider in colliders)
                    Physics2D.IgnoreCollision(item.GetComponent<Collider2D>(), collider);
            }
            yield return new WaitForSeconds(0.15f);
        }
    }

    #endregion

    /// <summary>
    /// Let the players run around and select the correct zone
    /// </summary>
    private IEnumerator Runaround_()
    {
        TxtActivePlayerCountdown.text = "";

        // allow the carts to move on
        yield return new WaitForSeconds(1.2f);

        // display instructional message
        if (!_timeoutOccurred)
            TxtCommentary.text = _players[_activePlayerIndex].GetPlayerName() + " claims that the gold is in:";
        else
            TxtCommentary.text = _players[_activePlayerIndex].GetPlayerName() + " timed out. The gold is in:";

        ImgCommentaryClaim.gameObject.SetActive(true);
        ImgCommentaryClaim.sprite = ButtonImages[(int)_goldClaimZone];

        yield return new WaitForSeconds(2f);

        if (!_timeoutOccurred)
            TxtCommentary.text = "But are they telling the truth?";
        else
            TxtCommentary.text = "This should be easy!";

        yield return new WaitForSeconds(2f);

        TxtCommentary.text = "GO!";

        // delay for players to go to correct zone
        for (int i = ROUND_TIME; i > 0; i--)
        {
            // display the countdown
            TxtRunaroundCountdown.text = i.ToString();

            // update message after 3 seconds
            if (i == (ROUND_TIME - 3))
            {
                TxtCommentary.text = "Stand under the cart where you think\n" + _players[_activePlayerIndex].GetPlayerName() + " has placed the gold";
            }

            yield return new WaitForSeconds(1);
        }

        TxtRunaroundCountdown.text = "";

        ImgCommentaryClaim.gameObject.SetActive(false);

        // display new messages
        TxtCommentary.text = "Time's up!";

        // disable players
        foreach (var p in _players)
            p.CanMove(false);

        yield return new WaitForSeconds(2f);

        TxtCommentary.text = "Let's see who gets points,\nand who is stuck with coal...";

        yield return new WaitForSeconds(2f);

        // tip carts to reveal contents
        foreach (var cart in Carts)
        {
            cart.TipCart();
        }

        // wait for carts to tip
        yield return new WaitForSeconds(3f);

        StartCoroutine(RoundResults_());
    }

    /// <summary>
    /// Shows the results for the round
    /// </summary>
    private IEnumerator RoundResults_()
    {
        // clear existing results
        foreach (var p in _players)
            p.ClearResults();

        yield return new WaitForSeconds(1f);

        List<int> celebrations = new List<int>();

        foreach (var p in _players)
        {
            if (p.GetPlayerIndex() != _activePlayerIndex)
            {
                // check answer
                if (p.ActiveZone() == (int)_goldZone)
                {
                    // player was correct
                    p.AddPoints(Correct_Points);
                    p.AddResultString(Correct_Points + "@for picking the Gold cart", Correct_Points);
                    celebrations.Add(p.GetPlayerIndex());
                }
                else if (p.ActiveZone() == (int)_coalZone)
                {
                    p.AddPoints(-1 * Wrong_Points);
                    p.AddResultString(-1 * Wrong_Points + "@for picking the Coal cart", -1 * Wrong_Points);

                    _players[_activePlayerIndex].AddPoints(Wrong_Points);
                    _players[_activePlayerIndex].AddResultString(Wrong_Points + "@for fooling players", Wrong_Points);
                    celebrations.Add(_activePlayerIndex);

                    // don't allow the score to go under 0
                    if (p.GetPoints() < 0)
                    {
                        p.AddPoints(-1 * p.GetPoints());
                    }
                }
            }
            else
            {
                // check if the player was truthful
                if (_goldClaimZone == _goldZone && !_timeoutOccurred)
                {
                    p.AddPoints(Truth_Points);
                    p.AddResultString(Truth_Points + "@Truth bonus", Truth_Points);
                }
            }
        }

        // make players celebration
        var grouped = celebrations.GroupBy(p => p);
        foreach (var c in grouped)
        {
            _playerMovements[c.Key].SetAnimationControl(false);
            _playerMovements[c.Key].Animate("Celebrate");
        }

        // wait a minute to show celebration
        yield return new WaitForSeconds(2f);

        // display scores
        foreach (var p in _players)
            ScoreboardScores[p.GetPlayerIndex()].text = p.GetPoints().ToString();

        ResultsPopup.SetActive(true);
        for (int i = 0; i < _players.Count; i++)
        {
            ResultDisplays[i].gameObject.SetActive(true);
            ResultDisplays[i].SetDisplay(_players[i]);
        }

        for (int i = _players.Count; i < ResultDisplays.Length; i++)
        {
            ResultDisplays[i].gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(5f);

        // stop celebrating
        foreach (var c in grouped)
        {
            _playerMovements[c.Key].SetAnimationControl(true);
            _playerMovements[c.Key].Animate("Idle");
        }

        ResultsPopup.SetActive(false);

        // move to next player
        NextPlayer_();
    }

    /// <summary>
    /// Move to the next player (to select drop zone)
    /// </summary>
    private void NextPlayer_()
    {
        var finished = false;
        _timeoutOccurred = false;

        // store the previous player who was on the platform (so they can be returned to the ground floor)
        _previousPlayerIndex = _activePlayerIndex;

        // increase index
        _activePlayerIndex++;

        // if we are at the end of the list, go back to first player
        if (_activePlayerIndex >= _players.Count)
        {
            // check if we are out of rounds
            finished = NextRound_();
        }

        // if not finished, start the timer
        if (!finished)
        {
            DisplayActivePlayer_();
            PlatformSetup();
        }
        else
        {
            // no more rounds, so end the game
            StartCoroutine(Complete_());
        }
    }

    /// <summary>
    /// Shows the details of the current player
    /// </summary>
    private void DisplayActivePlayer_()
    {
        // display image
        TxtActivePlayer.text = _players[_activePlayerIndex].GetPlayerName();
        ImgCharacterImage.sprite = PlayerIcons[_players[_activePlayerIndex].GetCharacterIndex()];

        // set colour of images
        foreach (var img in ColouredImages)
        {
            img.color = ColourFetcher.GetColour(_activePlayerIndex);
        }
    }

    /// <summary>
    /// Move to the next round
    /// </summary>
    /// <returns>Whether there are no more rounds</returns>
    private bool NextRound_()
    {
        _activePlayerIndex = 0;
        _roundIndex++;

        return _roundIndex >= NUM_ROUNDS;
    }

    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // sort the players by points scored
        var ordered = _players.OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 160, 60, 15 };

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
    /// Completes the game and return to object
    /// </summary>
    IEnumerator Complete_()
    {
        AssignBonusPoints_();

        yield return new WaitForSeconds(3f);

        ResultsScreen.Setup();

        GenericInputHandler[] genericPlayers = _players.ToArray<GenericInputHandler>();
        ResultsScreen.SetPlayers(genericPlayers);

        ScoreStoreHandler.StoreResults(Scene.MineGames, genericPlayers);

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
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(MineGamesInputHandler));
            _players.Add(player.GetComponent<MineGamesInputHandler>());

            // create the "visual" player at the start point
            var created = player.Spawn(PlayerPrefab, StartPositions[index++]);
            var movement = created.GetComponent<PlayerMovement>();
            movement.Shadow.gameObject.SetActive(true);
            _playerMovements.Add(movement);
        }
    }
}
