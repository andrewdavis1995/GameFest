using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour
{
    static float SPEED = 10f;

    private void Update()
    {
        transform.Translate(Vector2.up * Time.deltaTime * SPEED);
    }
}
