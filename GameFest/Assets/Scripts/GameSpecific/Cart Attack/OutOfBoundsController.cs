using UnityEngine;

public class OutOfBoundsController : MonoBehaviour
{
    public CarControllerScript Car;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Finish" || collision.gameObject.tag == "Checkpoint")
        {
            Car.NextCheckpoint(collision);
        }
        else if (collision.gameObject.tag == "AreaTrigger")
        {
            Car.OutOfBoundsEntered(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "AreaTrigger")
        {
            Car.OutOfBoundsExited(collision);
        }
    }
}
