using System;
using UnityEngine;

public class VehicleSelectionController : MonoBehaviour
{
    [Header("Images")]
    public Sprite[] BaseImages;
    public Sprite[] LeftWheelImages;
    public Sprite[] RightWheelImages;
    public Sprite[] BackWheelsImages;
    public Sprite[] GlassImages;
    public Sprite[] FrameImages;

    [Header("Collider")]
    public Vector2[] ColliderSize;
    public Vector2[] ColliderOffset;

    [Header("Trigger")]
    public Vector2[] TriggerSize;
    public Vector2[] TriggerOffset;

    [Header("Size")]
    public Vector3[] CarSize;

    [Header("Attributes")]
    public float[] MaxSpeeds;
    public float[] TurnPower;
    public float[] Acceleration;

    [Header("Names")]
    public string[] CarNames;

    [Header("Skids")]
    public bool[] SingleSkidmark;

    [Header("Propulsion")]
    public Vector2[] PropulsionSize;

    [Header("Selection")]
    public GameObject VehicleSelectionUI;
    public VehicleSelectionDisplay[] VehicleSelectionDisplays;

    bool _selectionActive = false;

    /// <summary>
    /// Updates the vehicle display for specified display
    /// </summary>
    /// <param name="playerIndex">The index of the player to update</param>
    /// <param name="direction">Direction to move in</param>
    public void UpdateDisplay(int playerIndex, int direction)
    {
        VehicleSelectionDisplays[playerIndex].Move(direction);
        UpdateDisplay_(playerIndex);
    }

    // Called once when the script starts
    private void Start()
    {
        for(int i = 0; i < VehicleSelectionDisplays.Length; i++)
        {
            UpdateDisplay(i, 0);
        }
    }

    /// <summary>
    /// Updates the vehicle display for specified display
    /// </summary>
    /// <param name="playerIndex">The index of the player to update</param>
    void UpdateDisplay_(int playerIndex)
    {
        var index = VehicleSelectionDisplays[playerIndex].VehicleIndex();
        VehicleSelectionDisplays[playerIndex].TxtVehicleType.text = CarNames[index];

        var car = CartAttackController.Instance.Cars[playerIndex];

        // update car
        car.BaseRenderer.sprite = BaseImages[index];
        car.BodyRenderer.sprite = FrameImages[index];
        car.LeftWheelRenderer.sprite = LeftWheelImages[index];
        car.RightWheelRenderer.sprite = RightWheelImages[index];
        car.BackWheelsRenderer.sprite = BackWheelsImages[index];
        car.GlassRenderer.sprite = GlassImages[index];

        // update stats
        car.MaxSpeed = MaxSpeeds[index];
        car.TurnFactor = TurnPower[index];
        car.AccelerationFactor = Acceleration[index];

        // update colliders
        car.TriggerCollider.offset = TriggerOffset[index];
        car.TriggerCollider.size = TriggerSize[index];
        car.CollisionCollider.offset = ColliderOffset[index];
        car.CollisionCollider.size = ColliderSize[index];

        // skidmarks
        car.Trails[0].gameObject.SetActive(!SingleSkidmark[index]);
        car.Trails[1].gameObject.SetActive(!SingleSkidmark[index]);
        car.Trails[2].gameObject.SetActive(SingleSkidmark[index]);

        // propulsion
        car.RocketBooster.localScale = PropulsionSize[index];

        // update UI
        VehicleSelectionDisplays[playerIndex].AttributeImages[0].fillAmount = (MaxSpeeds[index] / Mathf.Max(MaxSpeeds));
        VehicleSelectionDisplays[playerIndex].AttributeImages[1].fillAmount = (Acceleration[index] / Mathf.Max(Acceleration));
        VehicleSelectionDisplays[playerIndex].AttributeImages[2].fillAmount = (TurnPower[index] / Mathf.Max(TurnPower));
    }

    /// <summary>
    /// The player vehicle selection is no longer complete
    /// </summary>
    /// <param name="index">The index of the player who went back</param>
    internal void Incomplete(int index)
    {
        VehicleSelectionDisplays[index].ImgReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// The player vehicle selection is now complete
    /// </summary>
    /// <param name="index">The index of the player who completed</param>
    internal void Complete(int index)
    {
        VehicleSelectionDisplays[index].ImgReady.gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets whether the selection is active
    /// </summary>
    /// <param name="state">If the UI is active</param>
    public void SetActiveState(bool state)
    {
        _selectionActive = state;
    }

    /// <summary>
    /// Gets whether the selection is active
    /// </summary>
    /// <returns>If the UI is active</returns>
    public bool GetActiveState()
    {
        return _selectionActive;
    }
}
