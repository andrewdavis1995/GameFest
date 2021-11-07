using System;
using UnityEngine;
using UnityEngine.Events;

public class CoinScript : MonoBehaviour
{
    public int Points;
    public UnityEvent Callback;

    bool _active = true;

    public bool IsActive() { return _active; }

    public void Disable()
    {
        _active = false;
        gameObject.SetActive(false);

        Callback?.Invoke();
    }
}
