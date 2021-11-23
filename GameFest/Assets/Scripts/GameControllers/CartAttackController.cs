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
        for(int index = 0; index < Cars.Length && index < _players.Count; index++)
        {
            // assign car to player
            player.SetCarController(Cars[index]);
        }
        
        // TODO: this moves to after the countdown lights
        // enable all players
        foreach(var player in _players)
        {
            player.SetActiveState(true);
        }
    }
}
