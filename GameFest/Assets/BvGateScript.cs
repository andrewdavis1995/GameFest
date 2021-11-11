using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BvGateScript : MonoBehaviour
{
    public TextMesh BvSignText;
    public SpriteRenderer BvSignImage;
    public ParticleSystem Particles;

    ParticleSystem.MainModule _particleModule;

    Color _particleColour = new Color(0.4313f, 0.1137f, 0959f);

    private void Start()
    {
        _particleModule = Particles.main;

    }

    internal void DisplayMessage(string message, int playerColour)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayMessage_(message, playerColour));
    }

    IEnumerator DisplayMessage_(string message, int playerColour)
    {
        _particleModule.startColor = ColourFetcher.GetColour(playerColour);
        BvSignText.text = message;
        BvSignImage.color = ColourFetcher.GetColour(playerColour);
        yield return new WaitForSeconds(2f);
        BvSignImage.color = _particleColour;
        BvSignText.text = "Waiting";
        _particleModule.startColor = _particleColour;
    }
}
