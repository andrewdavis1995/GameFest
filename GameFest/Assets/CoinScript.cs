using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class to control coins or collectables
/// </summary>
public class CoinScript : MonoBehaviour
{
    public int Points;
    public UnityEvent Callback;
    public AudioSource CollectedNoise;
    public SpriteRenderer Renderer;
    public Collider2D Collider;

    bool _active = true;

    /// <summary>
    /// Returns if the coin is enabled or not
    /// </summary>
    /// <returns>Whether the coin is active</returns>
    public bool IsActive() { return _active; }

    /// <summary>
    /// Disables the coin (once collected)
    /// </summary>
    public void Disable()
    {
        CollectedNoise.Play();

        _active = false;
        Collider.enabled = false;
        Renderer.enabled = false;

        Callback?.Invoke();
    }
}
