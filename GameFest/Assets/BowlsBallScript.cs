using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlsBallScript : MonoBehaviour
{
    bool _running = false;
    public Rigidbody2D Body;
    public SpriteRenderer BallShadow;
    public PhysicsMaterial2D Airborne;
    public PhysicsMaterial2D Ground;

    float _shadowOffsetY;

    private void Start()
    {
        _shadowOffsetY = BallShadow.transform.localPosition.y;
    }

    private void Update()
    {
        if(Math.Abs(Body.velocity.y) < 0.1f && _running)
        {
            _running = false;
            Body.velocity = Vector3.zero;
            BeachBowlesController.Instance.BallStopped();
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "AreaTrigger")
        {
            var zone = collision.gameObject.GetComponent<BowlZoneScript>();
            BeachBowlesController.Instance.ZoneEntered(zone);
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "AreaTrigger")
        {
            var zone = collision.gameObject.GetComponent<BowlZoneScript>();
            BeachBowlesController.Instance.ZoneLeft(zone);
        }
    }

    internal void Started(bool overarm, float power)
    {
        _running = true;
        if(overarm)
        StartCoroutine(ControlHeight(power));
    }

    private IEnumerator ControlHeight(float power)
    {
        Body.drag = 0.35f;

        var maxOffset = power * -2.5f;

        for (var i = _shadowOffsetY; i > maxOffset; i-=0.05f)
        {
            BallShadow.transform.localPosition = new Vector3(0, i, 0);
            yield return new WaitForSeconds(0.003f);
        }

        for (var i = maxOffset; i < _shadowOffsetY; i+=0.05f)
        {
            BallShadow.transform.localPosition = new Vector3(0, i, 0);
            yield return new WaitForSeconds(0.007f);
        }

        for (var i = _shadowOffsetY; i > maxOffset/3; i -= 0.05f)
        {
            BallShadow.transform.localPosition = new Vector3(0, i, 0);
            yield return new WaitForSeconds(0.003f);
        }

        for (var i = maxOffset/3; i < _shadowOffsetY; i += 0.05f)
        {
            BallShadow.transform.localPosition = new Vector3(0, i, 0);
            yield return new WaitForSeconds(0.007f);
        }

        Body.sharedMaterial = Ground;
        Body.drag = 0.6f;
    }
}
