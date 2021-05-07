using UnityEngine;

public class GameCentralInputHandler : GenericInputHandler
{
    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    public override void Spawn(Transform prefab, Vector2 position)
    {
        // create the object
        Instantiate(prefab, position, Quaternion.identity);
    }
}
