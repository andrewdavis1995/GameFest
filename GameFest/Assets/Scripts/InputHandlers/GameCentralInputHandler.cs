using UnityEngine;

public class GameCentralInputHandler : GenericInputHandler
{
    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerIndex">The index of the player</param>
    /// <returns>The transform that was created</returns>
    public override Transform Spawn(Transform prefab, Vector2 position, int characterIndex, string playerName, int playerIndex)
    {
        // create the object
        var spawned = Instantiate(prefab, position, Quaternion.identity);
        SetAnimation(spawned, characterIndex);
        SetHeight(spawned, characterIndex);
        return spawned;
    }
}
