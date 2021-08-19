using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeachBowlesController : MonoBehaviour
{
    public Rigidbody2D Ball;
    public BowlsBallScript BallScript;
    public Transform Arrow;

    const float SWING_AMOUNT = 40f;
    const float SWING_SPEED = 1.1f;
    const float SWING_DELAY = 0.01f;
    const float FIRE_FORCE = 3000;

    const float DRAG_SKY = 0.7f;
    const float DRAG_GROUND = 1.4f;
    const float MOVE_FREEDOM = 13;

    bool _selectingDirection = false;
    bool _selectingDistance = false;
    bool _overarm = false;

    private List<BowlZoneScript> _activeZones = new List<BowlZoneScript>();
    public static BeachBowlesController Instance;

    private Vector2 _ballLocation = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        _ballLocation = Ball.transform.localPosition;
        Reset();
    }

    internal void BallStopped()
    {
        Debug.Log(_activeZones.Count + " - " + _activeZones.LastOrDefault()?.PointValue);
        Reset();
    }

    private void Reset()
    {
        _overarm = false;
        _activeZones.Clear();
        Ball.transform.localPosition = _ballLocation;
        Arrow.localScale = new Vector3(1, 1, 1);
        Arrow.eulerAngles = new Vector3(0, 0, 0);
        Ball.transform.eulerAngles = new Vector3(0, 0, 0);
        StartCoroutine(SelectDirection());
    }

    private IEnumerator SelectDirection()
    {
        Arrow.eulerAngles = new Vector3(0, 0, 0);
        Arrow.gameObject.SetActive(true);
        _selectingDirection = true;

        float eulerAngles = 0;

        while (_selectingDirection)
        {
            for (var i = eulerAngles; i < SWING_AMOUNT && _selectingDirection; i++)
            {
                Arrow.eulerAngles += new Vector3(0, 0, SWING_SPEED);
                yield return new WaitForSeconds(SWING_DELAY);
            }
            eulerAngles = SWING_AMOUNT;
            for (var i = eulerAngles; i > -SWING_AMOUNT && _selectingDirection; i--)
            {
                Arrow.eulerAngles -= new Vector3(0, 0, SWING_SPEED);
                yield return new WaitForSeconds(SWING_DELAY);
            }
            eulerAngles = -SWING_AMOUNT;
        }

        StartCoroutine(SelectDistance());
    }

    private IEnumerator SelectDistance()
    {
        _selectingDistance = true;

        while (_selectingDistance)
        {
            for (var i = 1f; i > 0.1f && _selectingDistance; i -= 0.01f)
            {
                Arrow.localScale = new Vector3(1, i, 1);
                yield return new WaitForSeconds(SWING_DELAY);
            }
            for (var i = 0.1f; i < 1 && _selectingDistance; i += 0.01f)
            {
                Arrow.localScale = new Vector3(1, i, 1);
                yield return new WaitForSeconds(SWING_DELAY);
            }
        }

        Arrow.gameObject.SetActive(false);
        StartCoroutine(Fire_());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpacePressed_();
        }
        if (Input.GetKeyDown(KeyCode.P) && _selectingDirection)
        {
            _overarm = !_overarm;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft_();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight_();
        }

        Camera.main.transform.localPosition = Ball.transform.localPosition - new Vector3(0, 0, 5);
    }

    private void SpacePressed_()
    {
        _selectingDirection = false;
        _selectingDistance = false;
    }

    private void MoveLeft_()
    {
        if (_selectingDirection && Ball.transform.localPosition.x > -MOVE_FREEDOM)
            Ball.transform.Translate(new Vector3(-0.1f, 0, 0));
    }

    private void MoveRight_()
    {
        if (_selectingDirection && Ball.transform.localPosition.x < MOVE_FREEDOM)
            Ball.transform.Translate(new Vector3(0.1f, 0, 0));
    }

    private IEnumerator Fire_()
    {
        Ball.transform.eulerAngles = Arrow.eulerAngles;
        yield return new WaitForSeconds(0.01f);
        Ball.AddRelativeForce(Vector2.up * FIRE_FORCE * Arrow.localScale.y);
        yield return new WaitForSeconds(0.2f);
        BallScript.Started(_overarm, Arrow.localScale.y);
    }

    public void ZoneEntered(BowlZoneScript zone)
    {
        _activeZones.LastOrDefault()?.BallLeft();
        _activeZones.Add(zone);
        zone.BallEntered();
    }

    public void ZoneLeft(BowlZoneScript zone)
    {
        _activeZones.Remove(zone);
    }
}
