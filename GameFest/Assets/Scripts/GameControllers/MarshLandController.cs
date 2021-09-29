using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MarshLandController : GenericController
{
    // configuration
    private const int GAME_TIMEOUT = 180;
    private const int MAX_POINTS = 1800;
    private int[] POSITIONAL_POINTS = { 160, 70, 20 };

    // Unity configuration
    public Vector2[] PlayerSpawnPositions;
    public Vector2 PlayerEndPositionStart;
    public Transform PlayerPrefab;
    public CameraFollow CameraFollowScript;
    public CameraZoomFollow CameraFollowZoomScript;
    public Text CountdownTimer;
    public MarshLandInputDisplay[] InputDisplays;
    public Text PointsCountdown;
    public Vector3 ResultScreenCameraPosition;
    public GameObject UI;
    public int ServingPosition;
    public GameObject SpeechBubble;
    public TextMesh SpeechBubbleText;

    // time out
    TimeLimit _overallLimit;
    TimeLimit _pointCountdown;

    // static instance
    public static MarshLandController Instance;

    // race status
    List<int> _completedPlayers = new List<int>();
    int _remainingPoints;
    int _resultsPlayerIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // hides the marshmallows that aren't assigned to any player
        HideUnusedMarshmallows_();

        // create players
        var playerTransforms = SpawnPlayers_();

        // assign players to the camera
        CameraFollowScript.SetPlayers(playerTransforms, FollowDirection.Right);
        CameraFollowZoomScript.SetPlayers(playerTransforms, FollowDirection.Right);

        // display correct colours
        SetDisplays_();

        // setup the timeout
        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _overallLimit.Initialise(GAME_TIMEOUT, OnTimeLimitTick, OnTimeUp);
        _pointCountdown = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _pointCountdown.Initialise(MAX_POINTS, OnPointsTick, null, 0.1f);

        // start the timer
        _overallLimit.StartTimer();
        _pointCountdown.StartTimer();
    }

    /// <summary>
    /// Called every 0.1 seconds
    /// </summary>
    /// <param name="seconds">How many points are left</param>
    private void OnPointsTick(int points)
    {
        _remainingPoints = points;
        PointsCountdown.text = points.ToString();
    }

    /// <summary>
    /// Sets the colour and visibility of the input displays
    /// </summary>
    private void SetDisplays_()
    {
        int i = 0;
        // set all colours
        for (; i < PlayerManagerScript.Instance.GetPlayerCount(); i++)
        {
            InputDisplays[i].SetColour(i, PlayerManagerScript.Instance.GetPlayers()[i].GetPlayerName());
        }
        // hide unused
        for (; i < PlayerManagerScript.Instance.Manager.maxPlayerCount; i++)
        {
            InputDisplays[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Spawn a player display for each player
    /// </summary>
    /// <returns>The list of transforms that were created</returns>
    private List<Transform> SpawnPlayers_()
    {
        var playerTransforms = new List<Transform>();

        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // add the input handle, and assign all paddles
            player.SetActiveScript(typeof(MarshLandInputHandler));

            // create the "visual" player at the start point
            var playerTransform = player.Spawn(PlayerPrefab, PlayerSpawnPositions[index]);
            playerTransforms.Add(playerTransform);

            // create action list, based on how many marshmallows this player must jump
            var marshmallows = FindObjectsOfType<MarshmallowScript>();
            var playerMarshmallows = marshmallows.Where(m => m.name.Contains((player.PlayerInput.playerIndex + 1).ToString()));
            player.GetComponent<MarshLandInputHandler>().SetActionList(playerMarshmallows.Count() - 1);

            index++;
        }

        return playerTransforms;
    }

    /// <summary>
    /// Checks if all players are complete
    /// </summary>
    /// <param name="index">The index of the newly completed player</param>
    public void CheckComplete(int index)
    {
        // store the player who is complete
        _completedPlayers.Add(index);

        var allComplete = true;

        // loop through each player
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            var handler = player.GetComponent<MarshLandInputHandler>();

            // if this player is the one who just completed
            if (player.PlayerInput.playerIndex == index && handler.GetPoints() == 0)
            {
                CameraFollowScript.RemovePlayer(handler.GetPlayerTransform());
                // add however many points are left in countdown
                handler.AddPoints(_remainingPoints);
                // add points based on position finished
                handler.AddPoints(POSITIONAL_POINTS[_completedPlayers.Count - 1]);
            }

            // look for any players that are not complete
            if (handler.Active())
            {
                allComplete = false;
            }
        }

        // hides player displays for those players who are complete
        UpdateDisplays_();

        // if all complete, end the game
        if (allComplete && CameraFollowScript.enabled)
        {
            StartCoroutine(EndGame_());
        }
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    private IEnumerator EndGame_()
    {
        CameraFollowScript.enabled = false;

        // kill timers
        _overallLimit.Abort();
        _pointCountdown.Abort();

        // hide the UI
        UI.SetActive(false);

        yield return new WaitForSeconds(1);

        // move players to correct position
        SetEndPositions_();

        yield return new WaitForSeconds(2f);

        StartCoroutine(CallNextPlayer());
    }

    /// <summary>
    /// Moves all players to the end position
    /// </summary>
    private void SetEndPositions_()
    {
        Camera.main.transform.position = ResultScreenCameraPosition;

        var allPlayers = FindObjectsOfType<PlayerJumper>();
        int index = 0;

        // loop through players
        foreach (var player in allPlayers)
        {
            player.transform.parent = null;
            player.transform.position = PlayerEndPositionStart + new Vector2(index++, 0);
            player.transform.localScale *= 1.75f;
        }
    }

    /// <summary>
    /// Hides the specified display
    /// </summary>
    /// <param name="playerIndex">The index of the player</param>
    internal void HideDisplay(int playerIndex)
    {
        InputDisplays[playerIndex].gameObject.SetActive(false);
    }

    /// <summary>
    /// Hides the marshmallows that are not assigned to a player
    /// </summary>
    private void HideUnusedMarshmallows_()
    {
        var marshmallows = FindObjectsOfType<MarshmallowScript>();

        // loop through unused player slots
        for (int i = PlayerManagerScript.Instance.GetPlayerCount(); i < PlayerManagerScript.Instance.Manager.maxPlayerCount; i++)
        {
            // find marshmallows that would be assigned to these missing players
            var playerMarshmallows = marshmallows.Where(m => m.name.Contains((i + 1).ToString()));

            // disable these marshmallows
            foreach (var marshmallow in playerMarshmallows)
                if (marshmallow.OffsetX == 0)
                    marshmallow.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates UI for player falling into water
    /// </summary>
    /// <param name="playerIndex">Index of the player</param>
    internal void Fall(int playerIndex)
    {
        InputDisplays[playerIndex].FallInWater();
    }

    /// <summary>
    /// Updates UI for player getting out of water
    /// </summary>
    /// <param name="playerIndex">Index of the player</param>
    internal void RecoverPlayer(int playerIndex)
    {
        InputDisplays[playerIndex].Recover();
    }

    /// <summary>
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        // disable all player
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            player.GetComponent<MarshLandInputHandler>().Active(false);
        }

        // hide all displays
        UpdateDisplays_();

        // show results
        StartCoroutine(EndGame_());
    }

    /// <summary>
    /// Shows/hides displays the input action based on whether the player is active
    /// </summary>
    private void UpdateDisplays_()
    {
        // hide unused
        for (int i = 0; i < PlayerManagerScript.Instance.Manager.maxPlayerCount; i++)
        {
            var playerExists = i < PlayerManagerScript.Instance.GetPlayerCount();
            InputDisplays[i].gameObject.SetActive(playerExists && PlayerManagerScript.Instance.GetPlayers()[i].GetComponent<MarshLandInputHandler>().Active());
        }
    }

    /// <summary>
    /// Called each second
    /// </summary>
    /// <param name="seconds">How many seconds are left</param>
    void OnTimeLimitTick(int seconds)
    {
        // display countdown from 10 to 0
        CountdownTimer.text = seconds <= 10 ? seconds.ToString() : "";
    }

    /// <summary>
    /// Displays the action for a player
    /// </summary>
    /// <param name="index">The player index</param>
    /// <param name="action">The action to display</param>
    public void SetAction(int index, MarshLandInputAction action)
    {
        InputDisplays[index].SetAction(action, PlayerManagerScript.Instance.GetPlayers()[index].PlayerInput.devices.FirstOrDefault());
    }

    /// <summary>
    /// Sets the action display to blank
    /// </summary>
    public void ClearAction(int index)
    {
        InputDisplays[index].ClearAction();
    }

    /// <summary>
    /// Called when a player has received their points and walked off
    /// </summary>
    void PlayerOrderComplete()
    {
        _resultsPlayerIndex++;

        // if there is a player left, show them
        if (_resultsPlayerIndex < PlayerManagerScript.Instance.GetPlayerCount())
        {
            StartCoroutine(CallNextPlayer());
        }
        else
        {
            // otherwise, go back to central page
            StartCoroutine(ReturnToCentral());
        }
    }

    /// <summary>
    /// Returns to the central page, after a short delay
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReturnToCentral()
    {
        yield return new WaitForSeconds(2);

        // Move to the central page
        PlayerManagerScript.Instance.NextScene(Scene.GameCentral);
    }

    /// <summary>
    /// The "barista" calls a player to get their points, and the player walks on
    /// </summary>
    IEnumerator CallNextPlayer()
    {
        if (_completedPlayers.Any(p => p == _resultsPlayerIndex))
        {
            yield return new WaitForSeconds(1.5f);
            var handler = PlayerManagerScript.Instance.GetPlayers()[_resultsPlayerIndex].GetComponent<MarshLandInputHandler>();
            Speak(handler.GetPoints() + " points\nfor\n" + handler.PlayerName());
            // wait a second, then make the player walk on
            yield return new WaitForSeconds(1f);
            handler.WalkOn(PlayerOrderComplete);
            yield return new WaitForSeconds(2.5f);
            HideSpeech();
        }
    }

    /// <summary>
    /// Displays a message in the speech bubble
    /// </summary>
    /// <param name="msg">The message to display</param>
    void Speak(string msg)
    {
        SpeechBubble.SetActive(true);
        SpeechBubbleText.text = TextFormatter.GetBubbleJokeString(msg);
    }

    /// <summary>
    /// Hides the speech bubble
    /// </summary>
    void HideSpeech()
    {
        SpeechBubble.SetActive(false);
    }
}
