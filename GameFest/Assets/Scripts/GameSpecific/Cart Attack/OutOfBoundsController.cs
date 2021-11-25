using UnityEngine;

/// <summary>
/// Controller for back off car, to check if it went out of bounds
/// </summary>
public class OutOfBoundsController : MonoBehaviour
{
    public CarControllerScript Car;

    /// <summary>
    /// Called when something enters the trigger
    /// <summary>
    /// <param id="collision">The item that entered the trigger</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // checkpoints/finish line
        if (collision.gameObject.tag == "Finish" || collision.gameObject.tag == "Checkpoint")
        {
            Car.NextCheckpoint(collision);
        }
        // out of bounds
        else if (collision.gameObject.tag == "AreaTrigger")
        {
            Car.OutOfBoundsEntered(collision);
        }
    }

    /// <summary>
    /// Called when something leaves the trigger
    /// <summary>
    /// <param id="collision">The item that left the trigger</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // out of bounds
        if (collision.gameObject.tag == "AreaTrigger")
        {
            Car.OutOfBoundsExited(collision);
        }
    }
}
