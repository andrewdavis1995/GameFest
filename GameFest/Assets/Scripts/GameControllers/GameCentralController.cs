using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using UnityEngine;

public enum Scene { Lobby, GameCentral, PunchlineBling, ShopDrop, MarshLand, Landslide, XTinguish }

public class GameCentralController : MonoBehaviour
{
    public Transform PlayerPrefab;      // The prefab to create

    public float START_LEFT = 0;        // where to start spawning players
    public float POSITION_GAP = 0;      // the gap to leave between players

    MiniGameManager _manager;   // the manager to handle mini games

    public TransitionFader EndFader;

    Scene _sceneToLoad;

    public TextMesh[] NameTexts;
    public TextMesh[] ScoreTexts;

    List<Scene> _games = new List<Scene>();

    public static GameCentralController Instance;

    /// <summary>
    /// Called when item is created
    /// </summary>
    void Start()
    {
        Instance = this;
        EndFader.StartFade(1, 0, ContinueWithProcess_);
    }

    /// <summary>
    /// Continues to display scores and player info
    /// </summary>
    private void ContinueWithProcess_()
    {
        // loop through all players
        float left = START_LEFT;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(GameCentralInputHandler));

            // create the "visual" player
            var spawned = player.Spawn(PlayerPrefab, new Vector2(left, 0));

            // move to next position
            left += POSITION_GAP;

            // get the latest points
            player.UpdatePoints();

            // display player info
            NameTexts[player.PlayerInput.playerIndex].text = player.GetPlayerName();
            ScoreTexts[player.PlayerInput.playerIndex].text = player.GetPoints().ToString();
        }

        // fade out, then load the game
        _sceneToLoad = Scene.PunchlineBling;

        StartCoroutine(DelayedLoad_());
    }

    /// <summary>
    /// Pause briefly before fading out
    /// </summary>
    private IEnumerator DelayedLoad_()
    {
        yield return new WaitForSeconds(2);
        EndFader.StartFade(0, 1, LoadMiniGame);
    }

    /// <summary>
    /// Loads the specified game
    /// </summary>
    private void LoadMiniGame()
    {
        PlayerManagerScript.Instance.NextScene(_sceneToLoad);
    }

    /// <summary>
    /// Set the list of games to play
    /// </summary>
    /// <param name="games"></param>
    public void SetGames(List<Scene> games)
    {
        _games = games;
    }
}
