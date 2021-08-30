using System;
using System.Collections;
using UnityEngine;

public class SwooshController : MonoBehaviour
{
    public Vector3 CentrePoint;
    public Transform SwooshLogo;
    public Camera SwooshCamera;

    const float SIZE_INCREASE_RATE = 0.045f;
    const float MOVE_SPEED = -0.3f;
    const float DISTANCE = 35;

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
    /// <param name="callbackAction">Function to call once swoosh is complete</param>
    /// <param name="midpointAction">Function to call once swoosh is in the middle</param>
    public void DoSwoosh(Action callbackAction, Action midpointAction)
    {
        StartCoroutine(DoSwoosh_(callbackAction, midpointAction));
    }

    /// <summary>
    /// Controls the swoosh
    /// </summary>
    /// <param name="callbackAction">Function to call once swoosh is complete</param>
    /// <param name="midpointAction">Function to call once swoosh is in the middle</param>
    /// <param name="speed">Speed at which to move</param>
    IEnumerator DoSwoosh_(Action callbackAction, Action midpointAction, float speed = 0.001f)
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
            yield return new WaitForSeconds(speed);
        }

        // swap cameras
        midpointAction?.Invoke();

        // move and shrink away from the middle
        while (SwooshLogo.localPosition.x > endPoint.x)
        {
            SwooshLogo.transform.Translate(new Vector3(MOVE_SPEED, 0, 0));
            SwooshLogo.localScale -= new Vector3(SIZE_INCREASE_RATE, SIZE_INCREASE_RATE, 0);
            yield return new WaitForSeconds(speed);
        }

        SwooshLogo.gameObject.SetActive(false);
        SwooshCamera.enabled = false;
        callbackAction?.Invoke();
    }
}

// set up a 3rd camera specifically for the Swoosh
// all it does is point at the swoosh logo, and overlay it over the current scene (see YouTube tutorial with spinning cube)
