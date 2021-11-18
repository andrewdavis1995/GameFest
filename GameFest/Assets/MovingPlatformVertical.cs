using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformVertical : PlatformBase
{
    public float UpperY;
    public float LowerY;
    public float Speed;

    public bool MovingUp;

    private void Start()
    {
        Enabled(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Enabled()) return;

        if (!CashDashController.Instance.IsActive() || PauseGameHandler.Instance.IsPaused()) return;

        if (MovingUp)
        {
            transform.Translate(new Vector3(0, Speed * Time.deltaTime, 0));

            if (transform.localPosition.y > UpperY)
                MovingUp = false;
        }
        else
        {
            transform.Translate(new Vector3(0, -Speed * Time.deltaTime, 0));

            if (transform.localPosition.y < LowerY)
                MovingUp = true;
        }
    }
}
