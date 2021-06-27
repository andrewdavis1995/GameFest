using UnityEngine;

/// <summary>
/// Collection of functions that are required for all controllers
/// </summary>
public class GenericController : MonoBehaviour
{
    public virtual bool CanPause() { return true; }
}
