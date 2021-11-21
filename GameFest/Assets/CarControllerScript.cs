using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerScript : MonoBehaviour
{
    List<Tuple<Vector3, bool>> _lapDrawings = new List<Tuple<Vector3, bool>>();

    public float AccelerationFactor = 30.0f;
    public float TurnFactor = 3.5f;

    public float DriftFactor = 0.95f;
    public float MaxSpeed = 30f;
    public float MaxDrag = 3f;
    public float AccelerationSpeed = 3f;

    public float BoostFactor = 1.1f;

    float _accelerationInput = 0;
    float _steeringInput = 0;
    float _rotationAngle = 0;
    bool _boosting = false;
    int _checkpointIndex = 0;

    List<Collider2D> _outOfBoundsZone = new List<Collider2D>();

    Rigidbody2D carRigidBody;

    public TrailRenderer[] Trails;
    public TrailRenderer DrawTrail;

    private void Awake()
    {
        carRigidBody = GetComponent<Rigidbody2D>();
    }

    public void Boost()
    {
        if (!_boosting)
            StartCoroutine(Boost_());
    }

    IEnumerator Boost_()
    {
        _boosting = true;
        MaxSpeed *= BoostFactor;
        yield return new WaitForSeconds(2);
        _boosting = false;
        MaxSpeed /= BoostFactor;
    }

    private void FixedUpdate()
    {
        ApplyEngineForce_();
        KillOrthogonalVelocity_();
        ApplySteering_();
    }

    void ToggleTrail(bool state)
    {
        foreach (var t in Trails)
            t.emitting = state;
    }

    void ApplyEngineForce_()
    {
        float velocityVsUp = Vector2.Dot(transform.up, carRigidBody.velocity);

        if (velocityVsUp > MaxSpeed && _accelerationInput > 0)
            return;

        if ((velocityVsUp < -MaxSpeed * 0.5f) && _accelerationInput < 0)
            return;

        // adjust drag
        if (_accelerationInput == 0)
            carRigidBody.drag = Mathf.Lerp(carRigidBody.drag, MaxDrag, Time.fixedDeltaTime * AccelerationSpeed);
        else
            carRigidBody.drag = Mathf.Lerp(carRigidBody.drag, 0f, Time.fixedDeltaTime * 2);

        var engineForceVector = transform.up * _accelerationInput * AccelerationFactor;
        carRigidBody.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering_()
    {
        float minSpeed = (carRigidBody.velocity.magnitude / 8);
        minSpeed = Mathf.Clamp01(minSpeed);

        _rotationAngle -= (_steeringInput * TurnFactor * minSpeed);
        carRigidBody.MoveRotation(_rotationAngle);
    }

    public void SetSteeringValue(float steerValue)
    {
        _steeringInput = steerValue;
        var emit = (Math.Abs(_steeringInput) > 0.5f) && (_accelerationInput > 0.5f);
        ToggleTrail(emit);
    }

    public void SetAccelerationValue(float acceleration)
    {
        _accelerationInput = acceleration;
    }

    void KillOrthogonalVelocity_()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidBody.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidBody.velocity, transform.right);

        carRigidBody.velocity = forwardVelocity + rightVelocity * DriftFactor;
    }

    public void OutOfBoundsEntered(Collider2D collision)
    {
        _outOfBoundsZone.Add(collision);
    }

    public void OutOfBoundsExited(Collider2D collision)
    {
        _outOfBoundsZone.Remove(collision);
    }

    internal void LapComplete_()
    {
        _lapDrawings.Add(new Tuple<Vector3, bool>(new Vector3(), true));
        DrawTrail.Clear();
        _checkpointIndex = 0;
    }

    internal void NextCheckpoint(Collider2D collider)
    {
        if (CorrectCheckpoint_(collider))
        {
            _checkpointIndex++;

            if (collider.gameObject.tag == "Finish")
            {
                LapComplete_();
            }
        }
    }

    private bool CorrectCheckpoint_(Collider2D collider)
    {
        return (collider == CartAttackController.Instance.Checkpoints[_checkpointIndex]);
    }
}
