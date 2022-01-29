using System.Collections;
using UnityEngine;

public enum OnOffPlatformState { Pause, Right, Left }

public class OnOffPlatformScript : PlatformBase
{
    public float LeftX;
    public float RightX;
    public float Speed;
    public float Delay;

    public OnOffPlatformState State = OnOffPlatformState.Pause;

    // Update is called once per frame
    void Update()
    {
        if (!Enabled()) return;

        if (State == OnOffPlatformState.Right)
        {
            transform.Translate(new Vector3(Speed * Time.deltaTime, 0, 0));

            if (transform.localPosition.x > RightX)
                StartCoroutine(SetState_(OnOffPlatformState.Left));
        }
        if (State == OnOffPlatformState.Left)
        {
            transform.Translate(new Vector3(-Speed * Time.deltaTime, 0, 0));

            if (transform.localPosition.x < LeftX)
                StartCoroutine(SetState_(OnOffPlatformState.Right));
        }
    }

    IEnumerator SetState_(OnOffPlatformState state)
    {
        State = OnOffPlatformState.Pause;
        yield return new WaitForSeconds(Delay);
        State = state;
    }
}
