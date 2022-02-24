using System.Collections.Generic;
using UnityEngine;

public class SpeakerScript : MonoBehaviour
{
    float INCREASE_FACTOR = 0.0025f;

    float[] _capturedValues = new float[4];
    List<int> _playersInZone = new List<int>();
    public SpriteRenderer[] ColourRenderers;
    public AudioSource[] AudioSources;
    public ParticleSystem[] Confetti;

    /// <summary>
    /// Hides unused elements
    /// </summary>
    /// <param name="numPlayers">The number of players in the game</param>
    public void Setup(int numPlayers)
    {
        for (int i = numPlayers; i < ColourRenderers.Length; i++)
        {
            ColourRenderers[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// When a player starts to claim this speaker
    /// </summary>
    /// <param name="playerIndex">The index of the player</param>
    internal void StartClaim(int playerIndex)
    {
        _playersInZone.Add(playerIndex);
    }

    /// <summary>
    /// When a player stops claiming this speaker
    /// </summary>
    /// <param name="playerIndex">The index of the player</param>
    internal void StopClaim(int playerIndex)
    {
        _playersInZone.Remove(playerIndex);
    }

    // Called once per frame
    private void Update()
    {
        foreach (var p in _playersInZone)
        {
            if (_capturedValues[p] <= 1)
            {
                _capturedValues[p] += INCREASE_FACTOR;

                // check if complete
                if (Mathf.Abs(1 - _capturedValues[p]) < INCREASE_FACTOR)
                {
                    // display light
                    ColourRenderers[p].color = ColourFetcher.GetColour(p);

                    // play audio
                    AudioSources[p].clip = ToneDeathController.Instance.GetAudioTrack(p);
                    AudioSources[p].Play();

                    // display confetti
                    var col = Confetti[p].main;
                    col.startColor = ColourFetcher.GetColour(p);
                    Confetti[p].Play();
                }
            }
        }
    }
}
