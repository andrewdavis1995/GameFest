using UnityEngine;

/// <summary>
/// Controls the flow of "Cart Attack"
/// </summary>
public class CartAttackController : MonoBehaviour
{
    public Collider2D[] Checkpoints;
    public CarControllerScript[] Cars;
    
    List<CartAttackInputHandler> _players = new List<CartAttackInputHandler>();

    public static CartAttackController Instance;

    // Called once on startup
    private void Start()
    {
        Instance = this;
        
        // TODO: this moves to SpawnPlayers (add to list)
        _players = FindObjectsOfType<CartAttackInputHandler>().ToList();
        int index = 0;
        for(; index < Cars.Length && index < _players.Count; index++)
        {
            // assign car to player
            player.SetCarController(Cars[index]);
        }
        
        // TODO: Move to after SpawnPlayers
        HideUnusedElements_(index, Cars.Length);
        
        // TODO: this moves to after the countdown lights
        // enable all players
        foreach(var player in _players)
        {
            player.SetActiveState(true);
        }
    }
    
    /// <summary>
    /// Hides cars and UI elements that are not needed (due to there not being the full 4 players playing)
    /// </summary>
    /// <param id="index">The index to start at</param>
    /// <param id="index">The maximum number of items to go up to</param>
    void HideUnusedElements_(int index, int maximum)
    {
        // hide unused cars
        for(; index < maximum; index++)
        {
            // hide car
            Cars[index].gameObject.SetActive(false);
        }
    }
}
