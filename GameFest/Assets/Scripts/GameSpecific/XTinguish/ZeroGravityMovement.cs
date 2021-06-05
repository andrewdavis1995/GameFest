using UnityEngine;

public class ZeroGravityMovement : MonoBehaviour
{
    // links to unity objects
    //Rigidbody2D _spacemanRigidBody;
    Rigidbody2D _rigidBody;
    public Transform Spaceman;

    // status
    float _angle = 0;
    float _health = 100f;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    // Called once per frame
    private void Update()
    {
        transform.eulerAngles = new Vector3(0, 0, _angle);
        Spaceman.position = transform.position;

        if (Input.GetKey(KeyCode.Space))
            _rigidBody.AddRelativeForce(new Vector3(2.2f, 0, 0));

        //if (Input.GetKey(KeyCode.RightArrow))
        //    _angle -= 1f;

        //if (Input.GetKey(KeyCode.LeftArrow))
        //    _angle += 1f;
    }

    /// <summary>
    /// When the player enters a trigger
    /// </summary>
    /// <param name="collision">The object the player collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);
    }
}
