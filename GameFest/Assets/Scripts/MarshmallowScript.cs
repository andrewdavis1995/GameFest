using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarshmallowScript : MonoBehaviour
{
    float xPos = 0;
    Vector3 _angle;

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
