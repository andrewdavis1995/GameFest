using System;
using System.Collections.Generic;
using UnityEngine;

public class CashDashController : MonoBehaviour
{
    public static CashDashController Instance;

    public Transform PlayerPrefab;
    public Vector3[] StartPositions;
    public CameraFollow CameraFollowScript;
    public Collider2D[] BvColliders;
    public Sprite KeyIcon;

    public Sprite[] DisabledImages;

    public GameObject[] BvKeysLeft;
    public GameObject[] BvKeysRight;

    /// <summary>
    /// Called once on startup
    /// </summary>
    private void Start()
    {
        Instance = this;

        var players = SpawnPlayers_();

        // assign players to the camera
        CameraFollowScript.SetPlayers(players, FollowDirection.Up);

        HideUnusedKeys_();
    }

    /// <summary>
    /// Hides the unused keys (for players who are not taking part)
    /// </summary>
    private void HideUnusedKeys_()
    {
        // for all indexes after the number of players, hide keys
        for (int i = PlayerManagerScript.Instance.GetPlayerCount(); i < BvKeysLeft.Length; i++)
        {
            BvKeysLeft[i].SetActive(false);
            BvKeysRight[i].SetActive(false);
        }
    }

    /// <summary>
    /// Spawns the player movement objects to be used in this game
    /// </summary>
    /// <returns>List of spawned items</returns>
    private List<Transform> SpawnPlayers_()
    {
        var playerTransforms = new List<Transform>();

        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(CashDashInputHandler));

            // create the "visual" player at the start point
            var playerTransform = player.Spawn(PlayerPrefab, StartPositions[index++]);

            playerTransforms.Add(playerTransform);
        }

        return playerTransforms;
    }
}
