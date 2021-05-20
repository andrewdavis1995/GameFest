using System;
using System.Collections;
using Assets;
using UnityEngine;

public class GameCentralController : MonoBehaviour
{
    public Transform PlayerPrefab;      // The prefab to create

    public float START_LEFT = 0;        // where to start spawning players
    public float POSITION_GAP = 0;      // the gap to leave between players

    MiniGameManager _manager;   // the manager to handle mini games

    /// <summary>
    /// Called when item is created
    /// </summary>
    void Start()
    {
        // loop through all players
        float left = START_LEFT;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(GameCentralInputHandler));

            // create the "visual" player
            player.Spawn(PlayerPrefab, new Vector2(left, 0));

            // move to next position
            left += POSITION_GAP;

            // get the latest points
            player.UpdatePoints();
            Debug.Log(player.GetPoints());
        }

        // TODO: get from enum
        StartCoroutine(LoadMiniGame(2));
    }

    /// <summary>
    /// Loads the specified game
    /// </summary>
    /// <param name="gameIndex">The index of the scene to load</param>
    private IEnumerator LoadMiniGame(int gameIndex)
    {
        yield return new WaitForSeconds(1.2f);
        PlayerManagerScript.Instance.NextScene(gameIndex);
    }
}
