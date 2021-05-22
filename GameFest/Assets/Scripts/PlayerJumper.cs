using System;
using UnityEngine;

/// <summary>
/// Controls the movement of a player
/// </summary>
public class PlayerJumper : MonoBehaviour
{
    Transform _platform;
    public PlayerAnimation Animator;

    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        transform.eulerAngles = new Vector3(0, 0, 0);

        if(_platform != null)
        {
            transform.position = new Vector3(_platform.position.x, transform.position.y, transform.position.z);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void Jump()
    {
        gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(50, 55));
        Animator.SetAnimation("Jump");
    }

    public void Detach_()
    {
        _platform = null;
        transform.SetParent(null);
    }

    public void Attach_(Transform platform)
    {
        _platform = platform;
        transform.SetParent(platform);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Attach_(collision.transform);

            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            //transform.position = new Vector3(0, transform.position.y, -1);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Detach_();
        }
    }
}
