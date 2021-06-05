using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimber : MonoBehaviour
{
    [SerializeField]
    bool _onSlope;
    bool _onGround;

    [SerializeField]
    float MOVE_SPEED = 7;
    [SerializeField]
    float JUMP_FORCE = 200;

    [SerializeField]
    PhysicsMaterial2D StaticMaterial;

    CapsuleCollider2D _collider;
    Vector2 _colliderSize;

    [SerializeField]
    float _slopeCheckDistance = 0.5f;
    float _slopeSideAngle;

    public LayerMask WhatIsGround;

    float _slopeDownAngle;
    float _slopeDownAngleOld;
    Vector2 _slopeNormalPerp;

    Rigidbody2D _rigidbody;

    Vector2 _newVelocity = new Vector2();

    float _movementX = 0;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        _collider = GetComponent<CapsuleCollider2D>();
        _colliderSize = _collider.size * transform.localScale;
    }

    void SlopeCheck_()
    {
        Vector2 checkPos = transform.position - new Vector3(0, _colliderSize.y / 2);
        SlopeCheckHorizontal_(checkPos);
        SlopeCheckVertical_(checkPos);
    }

    void SlopeCheckHorizontal_(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, _slopeCheckDistance, WhatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, _slopeCheckDistance, WhatIsGround);

        if(slopeHitFront)
        {
            _onSlope = true;
            _slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if(slopeHitBack)
        {
            _onSlope = true;
            _slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            _onSlope = false;
            _slopeSideAngle = 0f;
        }
    }

    void Jump()
    {
        if (_onGround)
        {
            _onGround = false;
            _rigidbody.AddForce(new Vector2(0, JUMP_FORCE));
        }
    }

    void SlopeCheckVertical_(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, _slopeCheckDistance, WhatIsGround);

        if(hit)
        {
            _slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            _slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            //if (_slopeDownAngle != _slopeDownAngleOld)
                _onSlope = true;

            _slopeDownAngleOld = _slopeDownAngle;

            Debug.DrawRay(hit.point, _slopeNormalPerp, Color.red);
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }

        if (_onGround && _movementX == 0f)
            _rigidbody.sharedMaterial = StaticMaterial;
        else
            _rigidbody.sharedMaterial = null;

    }

    // Update is called once per frame
    void Update()
    {
        bool moving = false;

        //CheckGround_();
        SlopeCheck_();
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _movementX = MOVE_SPEED;
            moving = true;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _movementX = -MOVE_SPEED;
            moving = true;
        }

        if (!moving)
            _movementX = 0;

        Move();

        if (Input.GetKey(KeyCode.Space) && _onGround)
        {
            Jump();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground") _onGround = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground") _onGround = false;
    }

    private void Move()
    {
        if(_onGround && !_onSlope)
        {
            _newVelocity.Set(_movementX, 0);
        }
        else if (_onGround && _onSlope)
        {
            _newVelocity.Set(-_movementX * _slopeNormalPerp.x, -_movementX * _slopeNormalPerp.y);
        }
        else if (!_onGround)
        {
            _newVelocity.Set(_movementX, _rigidbody.velocity.y);
        }

        _rigidbody.velocity = _newVelocity;
    }
}
