using UnityEngine;

public class MarshmallowScript : MonoBehaviour
{
    float xPos = 0;
    Vector3 _angle;

    public int OffsetX;

    // Start is called before the first frame update
    void Start()
    {
        xPos = transform.position.x;
        _angle = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
        transform.eulerAngles = _angle;
    }
}
