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

    public static GameCentralController Instance;

    private static int _gameIndex = 0;

    public MrAController MisterA;

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


        if (_gameIndex < PlayerManagerScript.Instance.SelectedGames.Count)
        {
            // fade out, then load the game
            _sceneToLoad = PlayerManagerScript.Instance.SelectedGames[_gameIndex++];

            StartCoroutine(DelayedLoad_());
        }
        else
        {
            // TODO: end game
        }
    }

    /// <summary>
    /// Pause briefly before fading out
    /// </summary>
    private IEnumerator DelayedLoad_()
    {
        yield return new WaitForSeconds(1);
        MisterA.Fly(-2.4f, FlyInComplete);
    }

    void FlyInComplete()
    {
        StartCoroutine(ReadIntro());
    }

    IEnumerator ReadIntro()
    {
        yield return new WaitForSeconds(4);
        EndFader.StartFade(0, 1, LoadMiniGame);
    }



    /// <summary>
    /// Loads the specified game
    /// </summary>
    private void LoadMiniGame()
    {
        PlayerManagerScript.Instance.NextScene(_sceneToLoad);
    }
}
