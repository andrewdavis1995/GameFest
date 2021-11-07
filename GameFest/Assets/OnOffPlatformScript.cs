using System.Collections;
using UnityEngine;

public class OnOffPlatformScript : PlatformBase
{
    public float LeftX;
    public float RightX;
    public float Speed;

    public bool? MovingRight = false;

    // Update is called once per frame
    void Update()
    {
        if (!Enabled()) return;

        if (MovingRight == true)
        {
            transform.Translate(new Vector3(Speed * Time.deltaTime, 0, 0));

            if (transform.localPosition.x > RightX)
                StartCoroutine(SetState_(false));
        }
        else if (MovingRight == false)
        {
            transform.Translate(new Vector3(-Speed * Time.deltaTime, 0, 0));

            if (transform.localPosition.x < LeftX)
                StartCoroutine(SetState_(true));
        }
    }

    IEnumerator SetState_(bool state)
    {
        MovingRight = null;
        yield return new WaitForSeconds(2.5f);
        MovingRight = state;
    }
}
