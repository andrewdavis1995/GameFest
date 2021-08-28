using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script for the ball in Beach Bowles
/// </summary>
public class BowlsBallScript : MonoBehaviour
{
    // status variables
    bool _running = false;
    float _shadowOffsetY;
    Vector2 _windStrength;

    // constant values for drag
    const float DRAG_AIRBORNE = 0.35f;
    const float DRAG_GROUNDED = 0.6f;
    const float WIND_INTERVAL = 0.1f;

    // constant values for appearance
    const float SHADOW_COLOUR_CHANGE = 0.007f;

    // Unity objects
    public Rigidbody2D Body;
    public Transform BallShadow;
    public SpriteRenderer BallShadowSprite;
    public SpriteRenderer Ball;

    // Called upon startup
    private void Start()
    {
        _shadowOffsetY = BallShadowSprite.transform.localPosition.y;
    }

    /// <summary>
    /// Called once each frame
    /// </summary>
    private void Update()
    {
        // make the ball face the correct direction
        if (_running)
        {
            Vector2 dir = Body.velocity;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
            Ball.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        BallShadow.transform.rotation = Quaternion.Euler(0.0f, 0.0f, gameObject.transform.rotation.z * -1f);

        // if the ball is moving and slows to below specified speed, stop the ball
        if (Math.Abs(Body.velocity.y) < 0.2f && _running)
        {
            _running = false;
            Body.velocity = Vector3.zero;

            // tell the controller that the ball stopped
            BeachBowlesController.Instance.BallStopped();
        }

        // stop wind when slows enough
        if (Math.Abs(Body.velocity.y) < 1.5f && _running)
        {
            DecreaseWind_(2);
        }
    }

    /// <summary>
    /// Put the ball back to its original state
    /// </summary>
    public void ResetBall()
    {
        Ball.transform.eulerAngles = Vector3.zero;
    }

    /// <summary>
    /// Called when the ball collides with an object with a trigger
    /// </summary>
    /// <param name="collision">The object it collided with</param>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // if it is a zone area, tell the controller that the ball entered the zone
        if (collision.tag == "AreaTrigger")
        {
            var zone = collision.gameObject.GetComponent<BowlZoneScript>();
            BeachBowlesController.Instance.ZoneEntered(zone);
        }
    }

    /// <summary>
    /// Called when the ball stops colliding with an object with a trigger
    /// </summary>
    /// <param name="collision">The object it stopped colliding with</param>
    public void OnTriggerExit2D(Collider2D collision)
    {
        // if it is a zone area, tell the controller that the ball left the zone
        if (collision.tag == "AreaTrigger")
        {
            var zone = collision.gameObject.GetComponent<BowlZoneScript>();
            BeachBowlesController.Instance.ZoneLeft(zone);
        }
    }

    /// <summary>
    /// Called when the ball collides with an object
    /// </summary>
    /// <param name="collision">The object it stopped colliding with</param>
    public void OnCollisionEnter2D(Collision2D collision)
    {
        // if it is a zone area, tell the controller that the ball left the zone
        if (collision.gameObject.tag == "Stick")
        {
            // centre stick
            if (collision.gameObject.name == "Centre Stick")
                BeachBowlesController.Instance.CentreStickHit();

            // stick of double points
            else if (collision.gameObject.name == "Stick of Double Points")
                BeachBowlesController.Instance.StickOfDoublePointsHit();
        }
    }

    /// <summary>
    /// Called when the ball is throw
    /// </summary>
    /// <param name="overarm">Whether the ball was thrown overarm</param>
    /// <param name="power">The power the ball was thrown with</param>
    /// <param name="windDirection">The strength of the wind</param>
    internal void Started(bool overarm, float power, Vector2 windDirection)
    {
        _running = true;
        _windStrength = windDirection;

        // if the ball was overarm, we need to control the shadow and the size of the ball
        if (overarm)
            StartCoroutine(ControlHeight(power));
        else
            DecreaseWind_(3);

        StartCoroutine(WindMovement_());
    }

    /// <summary>
    /// Wind is less strong when ball is on ground
    /// </summary>
    private void DecreaseWind_(int factor)
    {
        _windStrength /= factor;
    }

    /// <summary>
    /// Moves the ball as the "wind" blows it
    /// </summary>
    private IEnumerator WindMovement_()
    {
        // continue while running/rolling
        while (_running)
        {
            yield return new WaitForSeconds(WIND_INTERVAL);
            Body.AddForce(_windStrength * 50 * new Vector2(1, -1));
        }
    }

    /// <summary>
    /// Controls the size of the ball and shadow while it is "in the air"
    /// </summary>
    /// <param name="power">The power with which the ball is thrown</param>
    private IEnumerator ControlHeight(float power)
    {
        // reduce drag while in the air
        Body.drag = DRAG_AIRBORNE;

        var maxOffset = power * -2.5f;

        // ball goes up and shadow moves away
        for (var i = _shadowOffsetY; i > maxOffset; i -= 0.05f)
        {
            MoveBallHeight_(i, 0.01f, -0.005f, -SHADOW_COLOUR_CHANGE);
            yield return new WaitForSeconds(0.003f);
        }

        // ball comes down and shadow moves closer
        for (var i = maxOffset; i < _shadowOffsetY; i += 0.05f)
        {
            MoveBallHeight_(i, -0.01f, 0.005f, SHADOW_COLOUR_CHANGE);
            yield return new WaitForSeconds(0.007f);
        }

        // little bounce
        // move up
        for (var i = _shadowOffsetY; i > maxOffset / 2.7f; i -= 0.05f)
        {
            MoveBallHeight_(i, 0.01f, -0.005f, -SHADOW_COLOUR_CHANGE);
            yield return new WaitForSeconds(0.003f);
        }

        // move down
        for (var i = maxOffset / 2.7f; i < _shadowOffsetY; i += 0.05f)
        {
            MoveBallHeight_(i, -0.01f, 0.005f, SHADOW_COLOUR_CHANGE);
            yield return new WaitForSeconds(0.007f);
        }

        // increase drag now that we are on the ground
        Body.drag = DRAG_GROUNDED;

        // wind has less affect
        DecreaseWind_(3);
    }

    /// <summary>
    /// Changes appearance of the ball and the shadow to match the "height" of the ball
    /// </summary>
    /// <param name="shadowOffset">How far the shadow should be from the ball</param>
    /// <param name="ballScaleOffset">How much to increase the size of the ball by (can be negative)</param>
    /// <param name="shadowScaleOffset">How much to increase the size of the shadow by (can be negative)</param>
    /// <param name="shadowColourChange">How much to change the alpha value of the colour of the shadow by (can be negative)</param>
    void MoveBallHeight_(float shadowOffset, float ballScaleOffset, float shadowScaleOffset, float shadowColourChange)
    {
        BallShadowSprite.color = new Color(0, 0, 0, BallShadowSprite.color.a + shadowColourChange);
        BallShadowSprite.transform.localScale += new Vector3(shadowScaleOffset, shadowScaleOffset, 0);
        Ball.transform.localScale -= new Vector3(ballScaleOffset, ballScaleOffset, 0);
        BallShadowSprite.transform.localPosition = new Vector3(0, shadowOffset, 0.5f);
    }
}
