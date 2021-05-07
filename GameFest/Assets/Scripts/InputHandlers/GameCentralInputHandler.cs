using UnityEditor.Animations;
using UnityEngine;

public class GameCentralInputHandler : GenericInputHandler
{
    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    public override void Spawn(Transform prefab, Vector2 position, int characterIndex)
    {
        // create the object
        var spawned = Instantiate(prefab, position, Quaternion.identity);
        spawned.GetComponent<Rigidbody2D>().AddForce(new Vector2(100, 100));
        spawned.GetComponent<Animator>().runtimeAnimatorController = GameCentralController.Instance.CharacterControllers[characterIndex];
    }
}
