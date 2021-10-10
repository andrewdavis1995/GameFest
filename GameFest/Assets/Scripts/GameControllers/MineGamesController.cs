using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum MineSelectionState { None, GoldDestination, CoalDestination, GoldClaim };
public enum ButtonValues { Square, Triangle, Circle, Cross};

public class MineGamesController : GenericController
{
    const int NUM_ROUNDS = 2;
    const int ROUND_TIME = 7;   // TODO: Make 15

    public static MineGamesController Instance;

    public Transform PlayerPrefab;           // The prefab to create
    public Vector3[] StartPositions;         // Where the players should spawn
    public Vector2 PlatformPlayerPosition;   // Where the player should stand on the platform
    public Vector2 ReturnPlayerPosition;     // Where the player should stand on the platform
    public float RunOffX;                    // X Position to stop player at
    public Sprite[] PlayerIcons;             // Icons of the player
    public Collider2D RightWall;             // Right wall collider

    // UI
    public Text TxtActivePlayer;
    public Text TxtAction;
    public Text TxtActivePlayerCountdown;
    public Image[] ColouredImages;
    public Image ImgCharacterImage;
    public Image ImgClaimZone;
    public Sprite[] ButtonImages;

    List<MineGamesInputHandler> _players = new List<MineGamesInputHandler>();
    int _activePlayerIndex = 0;
    int _previousPlayerIndex = -1;
    int _roundIndex = 0;
    MineSelectionState _selectionState = MineSelectionState.None;

    ButtonValues _goldZone;
    ButtonValues _goldClaimZone;
    ButtonValues _coalZone;

    // points
    int Correct_Points = 24;
    int Wrong_Points = -9;
    int Truth_Points = 6;

    private void Start()
    {
        Instance = this;

        SpawnPlayers_();

        // more points for more players
        Correct_Points *= _players.Count;
        Wrong_Points *= _players.Count;
        Truth_Points *= _players.Count;

        DisplayActivePlayer_();

        StartGame_();
    }

    /// <summary>
    /// Starts the game functionality
    /// </summary>
    void StartGame_()
    {
        PlatformSetup();
    }

    /// <summary>
    /// Make a player start running to the platform
    /// </summary>
    private void PlatformSetup()
    {
        // run off the page
        _players[_activePlayerIndex].RunOff(RunOffX, RunOffCallback);

        TxtAction.text = "Waiting...";

        // make previous player run back to ground floor
        if (_previousPlayerIndex >= 0)
        {
            _players[_previousPlayerIndex].RunOff(RunOffX, ReturnCallback);
        }
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
        TxtAction.text = "Placing gold";
        _selectionState = MineSelectionState.GoldDestination;
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
        TxtAction.text = "Placing coal";
        _selectionState = MineSelectionState.CoalDestination;
    }

    /// <summary>
    /// Called when the player selects destination for coal
    /// </summary>
    void CoalDestinationSelected_(ButtonValues selection)
    {
        // can't put coal and gold in the same zone
        if (_goldZone != selection)
        {
            _coalZone = selection;
            TxtAction.text = "Making claim about gold";
            _selectionState = MineSelectionState.GoldClaim;
        }
    }

    /// <summary>
    /// Called when the player selects destination for gold
    /// </summary>
    void GoldClaimSelected_(ButtonValues selection)
    {
        _goldClaimZone = selection;
        _selectionState = MineSelectionState.None;

        ImgClaimZone.gameObject.SetActive(true);
        ImgClaimZone.sprite = ButtonImages[(int)selection];

        StartCoroutine(Runaround_());
    }

    #endregion

    /// <summary>
    /// Let the players run around and select the correct zone
    /// </summary>
    private IEnumerator Runaround_()
    {
        TxtAction.text = "Claims the gold is in:";

        // enable players except the one that needs to run off
        foreach (var p in _players)
            p.CanMove(p.GetPlayerIndex() != _activePlayerIndex);

        // brief pause while "Go!" is displayed
        Debug.Log("GO!!");  // TODO: Show in UI
        yield return new WaitForSeconds(1);

        // delay for players to go to correct zone
        yield return new WaitForSeconds(ROUND_TIME);

        Debug.Log("STOP!");
        ImgClaimZone.gameObject.SetActive(false);

        // disable players
        foreach (var p in _players)
            p.CanMove(false);

        TxtAction.text = "Viewing results";
        // TODO: show who was right etc.
        yield return new WaitForSeconds(1.5f);

        StartCoroutine(RoundResults_());
    }

    /// <summary>
    /// Shows the results for the round
    /// </summary>
    private IEnumerator RoundResults_()
    {
        yield return new WaitForSeconds(2f);

        foreach(var p in _players)
        {
            if (p.GetPlayerIndex() != _activePlayerIndex)
            {
                // check answer
                if (p.ActiveZone() == (int)_goldZone)
                {
                    // player was correct
                    p.AddPoints(Correct_Points);
                }
                else
                {
                    p.AddPoints(Wrong_Points);

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
                if(_goldClaimZone == _goldZone)
                {
                    _players[_activePlayerIndex].AddPoints(Truth_Points);
                }
            }
        }
        // move to next player
        NextPlayer_();
    }

    /// <summary>
    /// Move to the next player (to select drop zone)
    /// </summary>
    private void NextPlayer_()
    {
        var finished = false;

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
            EndGame_();
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
        foreach(var img in ColouredImages)
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
    /// Ends the game and returns to the menu
    /// </summary>
    private void EndGame_()
    {
        AssignBonusPoints_();

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
            player.Spawn(PlayerPrefab, StartPositions[index++]);
        }
    }
}
