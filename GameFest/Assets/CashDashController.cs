using UnityEngine;

public class CashDashController : MonoBehaviour
{
    public PlayerMovement Player;

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

    void SpawnPlayers_()
    {

    }
}
