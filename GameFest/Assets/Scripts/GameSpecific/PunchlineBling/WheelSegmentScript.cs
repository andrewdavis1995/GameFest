using System;
using UnityEngine;

public class WheelSegmentScript : MonoBehaviour
{
    // player data
    int _playerIndex;
    Color _colour;

    // Unity config
    SpriteRenderer _spriteRenderer;

    /// <summary>
    /// Links a player to this segment
    /// </summary>
    /// <param name="playerIndex">The index of the player that the segment belongs to</param>
    /// <param name="playerName">The name of the player that the segment belongs to</param>
    internal void Initialise(int playerIndex, string playerName)
    {
        // set index
        _playerIndex = playerIndex;
        // display name on segment
        GetComponentInChildren<TextMesh>().text = playerName;

        // get the colour for this player
        _colour = ColourFetcher.GetColour(playerIndex);

        // set the colour of the segment
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = _colour;
    }

    /// <summary>
    /// Returns the index of the player that the segment relates to
    /// </summary>
    /// <returns>The players index</returns>
    internal int GetPlayerIndex()
    {
        return _playerIndex;
    }

    /// <summary>
    /// Sets the colour back to normal (after being brighter)
    /// </summary>
    internal void ResetColour()
    {
        _spriteRenderer.color = _colour;
    }

    /// <summary>
    /// Brightens the colour of the segment (used to show it flashing)
    /// </summary>
    internal void SetBrightColour()
    {
        _spriteRenderer.color = new Color(_colour.r * 1.25f, _colour.g * 1.25f, _colour.b * 1.25f);
    }
}
