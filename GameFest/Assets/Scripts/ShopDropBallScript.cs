using System;
using System.Collections;
using UnityEngine;

public class ShopDropBallScript : MonoBehaviour
{
    public int Points { get; set; } = 0;
    public string Food { get; set; }

    public Rigidbody2D RigidBodyBall;

    /// <summary>
    /// When the ball collides with a trigger
    /// </summary>
    /// <param name="collision">The item the ball collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "AreaTrigger")
        {
            var index = int.Parse(collision.gameObject.name.Replace("AREA_", ""));
            StartCoroutine(BallComplete(index));
        }
    }

    /// <summary>
    /// Handle the destruction of the ball
    /// </summary>
    private IEnumerator BallComplete(int playerIndex)
    {
        // wait a second, then destroy the object
        yield return new WaitForSeconds(1);
        PlayerManagerScript.Instance.GetPlayers()[playerIndex].GetComponent<ShopDropInputHandler>().FoodCollected(this);
    }

    /// <summary>
    /// Moves the ball from a slot to the specified trolley
    /// </summary>
    /// <param name="trolley">The trolley to move to</param>
    internal void MoveToTrolley(Transform trolley)
    {
        // no longer moving
        RigidBodyBall.velocity = Vector3.zero;

        // move into trolley
        transform.SetParent(trolley);
        transform.localPosition = new Vector3(-0.18f, -0.1f, -1);

        // make smaller
        transform.localScale /= 1.7f;
    }
}
