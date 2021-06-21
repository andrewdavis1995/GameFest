using System;
using UnityEngine;

public class WheelSegmentScript : MonoBehaviour
{
    int _playerIndex;
    Color _colour;
    SpriteRenderer _spriteRenderer;

    internal void Initialise(int playerIndex, string playerName)
    {
        _playerIndex = playerIndex;
        GetComponentInChildren<TextMesh>().text = playerName;

        _colour = ColourFetcher.GetColour(playerIndex);
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = _colour;
    }

    internal int GetPlayerIndex()
    {
        return _playerIndex;
    }

    internal void ResetColour()
    {
        _spriteRenderer.color = _colour;
    }

    internal void SetBrightColour()
    {
        _spriteRenderer.color = new Color(_colour.r * 1.25f, _colour.g * 1.25f, _colour.b * 1.25f);
    }
}
