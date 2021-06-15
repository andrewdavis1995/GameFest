using UnityEngine;

public class BatteryScript : MonoBehaviour
{
    // Unity configuration
    public Sprite[] BatteryImages;
    public SpriteRenderer Renderer;

    // status variables
    private int _value = 0;

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
