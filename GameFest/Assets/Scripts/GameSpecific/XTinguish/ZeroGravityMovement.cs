using UnityEngine;

public class ZeroGravityMovement : MonoBehaviour
{
    const float MAX_SPEED = 4.5f;
    const float PROPULSION_FORCE = 20f;

    // links to unity objects
    Rigidbody2D _rigidBody;
    [SerializeField]
    SpriteRenderer _spaceman;
    [SerializeField]
    SpriteRenderer _extinguisher;

    float _xMovement = 0;

    // status
    float _health = 100f;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    // Called once per frame
    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
            _rigidBody.AddForce(new Vector3(0, PROPULSION_FORCE, 0));

        bool moving = false;

        if (Input.GetKey(KeyCode.RightArrow))
        {
            moving = true;
            _xMovement += 0.1f;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moving = true;
            _xMovement -= 0.1f;
        }

        if (_xMovement > MAX_SPEED) _xMovement = MAX_SPEED;
        if (_xMovement < -MAX_SPEED) _xMovement = -MAX_SPEED;

        if (!moving)
        {
            if (_xMovement > 0) _xMovement -= 0.1f;
            else if (_xMovement < 0) _xMovement += 0.1f;
        }
        else
        {
            _spaceman.flipX = _xMovement < 0;
            _extinguisher.flipX = _xMovement < 0;
        }

        if (_rigidBody.velocity.y > 0.1f || _rigidBody.velocity.y < -0.1f)
        {
            transform.Translate(new Vector3(_xMovement * Time.deltaTime, 0));
        }
        _spaceman.transform.eulerAngles = new Vector3(0, 0, -_xMovement * 7f);
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
