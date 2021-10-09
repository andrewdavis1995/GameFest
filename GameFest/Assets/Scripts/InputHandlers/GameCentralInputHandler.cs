using System;
using System.Linq;
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
    /// <param name="id">Profile ID</param>
    /// <returns>The transform that was created</returns>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex, Guid id)
    {
        base.Spawn(prefab, position, characterIndex, playerName, playerIndex, id);

        // create the object
        var spawned = Instantiate(prefab, position, Quaternion.identity);
        SetAnimation(spawned, characterIndex);
        SetHeight(spawned, characterIndex);
        spawned.GetComponent<PlayerMovement>().Shadow.gameObject.SetActive(true);
        spawned.GetComponent<PlayerMovement>().Shadow.gameObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        spawned.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        spawned.GetComponent<SpriteRenderer>().sortingLayerName = "Player3B";
        return spawned;
    }
}
