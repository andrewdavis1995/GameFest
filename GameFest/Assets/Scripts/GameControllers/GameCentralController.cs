using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using UnityEngine;

public enum Scene { Lobby, GameCentral, PunchlineBling, ShopDrop, MarshLand, Landslide, XTinguish, BeachBowles }

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

    public Transform Floor;
    public float FloorEndPoint;

    public GameObject[] Plinths;

    /// <summary>
    /// Called when item is created
    /// </summary>
    void Start()
    {
        Instance = this;
        SpawnPlayers_();
        EndFader.StartFade(1, 0, RevealPlayers_);
    }

    void RevealPlayers_()
    {
        StartCoroutine(RevealPlayersIEnum_());
    }

    IEnumerator RevealPlayersIEnum_()
    {
        while (Floor.transform.localPosition.y < FloorEndPoint)
        {
            Floor.transform.Translate(new Vector3(0, 2 * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }
        ContinueWithProcess_();
    }

    IEnumerator HidePlayersIEnum_()
    {
        foreach (var player in FindObjectsOfType<PlayerMovement>())
        {
            player.DisableAnimators();
        }

        while (Floor.transform.localPosition.y > -1000)
        {
            Floor.transform.Translate(new Vector3(0, 2 * -Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }
        ContinueWithProcess_();
    }

    void SpawnPlayers_()
    {
        // loop through all players
        float left = START_LEFT;
        int index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(GameCentralInputHandler));

            // create the "visual" player
            var spawned = player.Spawn(PlayerPrefab, new Vector2(left, -5.5f));

            // move to next position
            left += POSITION_GAP;

            // get the latest points
            player.UpdatePoints();

            // display player info
            NameTexts[player.PlayerInput.playerIndex].text = player.GetPlayerName();
            ScoreTexts[player.PlayerInput.playerIndex].text = player.GetPoints().ToString();

            Plinths[index].GetComponentsInChildren<SpriteRenderer>()[1].color = ColourFetcher.GetColour(index);
            Plinths[index++].SetActive(true);
        }
    }

    /// <summary>
    /// Continues to display scores and player info
    /// </summary>
    private void ContinueWithProcess_()
    {
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
        yield return new WaitForSeconds(0.1f);

        MisterA.Fly(-100f, null);

        StartCoroutine(HidePlayersIEnum_());

        // let players start to go down
        yield return new WaitForSeconds(0.1f);
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
