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
}
