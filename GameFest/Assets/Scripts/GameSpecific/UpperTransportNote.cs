using UnityEngine;

/// <summary>
/// Controls the movement of a note through the top transport
/// </summary>
public class UpperTransportNote : MonoBehaviour 
{
    float _noteResetX;
    bool _moving = false;

    const float SPEED = 2.5f;

    /// <summary>
    /// Starts the movement of the note to the specified point
    /// </summary>
    /// <param name="noteStartX">Where the note starts</param>
    /// <param name="noteResetX">Where to stop the movement</param>
    public void StartMovement(float noteStartX, float noteResetX)
    {
        transform.localPosition = new Vector3(noteStartX, transform.localPosition.y, transform.localPosition.z);
        _moving = true;
    }

    void Update()
    {
        if (_moving)
        {
            //transform.Translate(new Vector3(SPEED * Time.deltaTime, 0, 0));
            if (transform.localPosition.x < _noteResetX)
            {
                _moving = false;
            }
        }
    }
}
