using System.Collections;
using UnityEngine;

public class CarControllerScript : MonoBehaviour
{
    public float AccelerationFactor = 30.0f;
    public float TurnFactor = 3.5f;
    public float DriftFactor = 0.95f;
    public float MaxSpeed = 20f;
    public float MaxDrag = 3f;

    public float BoostFactor = 2f;


    float _accelerationInput = 0;
    float _steeringInput = 0;
    float _rotationAngle = 0;
    bool _boosting = false;

    Rigidbody2D carRigidBody;

    private void Awake()
    {
        carRigidBody = GetComponent<Rigidbody2D>();
    }

    public void Boost()
    {
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

    void ApplyEngineForce_()
    {

        float _velocityVsUp = Vector2.Dot(transform.up, carRigidBody.velocity);
        Debug.Log(_velocityVsUp);

        if (_velocityVsUp > MaxSpeed && _accelerationInput > 0)
            return;

        if ((_velocityVsUp < -MaxSpeed *0.5f) && _accelerationInput < 0)
            return;

        // adjust drag
        if (_accelerationInput == 0)
            carRigidBody.drag = Mathf.Lerp(carRigidBody.drag, MaxDrag, Time.fixedDeltaTime * 3);
        else
            carRigidBody.drag = Mathf.Lerp(carRigidBody.drag, 0f, Time.fixedDeltaTime * 3);



        var engineForceVector = transform.up * _accelerationInput * AccelerationFactor;
        carRigidBody.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering_()
    {
        float minSpeed = (carRigidBody.velocity.magnitude / 8);
        minSpeed = Mathf.Clamp01(minSpeed);

        _rotationAngle -= _steeringInput * TurnFactor * minSpeed;
        carRigidBody.MoveRotation(_rotationAngle);
    }

    public void SetSteeringValue(float steering)
    {
        _steeringInput = steering;
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
}
