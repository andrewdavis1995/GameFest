using UnityEngine;

public class MovingPlatformHorizontal : PlatformBase
{
    public float LeftX;
    public float RightX;
    public float Speed;

    public bool MovingRight;

    private void Start()
    {
        Enabled(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Enabled()) return;

        if (MovingRight)
        {
            transform.Translate(new Vector3(Speed * Time.deltaTime, 0, 0));

            if (transform.localPosition.x > RightX)
                MovingRight = false;
        }
        else
        {
            transform.Translate(new Vector3(-Speed * Time.deltaTime, 0, 0));

            if (transform.localPosition.x < LeftX)
                MovingRight = true;
        }
    }
}
