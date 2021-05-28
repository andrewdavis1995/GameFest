using UnityEngine;

public class ZeroGravityMovement : MonoBehaviour
{
    Rigidbody2D _rigidbody;
    float _angle = 0;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        transform.eulerAngles = new Vector3(0, 0, _angle);

        if (Input.GetKey(KeyCode.Space))
            _rigidbody.AddRelativeForce(new Vector3(2.2f, 0, 0));

        if (Input.GetKey(KeyCode.RightArrow))
            _angle -= 1f;

        if (Input.GetKey(KeyCode.LeftArrow))
            _angle += 1f;
    }
}
