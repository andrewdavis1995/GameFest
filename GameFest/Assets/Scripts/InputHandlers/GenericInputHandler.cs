using UnityEngine;
using UnityEngine.InputSystem;

public abstract class GenericInputHandler : MonoBehaviour
{
    [SerializeField]
    private int _points = 0;

    public virtual Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex)
    {
        _playerIndex = playerIndex;
        _playerName = playerName;
        _characterIndex = characterIndex;
        return null;
    }

    public virtual void OnMove(InputAction.CallbackContext ctx) { }
    public virtual void OnCross() { }
    public virtual void OnCircle() { }
    public virtual void OnTriangle() { }
    public virtual void OnSquare() { }
    public virtual void OnTouchpad() { }
    public virtual void OnL1() { }
    public virtual void OnR1() { }
    public virtual void OnL2() { }
    public virtual void OnR2() { }

    public virtual void TriggerEnter(Collision2D collision) { }
    public virtual void TriggerExit(Collision2D collision) { }

    // status variables
    int _playerIndex;
    int _characterIndex;
    string _playerName;

    /// <summary>
    /// Returns of the character being used by this player
    /// </summary>
    /// <returns>The character index</returns>
    internal int GetCharacterIndex()
    {
        return _characterIndex;
    }

    /// <summary>
    /// Returns of the character being used by this player
    /// </summary>
    /// <param name="index">The value to set</param>
    internal void SetCharacterIndex(int index)
    {
        _characterIndex = index;
    }

    /// <summary>
    /// Returns the name of the player
    /// </summary>
    /// <returns>The players name</returns>
    internal string GetPlayerName()
    {
        return _playerName;
    }

    /// <summary>
    /// Gets the index of the player
    /// </summary>
    /// <returns>The index of the player</returns>
    internal int GetPlayerIndex()
    {
        return _playerIndex;
    }

    /// <summary>
    /// Add points for the current game
    /// </summary>
    /// <param name="points">The points to add</param>
    public virtual void AddPoints(int points)
    {
        _points += points;
    }

    /// <summary>
    /// Returns the number of points won by this player
    /// </summary>
    /// <returns>Points earned</returns>
    public int GetPoints()
    {
        return _points;
    }

    /// <summary>
    /// Set the height of the player based on the selected character
    /// </summary>
    /// <param name="player">The transform the the player display</param>
    /// <param name="characterIndex">The index of the chosen character</param>
    public virtual void SetHeight(Transform player, int characterIndex)
    {
        float size = 0.2f;

        switch (characterIndex)
        {
            // Andrew
            case 0:
                size += 0.031f;
                break;
                // Rachel
            case 1:
                size -= 0.022f;
                break;
                // Naomi
            case 2:
                size += 0f;
                break;
                // Heather
            case 3:
                size -= 0.019f;
                break;
                // Mum & Dad
            case 4:
            case 5:
                size += 0.025f;
                break;
                // John & Fraser
            case 6:
            case 7:
                size += 0.022f;
                break;
                // Matthew
            case 8:
                size += 0.04f;
                break;
        }

        // set height
        player.localScale = new Vector3(size, size, 1);
    }

    /// <summary>
    /// Sets the animation to that of the correct character
    /// </summary>
    /// <param name="spawned">The instantiated object with an animator attached</param>
    /// <param name="characterIndex">The index of the selected character</param>
    public void SetAnimation(Transform spawned, int characterIndex)
    {
        // update the animation controller
        spawned.GetComponent<Animator>().runtimeAnimatorController = PlayerManagerScript.Instance.CharacterControllers[characterIndex];
    }
}