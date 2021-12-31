using System;
using UnityEngine;

public class BackgroundRocketScript : MonoBehaviour
{
    [SerializeField]
    GameObject Shadow;
    [SerializeField]
    GameObject[] Propulsions;
    public AudioSource TakeOffNoise;

    /// <summary>
    /// Makes the rocket take off
    /// </summary>
    internal void TakeOff()
    {
        TakeOffNoise.Play();

        Shadow.SetActive(false);
        foreach (var p in Propulsions)
            p.SetActive(true);
    }
}
