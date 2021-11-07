using UnityEngine;

public class PlatformBase : MonoBehaviour
{
    [SerializeField]
    bool _isEnabled;

    public bool Enabled()
    {
        return _isEnabled;
    }

    public void Enabled(bool enabled)
    {
        _isEnabled = enabled;
    }
}
