using System;
using UnityEngine;

public class BackgroundRocketScript : MonoBehaviour
{
    [SerializeField]
    GameObject Shadow;
    [SerializeField]
    GameObject[] Propulsions;

    internal void TakeOff()
    {
        Shadow.SetActive(false);
        foreach (var p in Propulsions)
            p.SetActive(true);
    }
}
