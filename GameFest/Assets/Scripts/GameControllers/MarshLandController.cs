using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MarshLandController : MonoBehaviour
{
    // configuration
    private const int GAME_TIMEOUT = 120;

    // Unity configuration
    public Vector2[] PlayerSpawnPositions;
    public Transform PlayerPrefab;
    public CameraFollow CameraFollowScript;
    public Text CountdownTimer;
    public MarshLandInputDisplay[] InputDisplays;

    // time out
    TimeLimit _overallLimit;

    // static instance
    public static MarshLandController Instance;

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

        // display correct colours
        SetDisplays_();

        // setup the timeout
        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _overallLimit.Initialise(GAME_TIMEOUT, OnTimeLimitTick, OnTimeUp);

        // start the timer
        _overallLimit.StartTimer();
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
            InputDisplays[i].SetColour(i);
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
    public void CheckComplete()
    {
        var allComplete = true;

        // loop through each player
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // look for any players that are not complete
            if(!player.GetComponent<MarshLandInputHandler>().Active())
            {
                allComplete = false;
            }
        }

        // hides player displays for those players who are complete
        UpdateDisplays_();

        // if all complete, end the game
        if (allComplete)
        {
            StartCoroutine(EndGame_());
        }
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndGame_()
    {
        // TODO: display results
        yield return new WaitForSeconds(4);

        // return to home
        PlayerManagerScript.Instance.NextScene(Scene.GameCentral);
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
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        // disable all player
        foreach(var player in PlayerManagerScript.Instance.GetPlayers())
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
            InputDisplays[i].gameObject.SetActive(PlayerManagerScript.Instance.GetPlayers()[i].GetComponent<MarshLandInputHandler>().Active());
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
}
