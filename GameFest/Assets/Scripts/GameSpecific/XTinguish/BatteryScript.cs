using System;
using System.Collections;
using UnityEngine;

public class BatteryScript : MonoBehaviour
{
    private const float BATTERY_LIFE = 7.5f;

    // Unity configuration
    public Sprite[] BatteryImages;
    public SpriteRenderer Renderer;

    // status variables
    private int _value = 0;
    private Vector3 _size;

    private void Start()
    {
        _size = transform.localScale;
        transform.localScale = Vector3.zero;

        StartCoroutine(ShowBattery_());
    }

    /// <summary>on startup
    /// Grows the battery 
    /// </summary>
    private IEnumerator ShowBattery_()
    {
        while(transform.localScale.x < _size.x)
        {
            transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
            yield return new WaitForSeconds(0.05f);
        }
        transform.localScale = _size;

        StartCoroutine(KillBattery_());
    }

    /// <summary>on startup
    /// Grows the battery 
    /// </summary>
    private IEnumerator KillBattery_()
    {
        yield return new WaitForSeconds(BATTERY_LIFE);

        // shrink and kill
        while (transform.localScale.x > 0)
        {
            transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
            yield return new WaitForSeconds(0.05f);
        }
        transform.localScale = _size;

        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // rotate the battery - rotation speed is quicker for higher value items
        transform.eulerAngles += new Vector3(0, 0, 0.2f + (_value/30f));
    }

    /// <summary>
    /// Sets the value and appearance of the battery
    /// </summary>
    /// <param name="value">The value of the battery (a multiple of 10 between 10 & 100)</param>
    public void Initialise(int value)
    {
        _value = value;
        Renderer.sprite = BatteryImages[((value) / 10)-1];
    }

    /// <summary>
    /// Gets the value of the battery - how much charge is left
    /// </summary>
    /// <returns>The value of the battery</returns>
    internal int GetValue()
    {
        return _value;
    }
}
