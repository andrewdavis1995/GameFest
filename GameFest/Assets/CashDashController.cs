using UnityEngine;

public class CashDashController : MonoBehaviour
{
    public PlayerMovement Player;

    private void Start()
    {
        SpawnPlayers_();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Player.Move(new Vector2(-1, 0));
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Player.Move(new Vector2(1, 0));
        }
        else
        {
            Player.Move(new Vector2(0, 0));
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            Player.Jump();
        }
    }

    // TODO: Move to input handler


    void PlatformLanded(Collision2D collider)
    {
        if (collider.gameObject.tag == "Ground")
            Player.transform.SetParent(collider.transform);
    }

    void PlatformLeft(Collision2D collider)
    {
        if (collider.gameObject.tag == "Ground")
            Player.transform.SetParent(null);
    }

    void TriggerEnter(Collider2D collider)
    {
        if(collider.gameObject.tag == "PowerUp")
        {
            // TODO: add points
            collider.GetComponent<CoinScript>().Disable();
        }
    }

    void SpawnPlayers_()
    {
        // TODO: Set heights
        // TODO: Set jump power
        Player.SetJumpModifier(0.82f);
        Player.AddMovementCallbacks(PlatformLanded, PlatformLeft);
        Player.AddTriggerCallbacks(TriggerEnter, null);
    }
}
