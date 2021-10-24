using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ShopDropBallScript : MonoBehaviour
{
    public int Points { get; set; } = 0;
    public string Food { get; set; }
    private bool _complete = false;
    int _playerIndex;

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
        // if collided with a bomb explosion, destroy it
        else if(collision.gameObject.name.ToLower().Contains("bomb"))
        {
            PlayerManagerScript.Instance.GetPlayers()[_playerIndex].GetComponent<ShopDropInputHandler>().FoodLost(this);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Handle the destruction of the ball
    /// </summary>
    private IEnumerator BallComplete(int playerIndex)
    {
        _playerIndex = playerIndex;

        // wait a second, then destroy the object
        yield return new WaitForSeconds(1);
        var multiplicationFactor = PlayerManagerScript.Instance.GetPlayerCount() / 4f;
        PlayerManagerScript.Instance.GetPlayers()[playerIndex].GetComponent<ShopDropInputHandler>().FoodCollected(this, multiplicationFactor);
        Detonate();
    }

    /// <summary>
    /// Moves the ball from a slot to the specified trolley
    /// </summary>
    /// <param name="trolley">The trolley to move to</param>
    internal void MoveToTrolley(Transform trolley)
    {
        if (!_complete)
        {
            _complete = true;

            // no longer moving
            RigidBodyBall.velocity = Vector3.zero;

            // move into trolley
            transform.SetParent(trolley);
            transform.localPosition = new Vector3(-0.18f, -0.1f, 1);

            // make smaller
            transform.localScale /= 1.8f;
        }
    }

    /// <summary>
    /// If this item is a bomb, detonate it
    /// </summary>
    public void Detonate()
    {
        var bomb = GetComponent<DropBombScript>();
        if (bomb != null)
        {
            StartCoroutine(bomb.Detonate());
        }
    }

}