using UnityEngine;

public class CameraLerp : MonoBehaviour
{
    public float Zoom;
    public Vector3 Position;
    public float Speed;

    Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, Zoom, Time.fixedDeltaTime * Speed);
        var x = Mathf.Lerp(_cam.transform.localPosition.x, Position.x, Time.fixedDeltaTime * Speed);
        var y = Mathf.Lerp(_cam.transform.localPosition.y, Position.y, Time.fixedDeltaTime * Speed);
        _cam.transform.position = new Vector3(x, y, _cam.transform.position.z);
    }
}
