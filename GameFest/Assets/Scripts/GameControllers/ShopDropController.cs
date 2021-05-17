using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the flow of the "Shop Drop" mini game
/// </summary>
public class ShopDropController : MonoBehaviour
{
    // configuration
    public Vector2[] StartPositions;
    public int LeftBound;
    public int RightBound;
    public int BallDropHeight;

    // prefabs
    public Transform PlayerPrefab;
    public Transform BallPrefab;

    // state variables
    private bool _gameRunning = false;

    /// <summary>
    /// Runs at start
    /// </summary>
    void Start()
    {
        // assign paddles to players
        SetPaddles_();

        // assign zones to players
        SetZones_();

        // create players
        SpawnPlayers_();

        // start the game
        StartGame_();
    }

    /// <summary>
    /// Assigns players to each paddle
    /// </summary>
    void SetPaddles_()
    {
        int playerIndex = 0;

        // find all paddles
        PaddleScript[] paddles = GameObject.FindObjectsOfType<PaddleScript>();

        // loop through each paddle
        for (int i = 0; i < paddles.Length; i++)
        {
            // assign to current player, and set colour accordingly
            paddles[i].SetColour(playerIndex);

            // move to the next player, and loop around if at end
            playerIndex++;
            if (playerIndex >= PlayerManagerScript.Instance.GetPlayers().Count)
            {
                playerIndex = 0;
            }
        }
    }

    /// <summary>
    /// Assigns players to each paddle
    /// </summary>
    void SetZones_()
    {
        int playerIndex = 0;

        // find all paddles
        GameObject[] paddles = GameObject.FindGameObjectsWithTag("AreaTrigger");

        // loop through each paddle
        for (int i = 0; i < paddles.Length; i++)
        {
            // assign to current player, and set colour accordingly
            paddles[i].name = "AREA_" + playerIndex;
            paddles[i].GetComponentInChildren<SpriteRenderer>().color = ColourFetcher.GetColour(playerIndex);

            // move to the next player, and loop around if at end
            playerIndex++;
            if (playerIndex >= PlayerManagerScript.Instance.GetPlayers().Count)
            {
                playerIndex = 0;
            }
        }
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    void StartGame_()
    {
        _gameRunning = true;
        StartCoroutine(GenerateBalls_());
    }

    /// <summary>
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        _gameRunning = false;
    }

    /// <summary>
    /// Called each second
    /// </summary>
    /// <param name="seconds"></param>
    void OnTimeLimitTick(int seconds)
    {
        Debug.Log(seconds);
    }

    /// <summary>
    /// Runs throughout the game, controls the dropping of the balls
    /// </summary>
    private IEnumerator GenerateBalls_()
    {
        while (_gameRunning)
        {
            CreateBall_();
            yield return new WaitForSeconds(Random.Range(0.5f, 3f));
        }
    }

    /// <summary>
    /// Creates a ball to drop from the top
    /// </summary>
    void CreateBall_()
    {
        var left = Random.Range(LeftBound, RightBound);
        Instantiate(BallPrefab, new Vector2(left, BallDropHeight), Quaternion.identity);
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // add the input handle, and assign all paddles
            player.SetActiveScript(typeof(ShopDropInputHandler));
            player.GetComponent<ShopDropInputHandler>().AssignPaddles(index);

            // create the "visual" player at the start point - only used for turning
            player.Spawn(PlayerPrefab, StartPositions[index]);
            index++;
        }
    }
}
