using System.Collections;
using UnityEngine;

public class CameraShakeScript : MonoBehaviour
{
    Vector3 originalPos;
    float shakeAmount = 0.03f;
    bool _enabled = false;

    /// <summary>
    /// Does the shudder
    /// </summary>
    IEnumerator ControlShudder_()
    {
        originalPos = transform.localPosition;
        // Move the camera
        while (_enabled)
        {
            transform.localPosition = originalPos + new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount));
            yield return new WaitForSeconds(0.025f);
        }
    }

    /// <summary>
    /// Enable the shake
    /// </summary>
    public void Enable()
    {
        _enabled = true;
        StartCoroutine(ControlShudder_());
    }

    /// <summary>
    /// Disable the shake
    /// </summary>
    public void Disable()
    {
        _enabled = false;
        transform.localPosition = originalPos;
    }
}