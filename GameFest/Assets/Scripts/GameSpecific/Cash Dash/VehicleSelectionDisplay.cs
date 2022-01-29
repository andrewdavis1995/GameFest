using UnityEngine;
using UnityEngine.UI;

public class VehicleSelectionDisplay : MonoBehaviour
{
    const int NUM_VEHICLES = 3;

    public Text TxtPlayerName;
    public Text TxtVehicleType;

    public Image[] AttributeImages;
    public Image ImgReady;

    int _vehicleIndex = 0;

    /// <summary>
    /// Moves to the next/previous vehicle
    /// </summary>
    /// <param name="direction">The direction to move in</param>
    public void Move(int direction)
    {
        _vehicleIndex += direction;

        // check bounds
        if (_vehicleIndex < 0)
            _vehicleIndex = NUM_VEHICLES - 1;

        if (_vehicleIndex >= NUM_VEHICLES)
            _vehicleIndex = 0;
    }

    /// <summary>
    /// Gets the index of the vehicle in use
    /// </summary>
    /// <returns>Index of the vehicle</returns>
    public int VehicleIndex()
    {
        return _vehicleIndex;
    }
}
