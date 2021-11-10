using System;
using System.Collections.Generic;
using UnityEngine;

public class CashDashController : MonoBehaviour
{
    public static CashDashController Instance;

    public Transform PlayerPrefab;
    public Vector3[] StartPositions;
    public CameraFollow CameraFollowScript;
    public CameraZoomFollow CameraFollowZoomScript;
    public Collider2D[] BvColliders;

    public GameObject[] BvKeysLeft;
    public GameObject[] BvKeysRight;

    private void Start()
    {
        Instance = this;

        var players = SpawnPlayers_();

        // assign players to the camera
        CameraFollowScript.SetPlayers(players, FollowDirection.Up);
        CameraFollowZoomScript.SetPlayers(players, FollowDirection.Up);

        HideUnusedKeys_();
    }

    private void HideUnusedKeys_()
    {
        for(int i = PlayerManagerScript.Instance.GetPlayerCount(); i < BvKeysLeft.Length; i++)
        {
            BvKeysLeft[i].SetActive(false);
            BvKeysRight[i].SetActive(false);
        }
    }

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
