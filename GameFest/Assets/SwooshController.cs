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

    void Start()
    {
        _logoScale = SwooshLogo.localScale;
        SwooshLogo.gameObject.SetActive(false);
    }

    public void DoSwoosh(Camera camera1, Camera camera2, Action callbackAction)
    {
        StartCoroutine(DoSwoosh_(camera1, camera2, callbackAction));
    }

    IEnumerator DoSwoosh_(Camera camera1, Camera camera2, Action callbackAction)
    {
        SwooshCamera.enabled = true;
        SwooshLogo.localScale = _logoScale;
     
        var startPoint = CentrePoint + new Vector3(DISTANCE / 2, 0, 0);
        var endPoint = CentrePoint - new Vector3(DISTANCE / 2, 0, 0);

        SwooshLogo.localPosition = startPoint;
        SwooshLogo.gameObject.SetActive(true);

        while (SwooshLogo.localPosition.x > CentrePoint.x)
        {
            SwooshLogo.transform.Translate(new Vector3(MOVE_SPEED, 0, 0));
            SwooshLogo.localScale += new Vector3(SIZE_INCREASE_RATE, SIZE_INCREASE_RATE, 0);
            yield return new WaitForSeconds(0.01f);
        }

        camera1.enabled = false;
        camera2.enabled = true;

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
