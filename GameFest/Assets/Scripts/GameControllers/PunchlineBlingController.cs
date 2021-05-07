using UnityEngine;

public class PunchlineBlingController : MonoBehaviour
{
    public Transform PlayerPrefab;      // The prefab to create
    public Vector2[] StartPositions;    // Where the players should spawn

    // Start is called before the first frame update
    void Start()
    {
        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(PunchlineBlingInputHandler));

            // create the "visual" player at the start point
            player.Spawn(PlayerPrefab, StartPositions[index++]);
        }
    }
}
