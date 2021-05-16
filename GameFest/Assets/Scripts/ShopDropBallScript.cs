using System.Collections;
using UnityEngine;

public class ShopDropBallScript : MonoBehaviour
{
    /// <summary>
    /// When the ball collides with a trigger
    /// </summary>
    /// <param name="collision">The item the ball collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "AreaTrigger")
        {
            StartCoroutine(BallComplete());
        }
    }

    /// <summary>
    /// Handle the destruction of the ball
    /// </summary>
    private IEnumerator BallComplete()
    {
        // wait a second, then destroy the object
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
