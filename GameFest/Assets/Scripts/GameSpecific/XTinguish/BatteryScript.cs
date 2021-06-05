using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryScript : MonoBehaviour
{
    public Sprite[] BatteryImages;
    public SpriteRenderer Renderer;

    private int _value = 0;

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0, 0, 0.5f);
    }

    public void Initialise(int value)
    {
        _value = value;
        // TODO: set sprite
    }
}
