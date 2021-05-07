using UnityEngine;

public class GameCentralController : MonoBehaviour
{
    public Transform PlayerPrefab;      // The prefac to create

    public float START_LEFT = 0;        // where to start spawning players
    public float POSITION_GAP = 0;      // the gap to leave between players

    public RuntimeAnimatorController[] CharacterControllers;   // controllers to control players appearance and animations

    public static GameCentralController Instance;       // static instance of this object

    /// <summary>
    /// Called when item is created
    /// </summary>
    void Start()
    {
        // keep a static instance to this class
        Instance = this;

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
        }
    }
}
