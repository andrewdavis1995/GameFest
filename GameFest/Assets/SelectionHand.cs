using System;
using UnityEngine;

/// <summary>
/// Controls trigger collisions of the selection hand
/// </summary>
public class SelectionHand : MonoBehaviour
{
    // callbacks
    Action<CookingSelectionObject> _triggerEnterCallback;
    Action<CookingSelectionObject> _triggerExitCallback;

    /// <summary>
    /// When a trigger is entered
    /// </summary>
    /// <param name="collision">The collider that was collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // check it was an area trigger (a component)
        if (collision.tag == "AreaTrigger")
        {
            // get the selected object
            var cso = collision.GetComponent<CookingSelectionObject>();

            // check there was a script
            if (cso != null)
            {
                _triggerEnterCallback?.Invoke(cso);
            }
        }
    }

    /// <summary>
    /// When a trigger is left
    /// </summary>
    /// <param name="collision">The collider that was left</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "AreaTrigger")
        {
            // get the selected object
            var cso = collision.GetComponent<CookingSelectionObject>();

            // check there was a script
            if (cso != null)
            {
                _triggerExitCallback?.Invoke(cso);
            }
        }
    }

    /// <summary>
    /// Sets the functions to call when the player collides with a trigger
    /// </summary>
    /// <param name="triggerEnter">Function to call when player enters the trigger</param>
    /// <param name="triggerExit">Function to call when player leaves the trigger</param>
    internal void AddItemSelectionCallbacks(Action<CookingSelectionObject> triggerEnter, Action<CookingSelectionObject> triggerExit)
    {
        _triggerEnterCallback = triggerEnter;
        _triggerExitCallback = triggerExit;
    }
}
