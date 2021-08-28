using System;
using System.Collections;
using UnityEngine;

public class SwooshController : MonoBehaviour
{
    public Vector3 CentrePoint;
    public Transform SwooshLogo;
    public Camera SwooshCamera;

    const float SIZE_INCREASE_RATE = 0.085f;
    const float MOVE_SPEED = -0.15f;
    const float DISTANCE = 30;

    Vector3 _logoScale;

    // Called when the script starts
    void Start()
    {
        _logoScale = SwooshLogo.localScale;
        SwooshLogo.gameObject.SetActive(false);
    }

    /// <summary>
    /// Triggers the start of the swoosh
    /// </summary>
    /// <param name="camera1">The camera that is currently active</param>
    /// <param name="camera2">The camera that is active after the logo swoosh completes</param>
    /// <param name="callbackAction">Function to call once swoosh is complete</param>
    public void DoSwoosh(Camera camera1, Camera camera2, Action callbackAction)
    {
        StartCoroutine(DoSwoosh_(camera1, camera2, callbackAction));
    }

    /// <summary>
    /// Controls the swoosh
    /// </summary>
    /// <param name="camera1">The camera that is currently active</param>
    /// <param name="camera2">The camera that is active after the logo swoosh completes</param>
    /// <param name="callbackAction">Function to call once swoosh is complete</param>
    IEnumerator DoSwoosh_(Camera camera1, Camera camera2, Action callbackAction)
    {
        SwooshCamera.enabled = true;
        SwooshLogo.localScale = _logoScale;
     
        // get the start point and end point for the logo
        var startPoint = CentrePoint + new Vector3(DISTANCE / 2, 0, 0);
        var endPoint = CentrePoint - new Vector3(DISTANCE / 2, 0, 0);

        // initialise the image
        SwooshLogo.localPosition = startPoint;
        SwooshLogo.gameObject.SetActive(true);

        // move and grow to the middle
        while (SwooshLogo.localPosition.x > CentrePoint.x)
        {
            SwooshLogo.transform.Translate(new Vector3(MOVE_SPEED, 0, 0));
            SwooshLogo.localScale += new Vector3(SIZE_INCREASE_RATE, SIZE_INCREASE_RATE, 0);
            yield return new WaitForSeconds(0.01f);
        }

        // swap cameras
        camera1.enabled = false;
        camera2.enabled = true;

        // move and shrink away from the middle
        while (SwooshLogo.localPosition.x > endPoint.x)
        {
            SwooshLogo.transform.Translate(new Vector3(MOVE_SPEED, 0, 0));
            SwooshLogo.localScale -= new Vector3(SIZE_INCREASE_RATE, SIZE_INCREASE_RATE, 0);
            yield return new WaitForSeconds(0.01f);
        }

        SwooshLogo.gameObject.SetActive(false);
        SwooshCamera.enabled = false;
        callbackAction?.Invoke();
    }
}

// set up a 3rd camera specifically for the Swoosh
// all it does is point at the swoosh logo, and overlay it over the current scene (see YouTube tutorial with spinning cube)
