using System;
using System.Collections;
using UnityEngine;

public class ItemFlash : MonoBehaviour
{
    // Controls behaviour of the flash
    float MAX_SIZE = 1.1f;
    float SPEED = 0.01f;

    // status variables
    bool _active = false;
    int _blingIndex = 0;

    // callbacks
    Action<int> _createCallback;
    Action _endCallback;

    private void Update()
    {
        // rotate the flash while it is active
        if (_active)
            transform.eulerAngles += new Vector3(0, 0, 1f);
    }

    /// <summary>
    /// Triggers the start of the flash
    /// </summary>
    /// <param name="createCallback">Callback to call once flash reaches largest point</param>
    /// <param name="endCallback">Callback to call once flash shrinks back to zero</param>
    /// <param name="index">Index of the item this links to</param>
    /// <param name="size">The size to grow to</param>
    /// <param name="speed">The speed at which to grow</param>
    public void Go(Action<int> createCallback, Action endCallback, int index, float size = 1.1f, float speed = 0.05f)
    {
        // set variables
        MAX_SIZE = size;
        SPEED = speed;
        _blingIndex = index;

        _active = true;

        // set callbacks
        _createCallback = createCallback;
        _endCallback = endCallback;

        // start growing
        StartCoroutine(Grow_());
    }

    /// <summary>
    /// Grows, then shrinks, th flash
    /// </summary>
    IEnumerator Grow_()
    {
        // grow to maximum
        while (transform.localScale.x < MAX_SIZE)
        {
            transform.localScale += new Vector3(SPEED, SPEED, 0);
            yield return new WaitForSeconds(0.001f);
        }

        // run callback
        _createCallback?.Invoke(_blingIndex);

        // shrink to 0
        while (transform.localScale.x > 0)
        {
            transform.localScale -= new Vector3(SPEED, SPEED, 0);
            yield return new WaitForSeconds(0.001f);
        }

        _active = false;

        // run callback
        _endCallback?.Invoke();
    }
}
